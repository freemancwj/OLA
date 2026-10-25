using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemManage;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service;

namespace WaterCloud.Service.InstructionalManagement
{
    #region 提交数据模型

    /// <summary>
    /// 试卷题目提交行
    /// </summary>
    public class ExamQuestionSubmitRow
    {
        public string F_QuestionId { get; set; }

        public decimal? F_Score { get; set; }

        public int? F_SortCode { get; set; }
    }

    /// <summary>
    /// 学生答题提交行
    /// </summary>
    public class ExamAnswerSubmitRow
    {
        public string F_QuestionId { get; set; }

        public string F_Answer { get; set; }
    }

    /// <summary>
    /// 人工批改提交行
    /// </summary>
    public class ExamGradeSubmitRow
    {
        public string F_Id { get; set; }

        public decimal? F_Score { get; set; }

        public string F_Comment { get; set; }
    }

    #endregion

    /// <summary>
    /// 在线考试服务
    /// </summary>
    public class ExamService : BaseService<ExamEntity>, IDenpendency
    {
        //科目字典
        private const string CourseItemId = "08dd9f77-fcba-446c-8a54-fb2b95717c33";
        //学生角色
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public ExamService(ISqlSugarClient context) : base(context)
        {
        }

        #region 基础数据

        /// <summary>
        /// 科目列表
        /// </summary>
        public async Task<List<ItemsDetailEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_ItemId == CourseItemId && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
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
        /// 当前用户是否为学生
        /// </summary>
        private bool IsStudent
        {
            get
            {
                return (currentuser.RoleId ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Contains(StudentRoleId);
            }
        }

        /// <summary>
        /// 获取学生所属班级（取第一个班级类型部门）
        /// </summary>
        private async Task<string> GetStudentClassId()
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
            var orgIds = organizeId.Split(',')
                .Select(a => a.Trim())
                .Where(a => a.Length > 0)
                .ToList();
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

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static string GetTypeName(int? type)
        {
            switch (type)
            {
                case 1: return "单选题";
                case 2: return "多选题";
                case 3: return "判断题";
                case 4: return "简答题";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// 排序字段白名单校验
        /// </summary>
        private static void ApplySort<T>(SoulPage<T> pagination, HashSet<string> allowed, string defaultField, string defaultOrder)
        {
            if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
            {
                pagination.field = defaultField;
                pagination.order = defaultOrder;
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = defaultOrder;
            }
        }

        #endregion

        #region 考试列表

        /// <summary>
        /// 考试列表（教师端；学生调用时只返回本班已发布/已结束的考试）
        /// </summary>
        public async Task<List<ExamExtend>> GetLookList(SoulPage<ExamExtend> pagination, string keyword, string courseId, string classId, string type, string status)
        {
            ApplySort(pagination, new HashSet<string>
            {
                "F_Title", "F_CourseName", "F_ClassName", "F_Type", "F_StartTime", "F_EndTime",
                "F_Duration", "F_TotalScore", "F_PassScore", "F_Status", "F_QuestionCount", "F_RecordCount", "F_CreatorTime"
            }, "F_CreatorTime", "desc");

            string studentClassId = null;
            if (IsStudent)
            {
                studentClassId = await GetStudentClassId();
                if (string.IsNullOrEmpty(studentClassId))
                {
                    pagination.count = 0;
                    return new List<ExamExtend>();
                }
            }
            var sql = BuildExamQuerySql(keyword, courseId, classId, type, status, studentClassId);
            return await repository.SqlQueryable<ExamExtend>(sql).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 学生端：我的考试列表（本班已发布/已结束，附带本人答题记录）
        /// </summary>
        public async Task<List<ExamExtend>> GetMyExamList(SoulPage<ExamExtend> pagination, string keyword, string type)
        {
            ApplySort(pagination, new HashSet<string>
            {
                "F_Title", "F_CourseName", "F_Type", "F_StartTime", "F_EndTime",
                "F_Duration", "F_TotalScore", "F_PassScore", "F_Status", "F_QuestionCount", "F_RecordStatus", "F_GotScore"
            }, "F_StartTime", "desc");

            var classId = await GetStudentClassId();
            if (string.IsNullOrEmpty(classId))
            {
                pagination.count = 0;
                return new List<ExamExtend>();
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.F_Id, a.F_Title, a.F_CourseId, a.F_ClassId, a.F_Type, a.F_StartTime, a.F_EndTime, a.F_Duration,
       a.F_TotalScore, a.F_PassScore, a.F_Status, a.F_Remark, a.F_PublisherId, a.F_PublisherName,
       a.F_EnabledMark, a.F_DeleteMark, a.F_CreatorTime, a.F_CreatorUserId,
       a.F_LastModifyTime, a.F_LastModifyUserId, a.F_DeleteTime, a.F_DeleteUserId,
       c.F_ItemName AS F_CourseName,
       o.F_FullName AS F_ClassName,
       (SELECT COUNT(1) FROM school_exam_question q WHERE q.F_ExamId = a.F_Id AND ISNULL(q.F_DeleteMark,0)=0) AS F_QuestionCount,
       (SELECT COUNT(1) FROM school_exam_record rc WHERE rc.F_ExamId = a.F_Id AND ISNULL(rc.F_DeleteMark,0)=0) AS F_RecordCount,
       r.F_Id AS F_RecordId,
       r.F_Status AS F_RecordStatus,
       r.F_TotalScore AS F_GotScore,
       r.F_IsPassed
FROM school_exam a
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_exam_record r ON a.F_Id = r.F_ExamId COLLATE Chinese_PRC_CI_AS
     AND r.F_StudentId = '" + Q(currentuser.UserId) + @"' AND ISNULL(r.F_DeleteMark,0)=0
WHERE ISNULL(a.F_DeleteMark,0)=0
  AND ISNULL(a.F_Status,0) IN (1,2)
  AND a.F_ClassId = '" + Q(classId) + "'");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Title COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (int.TryParse(type, out var typeValue) && typeValue >= 1 && typeValue <= 2)
            {
                sb.Append(" AND a.F_Type = " + typeValue);
            }
            return await repository.SqlQueryable<ExamExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        private string BuildExamQuerySql(string keyword, string courseId, string classId, string type, string status, string studentClassId)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*,
       c.F_ItemName AS F_CourseName,
       o.F_FullName AS F_ClassName,
       (SELECT COUNT(1) FROM school_exam_question q WHERE q.F_ExamId = a.F_Id AND ISNULL(q.F_DeleteMark,0)=0) AS F_QuestionCount,
       (SELECT COUNT(1) FROM school_exam_record r WHERE r.F_ExamId = a.F_Id AND ISNULL(r.F_DeleteMark,0)=0) AS F_RecordCount
FROM school_exam a
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0)=0");
            if (IsStudent)
            {
                //学生只能查看本班已发布及已结束的考试
                sb.Append(" AND ISNULL(a.F_Status,0) IN (1,2) AND a.F_ClassId = '" + Q(studentClassId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Title COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!IsStudent && !string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (int.TryParse(type, out var typeValue) && typeValue >= 1 && typeValue <= 2)
            {
                sb.Append(" AND a.F_Type = " + typeValue);
            }
            if (int.TryParse(status, out var statusValue) && statusValue >= 0 && statusValue <= 2)
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + statusValue);
            }
            return sb.ToString();
        }

        #endregion

        #region 表单与题库

        /// <summary>
        /// 考试表单（含已选试卷题目）
        /// </summary>
        public async Task<ExamFormModel> GetForm(string keyValue)
        {
            var entity = await repository.FindEntity(keyValue);
            if (entity == null || entity.F_DeleteMark == true)
            {
                throw new Exception("考试不存在或已删除！");
            }
            var model = new ExamFormModel
            {
                F_Id = entity.F_Id,
                F_Title = entity.F_Title,
                F_CourseId = entity.F_CourseId,
                F_ClassId = entity.F_ClassId,
                F_Type = entity.F_Type,
                F_StartTime = entity.F_StartTime,
                F_EndTime = entity.F_EndTime,
                F_Duration = entity.F_Duration,
                F_TotalScore = entity.F_TotalScore,
                F_PassScore = entity.F_PassScore,
                F_Status = entity.F_Status,
                F_Remark = entity.F_Remark,
                F_PublisherId = entity.F_PublisherId,
                F_PublisherName = entity.F_PublisherName,
                F_EnabledMark = entity.F_EnabledMark,
                F_DeleteMark = entity.F_DeleteMark,
                F_CreatorTime = entity.F_CreatorTime,
                F_CreatorUserId = entity.F_CreatorUserId,
                F_LastModifyTime = entity.F_LastModifyTime,
                F_LastModifyUserId = entity.F_LastModifyUserId,
                F_DeleteTime = entity.F_DeleteTime,
                F_DeleteUserId = entity.F_DeleteUserId
            };
            model.Questions = await GetExamQuestions(keyValue);
            return model;
        }

        /// <summary>
        /// 试卷题目（含题库答案与解析，仅用于教师端及判分）
        /// </summary>
        private async Task<List<ExamQuestionExtend>> GetExamQuestions(string examId)
        {
            var sql = @"
SELECT eq.F_Id, eq.F_QuestionId, q.F_Type, q.F_Content, q.F_OptionA, q.F_OptionB, q.F_OptionC, q.F_OptionD,
       q.F_Answer, q.F_Analysis,
       ISNULL(eq.F_Score,0) AS F_Score, ISNULL(q.F_Score,0) AS F_FullScore,
       eq.F_SortCode,
       c.F_ItemName AS F_CourseName
FROM school_exam_question eq
LEFT JOIN school_question q ON eq.F_QuestionId = q.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON q.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
WHERE eq.F_ExamId = '" + Q(examId) + @"' AND ISNULL(eq.F_DeleteMark,0)=0
ORDER BY ISNULL(eq.F_SortCode,0), eq.F_CreatorTime";
            var list = await repository.SqlQueryable<ExamQuestionExtend>(sql).ToListAsync();
            foreach (var item in list)
            {
                item.F_TypeName = GetTypeName(item.F_Type);
            }
            return list;
        }

        /// <summary>
        /// 题库列表（选题弹层使用）
        /// </summary>
        public async Task<List<ExamQuestionExtend>> GetQuestionList(string courseId, string type, string keyword)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT " + SqlDialect.Top(1000) + @"q.F_Id, q.F_Id AS F_QuestionId, q.F_Type, q.F_Content,
       q.F_OptionA, q.F_OptionB, q.F_OptionC, q.F_OptionD, q.F_Answer, q.F_Analysis,
       q.F_Score AS F_Score, q.F_Score AS F_FullScore,
       c.F_ItemName AS F_CourseName
FROM school_question q
LEFT JOIN sys_itemsdetail c ON q.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(q.F_DeleteMark,0)=0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.Append(" AND q.F_Content COLLATE Chinese_PRC_CI_AS LIKE N'%" + Q(keyword.Trim()) + "%'");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND q.F_CourseId = '" + Q(courseId) + "'");
            }
            if (int.TryParse(type, out var typeValue) && typeValue >= 1 && typeValue <= 4)
            {
                sb.Append(" AND q.F_Type = " + typeValue);
            }
            sb.Append(" ORDER BY q.F_Type, q.F_CreatorTime DESC" + SqlDialect.Limit(1000));
            var list = await repository.SqlQueryable<ExamQuestionExtend>(sb.ToString()).ToListAsync();
            foreach (var item in list)
            {
                item.F_TypeName = GetTypeName(item.F_Type);
            }
            return list;
        }

        #endregion

        #region 考试提交

        /// <summary>
        /// 保存考试（含试卷题目），同一事务内先保存考试再重写试卷题目
        /// </summary>
        public async Task SubmitForm(ExamFormModel exam, string questionsJson)
        {
            if (exam == null)
            {
                throw new Exception("考试信息不能为空！");
            }
            if (string.IsNullOrWhiteSpace(exam.F_Title))
            {
                throw new Exception("请填写考试标题！");
            }
            if (string.IsNullOrEmpty(exam.F_CourseId))
            {
                throw new Exception("请选择科目！");
            }
            if (string.IsNullOrEmpty(exam.F_ClassId))
            {
                throw new Exception("请选择班级！");
            }
            if (!exam.F_Type.HasValue || exam.F_Type.Value < 1 || exam.F_Type.Value > 2)
            {
                throw new Exception("请选择考试类型！");
            }
            if (!exam.F_Duration.HasValue || exam.F_Duration.Value <= 0)
            {
                throw new Exception("考试时长必须大于0分钟！");
            }
            if (exam.F_StartTime.HasValue && exam.F_EndTime.HasValue && exam.F_EndTime.Value <= exam.F_StartTime.Value)
            {
                throw new Exception("结束时间必须晚于开始时间！");
            }

            var parsedRows = string.IsNullOrEmpty(questionsJson)
                ? new List<ExamQuestionSubmitRow>()
                : questionsJson.ToObject<List<ExamQuestionSubmitRow>>();
            var rows = new List<ExamQuestionSubmitRow>();
            var questionIds = new HashSet<string>();
            foreach (var row in parsedRows ?? new List<ExamQuestionSubmitRow>())
            {
                if (row == null || string.IsNullOrWhiteSpace(row.F_QuestionId))
                {
                    continue;
                }
                if (!questionIds.Add(row.F_QuestionId))
                {
                    throw new Exception("试卷中存在重复题目，请刷新后重试！");
                }
                if (row.F_Score.HasValue && row.F_Score.Value < 0)
                {
                    throw new Exception("题目分值不能小于0！");
                }
                rows.Add(row);
            }
            if (rows.Count > 0)
            {
                var ids = rows.Select(a => a.F_QuestionId).ToList();
                var validCount = await repository.Db.Queryable<QuestionEntity>()
                    .Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false)
                    .CountAsync();
                if (validCount != rows.Count)
                {
                    throw new Exception("存在无效或已删除的题目，请刷新后重试！");
                }
            }

            var totalScore = rows.Where(a => a.F_Score.HasValue).Sum(a => a.F_Score.Value);
            var passScore = exam.F_PassScore ?? 0;
            if (passScore < 0)
            {
                throw new Exception("及格分不能小于0！");
            }
            if (totalScore > 0 && passScore > totalScore)
            {
                throw new Exception("及格分不能大于试卷总分！");
            }

            var entity = new ExamEntity
            {
                F_Title = exam.F_Title.Trim(),
                F_CourseId = exam.F_CourseId,
                F_ClassId = exam.F_ClassId,
                F_Type = exam.F_Type,
                F_StartTime = exam.F_StartTime,
                F_EndTime = exam.F_EndTime,
                F_Duration = exam.F_Duration,
                F_TotalScore = totalScore,
                F_PassScore = passScore,
                F_Remark = string.IsNullOrWhiteSpace(exam.F_Remark) ? null : exam.F_Remark.Trim()
            };

            string examId;
            repository.Db.Ado.BeginTran();
            try
            {
                if (string.IsNullOrEmpty(exam.F_Id))
                {
                    entity.F_Status = exam.F_Status ?? 0;
                    entity.F_PublisherId = currentuser.UserId;
                    entity.F_PublisherName = currentuser.UserName;
                    entity.F_DeleteMark = false;
                    entity.F_EnabledMark = true;
                    entity.Create();
                    await repository.Db.Insertable(entity).ExecuteCommandAsync();
                    examId = entity.F_Id;
                }
                else
                {
                    var existing = await repository.Db.Queryable<ExamEntity>()
                        .FirstAsync(a => a.F_Id == exam.F_Id && a.F_DeleteMark == false);
                    if (existing == null)
                    {
                        throw new Exception("考试不存在或已删除！");
                    }
                    existing.F_Title = entity.F_Title;
                    existing.F_CourseId = entity.F_CourseId;
                    existing.F_ClassId = entity.F_ClassId;
                    existing.F_Type = entity.F_Type;
                    existing.F_StartTime = entity.F_StartTime;
                    existing.F_EndTime = entity.F_EndTime;
                    existing.F_Duration = entity.F_Duration;
                    existing.F_TotalScore = entity.F_TotalScore;
                    existing.F_PassScore = entity.F_PassScore;
                    existing.F_Remark = entity.F_Remark;
                    existing.F_LastModifyTime = DateTime.Now;
                    existing.F_LastModifyUserId = currentuser.UserId;
                    await repository.Db.Updateable(existing).ExecuteCommandAsync();
                    examId = existing.F_Id;
                }

                //重写试卷题目
                await repository.Db.Deleteable<ExamQuestionEntity>()
                    .Where(a => a.F_ExamId == examId)
                    .ExecuteCommandAsync();
                if (rows.Count > 0)
                {
                    var list = new List<ExamQuestionEntity>();
                    for (var i = 0; i < rows.Count; i++)
                    {
                        var item = new ExamQuestionEntity
                        {
                            F_ExamId = examId,
                            F_QuestionId = rows[i].F_QuestionId,
                            F_Score = rows[i].F_Score ?? 0,
                            F_SortCode = rows[i].F_SortCode ?? (i + 1),
                            F_DeleteMark = false
                        };
                        item.Create();
                        list.Add(item);
                    }
                    await repository.Db.Insertable(list).ExecuteCommandAsync();
                }
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }

            exam.F_Id = examId;
            exam.F_TotalScore = totalScore;
        }

        /// <summary>
        /// 删除考试（逻辑删除考试，同时物理删除其试卷题目）
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            var ids = (keyValue ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                throw new Exception("请选择要删除的考试！");
            }
            var list = await repository.Db.Queryable<ExamEntity>()
                .Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false)
                .ToListAsync();
            if (list.Count == 0)
            {
                throw new Exception("没有找到要删除的考试！");
            }
            var now = DateTime.Now;
            repository.Db.Ado.BeginTran();
            try
            {
                foreach (var item in list)
                {
                    item.F_DeleteMark = true;
                    item.F_DeleteTime = now;
                    item.F_DeleteUserId = currentuser.UserId;
                }
                await repository.Db.Updateable(list).ExecuteCommandAsync();
                await repository.Db.Deleteable<ExamQuestionEntity>()
                    .Where(a => ids.Contains(a.F_ExamId))
                    .ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 考试状态设置（0草稿 1已发布 2已结束）
        /// </summary>
        public async Task SetStatus(string keyValue, int status)
        {
            if (status != 0 && status != 1 && status != 2)
            {
                throw new Exception("考试状态不正确！");
            }
            var entity = await repository.FindEntity(keyValue);
            if (entity == null || entity.F_DeleteMark == true)
            {
                throw new Exception("考试不存在或已删除！");
            }
            if (status == 1)
            {
                var questionCount = await repository.Db.Queryable<ExamQuestionEntity>()
                    .Where(a => a.F_ExamId == keyValue && a.F_DeleteMark == false)
                    .CountAsync();
                if (questionCount == 0)
                {
                    throw new Exception("试卷还没有题目，不能发布！");
                }
                if ((entity.F_TotalScore ?? 0) <= 0)
                {
                    throw new Exception("试卷总分必须大于0才能发布！");
                }
            }
            entity.F_Status = status;
            entity.F_LastModifyTime = DateTime.Now;
            entity.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(entity);
        }

        #endregion

        #region 学生答题

        /// <summary>
        /// 开始考试（校验考试信息并返回试卷，题目不含答案与解析）
        /// </summary>
        public async Task<ExamTakeModel> StartExam(string examId)
        {
            if (!IsStudent)
            {
                throw new Exception("只有学生才能参加考试！");
            }
            if (string.IsNullOrEmpty(examId))
            {
                throw new Exception("缺少考试参数！");
            }
            var exam = await repository.FindEntity(examId);
            if (exam == null || exam.F_DeleteMark == true)
            {
                throw new Exception("考试不存在或已删除！");
            }
            if ((exam.F_Status ?? 0) != 1)
            {
                throw new Exception((exam.F_Status ?? 0) == 2 ? "考试已结束！" : "考试尚未发布！");
            }
            var classId = await GetStudentClassId();
            if (string.IsNullOrEmpty(classId))
            {
                throw new Exception("您还没有分配班级，不能参加考试！");
            }
            if (!string.Equals(exam.F_ClassId, classId, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("该考试不属于您所在的班级！");
            }
            var now = DateTime.Now;
            if (exam.F_StartTime.HasValue && now < exam.F_StartTime.Value)
            {
                throw new Exception("考试尚未开始！");
            }
            if (exam.F_EndTime.HasValue && now > exam.F_EndTime.Value)
            {
                throw new Exception("考试已结束！");
            }

            var record = await repository.Db.Queryable<ExamRecordEntity>()
                .FirstAsync(a => a.F_ExamId == examId && a.F_StudentId == currentuser.UserId && a.F_DeleteMark == false);
            if (record == null)
            {
                record = new ExamRecordEntity
                {
                    F_ExamId = examId,
                    F_StudentId = currentuser.UserId,
                    F_StartTime = now,
                    F_ObjectiveScore = 0,
                    F_SubjectiveScore = 0,
                    F_TotalScore = 0,
                    F_Status = 0,
                    F_IsPassed = 0,
                    F_DeleteMark = false
                };
                record.Create();
                await repository.Db.Insertable(record).ExecuteCommandAsync();
            }
            else if (record.F_SubmitTime != null)
            {
                throw new Exception("您已完成该考试，请查看考试成绩！");
            }

            var questions = await GetExamQuestions(examId);
            var remaining = CalculateRemainingSeconds(record.F_StartTime ?? now, exam.F_Duration);
            foreach (var item in questions)
            {
                //答题页面以题目Id作为作答标识，且不返回答案与解析
                item.F_Id = item.F_QuestionId;
                item.F_FullScore = item.F_Score;
                item.F_Answer = null;
                item.F_Analysis = null;
            }
            return new ExamTakeModel
            {
                F_RecordId = record.F_Id,
                F_ExamId = examId,
                F_Title = exam.F_Title,
                F_Duration = exam.F_Duration,
                F_RemainingSeconds = remaining,
                F_TotalScore = exam.F_TotalScore,
                Questions = questions
            };
        }

        /// <summary>
        /// 计算剩余答题秒数
        /// </summary>
        private static int CalculateRemainingSeconds(DateTime startTime, int? duration)
        {
            if (!duration.HasValue || duration.Value <= 0)
            {
                return 1;
            }
            var total = duration.Value * 60;
            var used = (int)(DateTime.Now - startTime).TotalSeconds;
            var remaining = total - used;
            return remaining > 0 ? remaining : 1;
        }

        /// <summary>
        /// 交卷（自动判分客观题，简答题转人工批改）
        /// </summary>
        public async Task SubmitExam(string recordId, string answersJson)
        {
            if (string.IsNullOrEmpty(recordId))
            {
                throw new Exception("考试记录不存在！");
            }
            var record = await repository.Db.Queryable<ExamRecordEntity>()
                .FirstAsync(a => a.F_Id == recordId && a.F_DeleteMark == false);
            if (record == null)
            {
                throw new Exception("考试记录不存在！");
            }
            if (!string.Equals(record.F_StudentId, currentuser.UserId, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("只能提交自己的答卷！");
            }
            if (record.F_SubmitTime != null)
            {
                throw new Exception("该考试已交卷，请勿重复提交！");
            }
            var exam = await repository.Db.Queryable<ExamEntity>()
                .FirstAsync(a => a.F_Id == record.F_ExamId && a.F_DeleteMark == false);
            if (exam == null)
            {
                throw new Exception("考试不存在或已删除！");
            }
            var paper = await GetExamQuestions(record.F_ExamId);
            if (paper.Count == 0)
            {
                throw new Exception("试卷没有题目，无法交卷！");
            }

            var parsedAnswers = string.IsNullOrEmpty(answersJson)
                ? new List<ExamAnswerSubmitRow>()
                : answersJson.ToObject<List<ExamAnswerSubmitRow>>();
            var answerMap = new Dictionary<string, string>();
            foreach (var row in parsedAnswers ?? new List<ExamAnswerSubmitRow>())
            {
                if (row == null || string.IsNullOrWhiteSpace(row.F_QuestionId))
                {
                    continue;
                }
                if (!answerMap.ContainsKey(row.F_QuestionId))
                {
                    answerMap[row.F_QuestionId] = row.F_Answer ?? string.Empty;
                }
            }

            var list = new List<ExamAnswerEntity>();
            decimal objectiveScore = 0;
            var hasSubjective = false;
            foreach (var question in paper)
            {
                var studentAnswer = answerMap.ContainsKey(question.F_QuestionId) ? answerMap[question.F_QuestionId] : string.Empty;
                var fullScore = question.F_Score ?? 0;
                var answer = new ExamAnswerEntity
                {
                    F_ExamId = record.F_ExamId,
                    F_RecordId = recordId,
                    F_QuestionId = question.F_QuestionId,
                    F_QuestionType = question.F_Type,
                    F_Answer = studentAnswer,
                    F_FullScore = fullScore,
                    F_Score = 0,
                    F_DeleteMark = false
                };
                if (question.F_Type == 4)
                {
                    //简答题待人工批改
                    hasSubjective = true;
                    answer.F_IsCorrect = 2;
                }
                else
                {
                    var correct = IsAnswerCorrect(question.F_Type ?? 0, studentAnswer, question.F_Answer);
                    answer.F_IsCorrect = correct ? 1 : 0;
                    answer.F_Score = correct ? fullScore : 0;
                    objectiveScore += answer.F_Score.Value;
                }
                answer.Create();
                list.Add(answer);
            }

            repository.Db.Ado.BeginTran();
            try
            {
                await repository.Db.Deleteable<ExamAnswerEntity>()
                    .Where(a => a.F_RecordId == recordId)
                    .ExecuteCommandAsync();
                await repository.Db.Insertable(list).ExecuteCommandAsync();

                record.F_ObjectiveScore = objectiveScore;
                record.F_SubjectiveScore = 0;
                record.F_TotalScore = objectiveScore;
                record.F_SubmitTime = DateTime.Now;
                if (hasSubjective)
                {
                    record.F_Status = 0;
                    record.F_IsPassed = 0;
                }
                else
                {
                    record.F_Status = 1;
                    record.F_IsPassed = objectiveScore >= (exam.F_PassScore ?? 0) ? 1 : 0;
                }
                record.F_LastModifyTime = DateTime.Now;
                record.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(record).ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 客观题判分：单选/判断按去空格大写后比较，多选按排序后比较
        /// </summary>
        private static bool IsAnswerCorrect(int type, string studentAnswer, string correctAnswer)
        {
            var left = NormalizeAnswer(studentAnswer, type == 2);
            var right = NormalizeAnswer(correctAnswer, type == 2);
            if (string.IsNullOrEmpty(right))
            {
                return false;
            }
            return left == right;
        }

        private static string NormalizeAnswer(string answer, bool sort)
        {
            IEnumerable<char> chars = (answer ?? string.Empty).Where(char.IsLetter).Select(char.ToUpperInvariant);
            if (sort)
            {
                chars = chars.OrderBy(a => a);
            }
            return new string(chars.ToArray());
        }

        #endregion

        #region 成绩与批改

        /// <summary>
        /// 考试记录列表（教师查看成绩/批改；学生只能查看自己的记录）
        /// </summary>
        public async Task<List<ExamRecordExtend>> GetRecordList(SoulPage<ExamRecordExtend> pagination, string examId, string keyword, string status)
        {
            ApplySort(pagination, new HashSet<string>
            {
                "F_ExamTitle", "F_RealName", "F_Account", "F_ClassName", "F_SubmitTime",
                "F_ObjectiveScore", "F_SubjectiveScore", "F_TotalScore", "F_Status", "F_IsPassed", "F_GradeTime"
            }, "F_SubmitTime", "desc");

            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.F_Id, a.F_ExamId, e.F_Title AS F_ExamTitle, a.F_StudentId,
       u.F_Account, u.F_RealName,
       (SELECT " + SqlDialect.Top(1) + @"o.F_FullName FROM sys_organize o
          WHERE o.F_CategoryId = 'banji' AND ISNULL(o.F_DeleteMark,0)=0
            AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE ('%,' + o.F_Id COLLATE Chinese_PRC_CI_AS + ',%')" + SqlDialect.Limit(1) + @") AS F_ClassName,
       a.F_StartTime, a.F_SubmitTime, a.F_ObjectiveScore, a.F_SubjectiveScore, a.F_TotalScore,
       a.F_Status, a.F_IsPassed, a.F_GradeTime, a.F_GraderId, a.F_GraderName,
       e.F_TotalScore AS F_ExamTotalScore, e.F_PassScore, e.F_Duration,
       (SELECT COUNT(1) FROM school_exam_question q WHERE q.F_ExamId = e.F_Id AND ISNULL(q.F_DeleteMark,0)=0) AS F_QuestionCount
FROM school_exam_record a
INNER JOIN school_exam e ON a.F_ExamId = e.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0)=0 AND a.F_SubmitTime IS NOT NULL");
            if (IsStudent)
            {
                sb.Append(" AND a.F_StudentId = '" + Q(currentuser.UserId) + "'");
            }
            if (!string.IsNullOrEmpty(examId))
            {
                sb.Append(" AND a.F_ExamId = '" + Q(examId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR e.F_Title COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (int.TryParse(status, out var statusValue) && statusValue >= 0 && statusValue <= 1)
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + statusValue);
            }
            return await repository.SqlQueryable<ExamRecordExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 考试记录详情（含答题明细）
        /// </summary>
        public async Task<ExamRecordExtend> GetRecordForm(string recordId)
        {
            if (string.IsNullOrEmpty(recordId))
            {
                throw new Exception("考试记录不存在！");
            }
            var sql = @"
SELECT a.F_Id, a.F_ExamId, e.F_Title AS F_ExamTitle, a.F_StudentId,
       u.F_Account, u.F_RealName,
       (SELECT " + SqlDialect.Top(1) + @"o.F_FullName FROM sys_organize o
          WHERE o.F_CategoryId = 'banji' AND ISNULL(o.F_DeleteMark,0)=0
            AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE ('%,' + o.F_Id COLLATE Chinese_PRC_CI_AS + ',%')" + SqlDialect.Limit(1) + @") AS F_ClassName,
       a.F_StartTime, a.F_SubmitTime, a.F_ObjectiveScore, a.F_SubjectiveScore, a.F_TotalScore,
       a.F_Status, a.F_IsPassed, a.F_GradeTime, a.F_GraderId, a.F_GraderName,
       e.F_TotalScore AS F_ExamTotalScore, e.F_PassScore, e.F_Duration,
       (SELECT COUNT(1) FROM school_exam_question q WHERE q.F_ExamId = e.F_Id AND ISNULL(q.F_DeleteMark,0)=0) AS F_QuestionCount
FROM school_exam_record a
INNER JOIN school_exam e ON a.F_ExamId = e.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
WHERE a.F_Id = '" + Q(recordId) + @"' AND ISNULL(a.F_DeleteMark,0)=0";
            var list = await repository.SqlQueryable<ExamRecordExtend>(sql).ToListAsync();
            var data = list.FirstOrDefault();
            if (data == null)
            {
                throw new Exception("考试记录不存在！");
            }
            if (IsStudent && !string.Equals(data.F_StudentId, currentuser.UserId, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("只能查看自己的考试成绩！");
            }
            data.Answers = await GetRecordAnswers(recordId);
            return data;
        }

        /// <summary>
        /// 答题明细（含正确答案、解析、学生答案、得分、评语）
        /// </summary>
        private async Task<List<ExamAnswerExtend>> GetRecordAnswers(string recordId)
        {
            var sql = @"
SELECT an.F_Id, an.F_QuestionId, an.F_QuestionType,
       an.F_Answer AS F_StudentAnswer, an.F_IsCorrect, an.F_Score, an.F_FullScore, an.F_Comment,
       q.F_Content, q.F_OptionA, q.F_OptionB, q.F_OptionC, q.F_OptionD, q.F_Answer, q.F_Analysis,
       eq.F_SortCode
FROM school_exam_answer an
LEFT JOIN school_question q ON an.F_QuestionId = q.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_exam_question eq ON eq.F_ExamId = an.F_ExamId COLLATE Chinese_PRC_CI_AS
     AND eq.F_QuestionId = an.F_QuestionId COLLATE Chinese_PRC_CI_AS
     AND ISNULL(eq.F_DeleteMark,0)=0
WHERE an.F_RecordId = '" + Q(recordId) + @"' AND ISNULL(an.F_DeleteMark,0)=0
ORDER BY ISNULL(eq.F_SortCode,0), an.F_CreatorTime";
            var list = await repository.SqlQueryable<ExamAnswerExtend>(sql).ToListAsync();
            foreach (var item in list)
            {
                item.F_TypeName = GetTypeName(item.F_QuestionType);
            }
            return list;
        }

        /// <summary>
        /// 人工批改（仅批改简答题，重新汇总主观分与总分）
        /// </summary>
        public async Task GradeRecord(string recordId, string answersJson)
        {
            if (IsStudent)
            {
                throw new Exception("学生不能批改试卷！");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new Exception("考试记录不存在！");
            }
            var rows = string.IsNullOrEmpty(answersJson)
                ? new List<ExamGradeSubmitRow>()
                : answersJson.ToObject<List<ExamGradeSubmitRow>>();
            if (rows == null || rows.Count == 0)
            {
                throw new Exception("没有可保存的批改结果！");
            }
            var record = await repository.Db.Queryable<ExamRecordEntity>()
                .FirstAsync(a => a.F_Id == recordId && a.F_DeleteMark == false);
            if (record == null)
            {
                throw new Exception("考试记录不存在！");
            }
            if (record.F_SubmitTime == null)
            {
                throw new Exception("该记录尚未交卷，不能批改！");
            }
            var exam = await repository.Db.Queryable<ExamEntity>()
                .FirstAsync(a => a.F_Id == record.F_ExamId && a.F_DeleteMark == false);
            if (exam == null)
            {
                throw new Exception("考试不存在或已删除！");
            }
            var answers = await repository.Db.Queryable<ExamAnswerEntity>()
                .Where(a => a.F_RecordId == recordId && a.F_DeleteMark == false)
                .ToListAsync();
            if (answers.Count == 0)
            {
                throw new Exception("没有找到答题明细！");
            }

            repository.Db.Ado.BeginTran();
            try
            {
                foreach (var row in rows)
                {
                    if (row == null || string.IsNullOrEmpty(row.F_Id))
                    {
                        continue;
                    }
                    var answer = answers.FirstOrDefault(a => a.F_Id == row.F_Id);
                    if (answer == null)
                    {
                        throw new Exception("答题明细不存在，请刷新后重试！");
                    }
                    if (answer.F_QuestionType != 4)
                    {
                        //客观题已自动判分，不允许人工修改
                        continue;
                    }
                    var fullScore = answer.F_FullScore ?? 0;
                    var score = row.F_Score ?? 0;
                    if (score < 0 || score > fullScore)
                    {
                        throw new Exception("简答题得分必须在0到" + fullScore.ToString("0.#", CultureInfo.InvariantCulture) + "分之间！");
                    }
                    answer.F_Score = score;
                    //批改完成后不再处于待批改状态，按是否得满分标记对错
                    answer.F_IsCorrect = score >= fullScore ? 1 : 0;
                    answer.F_Comment = string.IsNullOrWhiteSpace(row.F_Comment) ? null : row.F_Comment.Trim();
                    answer.F_LastModifyTime = DateTime.Now;
                    answer.F_LastModifyUserId = currentuser.UserId;
                    await repository.Db.Updateable(answer).ExecuteCommandAsync();
                }

                var subjectiveScore = answers.Where(a => a.F_QuestionType == 4).Sum(a => a.F_Score ?? 0);
                record.F_SubjectiveScore = subjectiveScore;
                record.F_TotalScore = (record.F_ObjectiveScore ?? 0) + subjectiveScore;
                record.F_Status = 1;
                record.F_IsPassed = record.F_TotalScore >= (exam.F_PassScore ?? 0) ? 1 : 0;
                record.F_GraderId = currentuser.UserId;
                record.F_GraderName = currentuser.UserName;
                record.F_GradeTime = DateTime.Now;
                record.F_LastModifyTime = DateTime.Now;
                record.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(record).ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 成绩详情（学生查看自己的成绩；待批改时简答题不返回正确答案与得分）
        /// </summary>
        public async Task<ExamRecordExtend> GetResult(string recordId)
        {
            var data = await GetRecordForm(recordId);
            if ((data.F_Status ?? 0) == 0)
            {
                foreach (var item in data.Answers.Where(a => a.F_QuestionType == 4))
                {
                    item.F_Answer = null;
                    item.F_Analysis = null;
                    item.F_Score = null;
                }
            }
            return data;
        }

        /// <summary>
        /// 错题列表（学生只看自己的错题，教师查看全部错题）
        /// </summary>
        public async Task<List<WrongQuestionExtend>> GetWrongList(SoulPage<WrongQuestionExtend> pagination, string keyword, string courseId)
        {
            ApplySort(pagination, new HashSet<string>
            {
                "F_CourseName", "F_QuestionType", "F_Content", "F_ExamTitle", "F_CreateTime", "F_StudentName"
            }, "F_CreateTime", "desc");

            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.F_Id, a.F_QuestionId, e.F_Title AS F_ExamTitle, c.F_ItemName AS F_CourseName,
       a.F_QuestionType, q.F_Content, q.F_OptionA, q.F_OptionB, q.F_OptionC, q.F_OptionD,
       q.F_Answer, a.F_Answer AS F_StudentAnswer, q.F_Analysis,
       r.F_SubmitTime AS F_CreateTime, u.F_RealName AS F_StudentName
FROM school_exam_answer a
INNER JOIN school_exam_record r ON a.F_RecordId = r.F_Id COLLATE Chinese_PRC_CI_AS
INNER JOIN school_exam e ON a.F_ExamId = e.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_question q ON a.F_QuestionId = q.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON e.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON r.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0)=0 AND ISNULL(r.F_DeleteMark,0)=0
  AND ISNULL(a.F_IsCorrect,0) = 0");
            if (IsStudent)
            {
                sb.Append(" AND r.F_StudentId = '" + Q(currentuser.UserId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (q.F_Content COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR e.F_Title COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND e.F_CourseId = '" + Q(courseId) + "'");
            }
            return await repository.SqlQueryable<WrongQuestionExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        #endregion
    }
}
