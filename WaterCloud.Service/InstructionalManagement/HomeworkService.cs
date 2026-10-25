using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 作业管理服务（教师发布作业、学生在线/拍照提交、教师批改）
    /// </summary>
    public class HomeworkService : BaseService<HomeworkEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色Id
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public HomeworkService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 当前登录用户是否为学生
        /// </summary>
        public bool IsStudent
        {
            get
            {
                var roleIds = (currentuser.RoleId ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (roleIds.Contains(StudentRoleId))
                {
                    return true;
                }
                if (roleIds.Length == 0)
                {
                    return false;
                }
                var names = repository.Db.Queryable<RoleEntity>()
                    .Where(a => roleIds.Contains(a.F_Id))
                    .Select(a => a.F_FullName)
                    .ToList();
                return names.Any(a => !string.IsNullOrEmpty(a) && a.Contains("学生"));
            }
        }

        #region 基础数据

        /// <summary>
        /// 课程列表（课程管理中的课程）
        /// </summary>
        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        /// <summary>
        /// 班级列表
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_EnCode)
                .ToListAsync();
        }

        /// <summary>
        /// 当前登录学生所属班级（取第一个班级类型部门）
        /// </summary>
        public async Task<string> GetMyClassId()
        {
            var organizeId = currentuser.OrganizeId;
            if (string.IsNullOrEmpty(organizeId) && !string.IsNullOrEmpty(currentuser.UserId))
            {
                organizeId = await repository.Db.Queryable<UserEntity>()
                    .Where(a => a.F_Id == currentuser.UserId)
                    .Select(a => a.F_OrganizeId)
                    .FirstAsync();
            }
            if (string.IsNullOrEmpty(organizeId))
            {
                return null;
            }
            var orgIds = organizeId.Split(',').Select(a => a.Trim()).Where(a => a.Length > 0).ToList();
            if (orgIds.Count == 0)
            {
                return null;
            }
            var classList = await GetClassList();
            foreach (var orgId in orgIds)
            {
                var match = classList.FirstOrDefault(a => string.Equals(a.F_Id, orgId, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match.F_Id;
                }
            }
            return null;
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 作业列表（教师视角；学生仅能看到本班已发布作业）
        /// </summary>
        public async Task<List<HomeworkExtend>> GetLookList(SoulPage<HomeworkExtend> pagination, string keyword, string courseId, string classId, string status)
        {
            var allowed = new HashSet<string> { "F_Title", "F_CourseName", "F_ClassName", "F_Deadline", "F_Status", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var studentOnly = IsStudent;
            var myClassId = studentOnly ? await GetMyClassId() : null;
            var sql = BuildQuerySql(keyword, courseId, classId, status, studentOnly, myClassId);
            return await repository.Db.SqlQueryable<HomeworkExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildQuerySql(string keyword, string courseId, string classId, string status, bool studentOnly, string myClassId)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*,
       c.F_Name AS F_CourseName,
       o.F_FullName AS F_ClassName,
       (SELECT COUNT(1) FROM school_homework_submit s WHERE s.F_HomeworkId = a.F_Id COLLATE Chinese_PRC_CI_AS AND ISNULL(s.F_DeleteMark,0) = 0) AS F_SubmitCount,
       (SELECT COUNT(1) FROM sys_user u WHERE ISNULL(u.F_DeleteMark,0) = 0 AND ISNULL(u.F_EnabledMark,0) = 1
            AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE ('%,' + a.F_ClassId COLLATE Chinese_PRC_CI_AS + ',%')
            AND (',' + ISNULL(u.F_RoleId,'') + ',') LIKE '%," + StudentRoleId + @",%') AS F_TotalCount,
       (SELECT COUNT(1) FROM school_homework_submit s2 WHERE s2.F_HomeworkId = a.F_Id COLLATE Chinese_PRC_CI_AS AND ISNULL(s2.F_DeleteMark,0) = 0 AND ISNULL(s2.F_Status,0) = 0) AS F_PendingCount,
       (SELECT COUNT(1) FROM school_homework_submit s3 WHERE s3.F_HomeworkId = a.F_Id COLLATE Chinese_PRC_CI_AS AND ISNULL(s3.F_DeleteMark,0) = 0 AND ISNULL(s3.F_Status,0) = 1) AS F_GradedCount
FROM school_homework a
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (studentOnly)
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = 1");
                if (string.IsNullOrEmpty(myClassId))
                {
                    sb.Append(" AND 1 = 0");
                }
                else
                {
                    sb.Append(" AND a.F_ClassId = '" + Q(myClassId) + "'");
                }
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Title LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + (status == "1" ? "1" : "0"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 学生视角：我的作业（本班已发布作业 + 本人提交记录）
        /// status: 空=全部，0=未提交，1=已提交待批改，2=已批改
        /// </summary>
        public async Task<List<HomeworkExtend>> GetMyList(SoulPage<HomeworkExtend> pagination, string keyword, string status)
        {
            var allowed = new HashSet<string> { "F_Title", "F_CourseName", "F_Deadline", "F_Score", "F_SubmitTime", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
            {
                pagination.field = "F_Deadline";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var myClassId = await GetMyClassId();
            var sql = BuildMyQuerySql(keyword, status, myClassId);
            return await repository.Db.SqlQueryable<HomeworkExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildMyQuerySql(string keyword, string status, string classId)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*,
       c.F_Name AS F_CourseName,
       o.F_FullName AS F_ClassName,
       s.F_Id AS F_SubmitId,
       s.F_Status AS F_SubmitStatus,
       s.F_Score AS F_Score,
       s.F_SubmitTime AS F_SubmitTime
FROM school_homework a
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_homework_submit s ON s.F_HomeworkId = a.F_Id COLLATE Chinese_PRC_CI_AS
    AND s.F_StudentId = '" + Q(currentuser.UserId) + @"' AND ISNULL(s.F_DeleteMark,0) = 0
WHERE ISNULL(a.F_DeleteMark,0) = 0 AND ISNULL(a.F_Status,0) = 1");
            if (string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND 1 = 0");
            }
            else
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Title LIKE N'%" + kw + "%' OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (status == "0")
            {
                sb.Append(" AND s.F_Id IS NULL");
            }
            else if (status == "1")
            {
                sb.Append(" AND s.F_Id IS NOT NULL AND ISNULL(s.F_Status,0) = 0");
            }
            else if (status == "2")
            {
                sb.Append(" AND ISNULL(s.F_Status,0) = 1");
            }
            return sb.ToString();
        }

        public async Task<HomeworkEntity> GetForm(string keyValue)
        {
            var data = await repository.FindEntity(keyValue);
            if (data == null || data.F_DeleteMark == true)
            {
                throw new Exception("作业不存在");
            }
            if (IsStudent)
            {
                if ((data.F_Status ?? 0) != 1)
                {
                    throw new Exception("作业尚未发布");
                }
                var myClassId = await GetMyClassId();
                if (string.IsNullOrEmpty(myClassId) || !string.Equals(myClassId, data.F_ClassId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("只能查看本班作业");
                }
            }
            return data;
        }

        /// <summary>
        /// 学生端作业详情（作业信息 + 本人提交信息）
        /// </summary>
        public async Task<HomeworkDetailModel> GetHomeworkDetail(string homeworkId)
        {
            if (string.IsNullOrEmpty(homeworkId))
            {
                throw new Exception("缺少作业参数");
            }
            var homework = await repository.FindEntity(homeworkId);
            if (homework == null || homework.F_DeleteMark == true)
            {
                throw new Exception("作业不存在");
            }
            if (IsStudent)
            {
                if ((homework.F_Status ?? 0) != 1)
                {
                    throw new Exception("作业尚未发布");
                }
                var myClassId = await GetMyClassId();
                if (string.IsNullOrEmpty(myClassId) || !string.Equals(myClassId, homework.F_ClassId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("只能查看本班作业");
                }
            }
            var model = new HomeworkDetailModel
            {
                F_Id = homework.F_Id,
                F_Title = homework.F_Title,
                F_Content = homework.F_Content,
                F_FileUrl = homework.F_FileUrl,
                F_FileName = homework.F_FileName,
                F_FileSize = homework.F_FileSize,
                F_Deadline = homework.F_Deadline,
                F_FullScore = homework.F_FullScore,
                F_Status = homework.F_Status
            };
            if (!string.IsNullOrEmpty(homework.F_CourseId))
            {
                model.F_CourseName = await repository.Db.Queryable<CourseEntity>()
                    .Where(a => a.F_Id == homework.F_CourseId)
                    .Select(a => a.F_Name)
                    .FirstAsync();
                model.F_CourseName = model.F_CourseName ?? "";
            }
            if (!string.IsNullOrEmpty(homework.F_ClassId))
            {
                var className = await repository.Db.Queryable<OrganizeEntity>()
                    .Where(a => a.F_Id == homework.F_ClassId)
                    .Select(a => a.F_FullName)
                    .FirstAsync();
                model.F_ClassName = className ?? "";
            }
            if (!string.IsNullOrEmpty(currentuser.UserId))
            {
                var submit = await repository.Db.Queryable<HomeworkSubmitEntity>()
                    .Where(a => a.F_HomeworkId == homeworkId && a.F_StudentId == currentuser.UserId && a.F_DeleteMark == false)
                    .FirstAsync();
                if (submit != null)
                {
                    model.F_MySubmitId = submit.F_Id;
                    model.F_MyStatus = submit.F_Status;
                    model.F_MyScore = submit.F_Score;
                    model.F_MyComment = submit.F_Comment;
                    model.F_MyContent = submit.F_Content;
                    model.F_MyFileUrl = submit.F_FileUrl;
                    model.F_MyFileName = submit.F_FileName;
                    model.F_MyFileSize = submit.F_FileSize;
                    model.F_MySubmitTime = submit.F_SubmitTime;
                }
            }
            return model;
        }

        /// <summary>
        /// 提交名单（status: 空/1已提交，0未提交，2待批改，3已批改）
        /// </summary>
        public async Task<List<HomeworkSubmitExtend>> GetSubmitList(SoulPage<HomeworkSubmitExtend> pagination, string homeworkId, string status)
        {
            if (IsStudent)
            {
                throw new Exception("学生无权查看提交名单");
            }
            if (string.IsNullOrEmpty(homeworkId))
            {
                throw new Exception("缺少作业参数");
            }
            var homework = await repository.FindEntity(homeworkId);
            if (homework == null || homework.F_DeleteMark == true)
            {
                throw new Exception("作业不存在");
            }
            string sql;
            if (status == "0")
            {
                var allowed = new HashSet<string> { "F_Account", "F_RealName", "F_ClassName" };
                if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
                {
                    pagination.field = "F_Account";
                    pagination.order = "asc";
                }
                else if (pagination.order != "asc" && pagination.order != "desc")
                {
                    pagination.order = "asc";
                }
                sql = BuildUnsubmitSql(homework);
            }
            else
            {
                var allowed = new HashSet<string> { "F_Account", "F_RealName", "F_ClassName", "F_SubmitTime", "F_Status", "F_Score" };
                if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
                {
                    pagination.field = "F_SubmitTime";
                    pagination.order = "desc";
                }
                else if (pagination.order != "asc" && pagination.order != "desc")
                {
                    pagination.order = "desc";
                }
                sql = BuildSubmitSql(homeworkId, status);
            }
            return await repository.Db.SqlQueryable<HomeworkSubmitExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildSubmitSql(string homeworkId, string status)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT s.*,
       u.F_Account,
       u.F_RealName,
       o.F_FullName AS F_ClassName,
       h.F_Title AS F_HomeworkTitle,
       CONVERT(bit,1) AS F_IsSubmitted
FROM school_homework_submit s
LEFT JOIN sys_user u ON s.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_homework h ON s.F_HomeworkId = h.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON h.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE s.F_HomeworkId = '" + Q(homeworkId) + @"' AND ISNULL(s.F_DeleteMark,0) = 0");
            if (status == "2")
            {
                sb.Append(" AND ISNULL(s.F_Status,0) = 0");
            }
            else if (status == "3")
            {
                sb.Append(" AND ISNULL(s.F_Status,0) = 1");
            }
            return sb.ToString();
        }

        private string BuildUnsubmitSql(HomeworkEntity homework)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT u.F_Id AS F_StudentId,
       u.F_Account,
       u.F_RealName,
       (SELECT o.F_FullName FROM sys_organize o WHERE o.F_Id = '" + Q(homework.F_ClassId) + @"') AS F_ClassName,
       '" + Q(homework.F_Id) + @"' AS F_HomeworkId,
       N'" + Q(homework.F_Title ?? "") + @"' AS F_HomeworkTitle,
       CONVERT(bit,0) AS F_IsSubmitted
FROM sys_user u
LEFT JOIN school_homework_submit s ON s.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
    AND s.F_HomeworkId = '" + Q(homework.F_Id) + @"' AND ISNULL(s.F_DeleteMark,0) = 0
WHERE ISNULL(u.F_DeleteMark,0) = 0 AND ISNULL(u.F_EnabledMark,0) = 1
  AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE '%,' + '" + Q(homework.F_ClassId) + @"' + ',%'
  AND (',' + ISNULL(u.F_RoleId,'') + ',') LIKE '%," + StudentRoleId + @",%'
  AND s.F_Id IS NULL");
            return sb.ToString();
        }

        /// <summary>
        /// 批改页数据
        /// </summary>
        public async Task<HomeworkSubmitExtend> GetSubmitDetail(string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("缺少提交记录参数");
            }
            var sql = @"
SELECT s.*,
       u.F_Account,
       u.F_RealName,
       o.F_FullName AS F_ClassName,
       h.F_Title AS F_HomeworkTitle,
       h.F_Content AS F_HomeworkContent,
       h.F_Deadline AS F_HomeworkDeadline,
       h.F_FullScore AS F_HomeworkFullScore,
       h.F_FileUrl AS F_HomeworkFileUrl,
       h.F_FileName AS F_HomeworkFileName,
       CONVERT(bit,1) AS F_IsSubmitted
FROM school_homework_submit s
LEFT JOIN sys_user u ON s.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_homework h ON s.F_HomeworkId = h.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON h.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE s.F_Id = '" + Q(keyValue) + @"' AND ISNULL(s.F_DeleteMark,0) = 0";
            var data = await repository.Db.SqlQueryable<HomeworkSubmitExtend>(sql).FirstAsync();
            if (data == null)
            {
                throw new Exception("提交记录不存在");
            }
            if (IsStudent && data.F_StudentId != currentuser.UserId)
            {
                throw new Exception("只能查看自己的提交记录");
            }
            return data;
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 新增/修改作业（新增为草稿）
        /// </summary>
        public async Task SubmitForm(HomeworkEntity entity, string keyValue)
        {
            if (entity == null)
            {
                throw new Exception("提交数据不能为空");
            }
            if (IsStudent)
            {
                throw new Exception("学生不能发布作业");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Title))
            {
                throw new Exception("请填写作业标题");
            }
            if (string.IsNullOrEmpty(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            if (string.IsNullOrEmpty(entity.F_ClassId))
            {
                throw new Exception("请选择班级");
            }
            if (entity.F_FullScore == null || entity.F_FullScore <= 0 || entity.F_FullScore > 150)
            {
                throw new Exception("满分必须在0到150之间");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Content) && string.IsNullOrEmpty(entity.F_FileUrl))
            {
                throw new Exception("请填写作业要求或上传附件");
            }
            entity.F_Title = entity.F_Title.Trim();

            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Status = 0;
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.F_PublisherId = currentuser.UserId;
                entity.F_PublisherName = currentuser.UserName;
                entity.Create();
                await repository.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var existing = await repository.Db.Queryable<HomeworkEntity>()
                    .FirstAsync(a => a.F_Id == keyValue && a.F_DeleteMark == false);
                if (existing == null)
                {
                    throw new Exception("作业不存在");
                }
                existing.F_Title = entity.F_Title;
                existing.F_CourseId = entity.F_CourseId;
                existing.F_ClassId = entity.F_ClassId;
                existing.F_Content = entity.F_Content;
                existing.F_FileUrl = entity.F_FileUrl;
                existing.F_FileName = entity.F_FileName;
                existing.F_FileSize = entity.F_FileSize;
                existing.F_Deadline = entity.F_Deadline;
                existing.F_FullScore = entity.F_FullScore;
                existing.F_LastModifyTime = DateTime.Now;
                existing.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 发布/取消发布
        /// </summary>
        public async Task SetStatus(string keyValue, int status)
        {
            if (IsStudent)
            {
                throw new Exception("学生不能发布作业");
            }
            if (status != 0 && status != 1)
            {
                throw new Exception("状态参数不正确");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("请选择作业");
            }
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false, a => new HomeworkEntity
            {
                F_Status = status,
                F_LastModifyTime = DateTime.Now,
                F_LastModifyUserId = currentuser.UserId
            });
        }

        /// <summary>
        /// 删除作业及其提交记录
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            if (IsStudent)
            {
                throw new Exception("学生不能删除作业");
            }
            var ids = (keyValue ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                throw new Exception("请选择需要删除的作业");
            }
            var now = DateTime.Now;
            await repository.Update(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false, a => new HomeworkEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = now,
                F_DeleteUserId = currentuser.UserId
            });
            await repository.Db.Updateable<HomeworkSubmitEntity>()
                .SetColumns(a => new HomeworkSubmitEntity
                {
                    F_DeleteMark = true,
                    F_DeleteTime = now,
                    F_DeleteUserId = currentuser.UserId
                })
                .Where(a => ids.Contains(a.F_HomeworkId) && a.F_DeleteMark == false)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 学生提交作业（在线作答 + 附件/拍照，按作业+学生覆盖保存）
        /// </summary>
        public async Task SaveAnswer(string homeworkId, string content, string fileUrl, string fileName, long fileSize)
        {
            if (string.IsNullOrEmpty(homeworkId))
            {
                throw new Exception("缺少作业参数");
            }
            if (!IsStudent)
            {
                throw new Exception("只有学生才能提交作业");
            }
            var homework = await repository.FindEntity(homeworkId);
            if (homework == null || homework.F_DeleteMark == true)
            {
                throw new Exception("作业不存在");
            }
            if ((homework.F_Status ?? 0) != 1)
            {
                throw new Exception("作业尚未发布，不能提交");
            }
            var myClassId = await GetMyClassId();
            if (string.IsNullOrEmpty(myClassId) || !string.Equals(myClassId, homework.F_ClassId, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("只能提交本班作业");
            }
            if (homework.F_Deadline.HasValue && DateTime.Now > homework.F_Deadline.Value)
            {
                throw new Exception("已过截止时间，无法提交");
            }
            if (string.IsNullOrWhiteSpace(content) && string.IsNullOrEmpty(fileUrl))
            {
                throw new Exception("请在线填写作答内容或上传附件");
            }
            var existing = await repository.Db.Queryable<HomeworkSubmitEntity>()
                .Where(a => a.F_HomeworkId == homeworkId && a.F_StudentId == currentuser.UserId)
                .FirstAsync();
            var now = DateTime.Now;
            if (existing == null)
            {
                var entity = new HomeworkSubmitEntity
                {
                    F_HomeworkId = homeworkId,
                    F_StudentId = currentuser.UserId,
                    F_Content = content,
                    F_FileUrl = fileUrl,
                    F_FileName = fileName,
                    F_FileSize = fileSize,
                    F_SubmitTime = now,
                    F_Status = 0,
                    F_DeleteMark = false
                };
                entity.Create();
                await repository.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                existing.F_Content = content;
                existing.F_FileUrl = fileUrl;
                existing.F_FileName = fileName;
                existing.F_FileSize = fileSize;
                existing.F_SubmitTime = now;
                existing.F_Status = 0;
                existing.F_Score = null;
                existing.F_Comment = null;
                existing.F_GradeTime = null;
                existing.F_GraderId = null;
                existing.F_GraderName = null;
                existing.F_DeleteMark = false;
                existing.F_DeleteTime = null;
                existing.F_DeleteUserId = null;
                existing.F_LastModifyTime = now;
                existing.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 教师批改作业（打分 + 评语）
        /// </summary>
        public async Task GradeForm(string keyValue, decimal score, string comment)
        {
            if (IsStudent)
            {
                throw new Exception("学生不能批改作业");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("请选择提交记录");
            }
            if (!string.IsNullOrEmpty(comment) && comment.Length > 500)
            {
                throw new Exception("评语不能超过500字");
            }
            var entity = await repository.Db.Queryable<HomeworkSubmitEntity>()
                .Where(a => a.F_Id == keyValue && a.F_DeleteMark == false)
                .FirstAsync();
            if (entity == null)
            {
                throw new Exception("提交记录不存在");
            }
            var homework = await repository.FindEntity(entity.F_HomeworkId);
            var fullScore = homework == null || homework.F_FullScore == null ? 100m : homework.F_FullScore.Value;
            if (score < 0 || score > fullScore)
            {
                throw new Exception("分数必须在0到" + fullScore.ToString("0.#") + "之间");
            }
            entity.F_Score = Math.Round(score, 1);
            entity.F_Comment = comment;
            entity.F_Status = 1;
            entity.F_GradeTime = DateTime.Now;
            entity.F_GraderId = currentuser.UserId;
            entity.F_GraderName = currentuser.UserName;
            entity.F_LastModifyTime = DateTime.Now;
            entity.F_LastModifyUserId = currentuser.UserId;
            await repository.Db.Updateable(entity).ExecuteCommandAsync();
        }

        #endregion
    }
}
