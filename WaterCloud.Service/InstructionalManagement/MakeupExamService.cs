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

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 补考管理服务
    /// </summary>
    public class MakeupExamService : BaseService<MakeupExamEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public MakeupExamService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 补考名单列表（分页）
        /// </summary>
        public async Task<List<MakeupExamExtend>> GetLookList(SoulPage<MakeupExamExtend> pagination, string keyword, string classId, string courseId, int? status)
        {
            var allowedFields = new HashSet<string> { "F_StudentName", "F_Account", "F_ExamDate", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sb = BuildBaseSql();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (st.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR st.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (status != null)
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + status.Value);
            }
            return await repository.SqlQueryable<MakeupExamExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 单条记录（含学生、课程等名称，供表单只读展示）
        /// </summary>
        public async Task<MakeupExamExtend> GetForm(string keyValue)
        {
            var sb = BuildBaseSql();
            sb.Append(" AND a.F_Id = '" + Q(keyValue) + "'");
            return await repository.SqlQueryable<MakeupExamExtend>(sb.ToString()).FirstAsync();
        }

        /// <summary>
        /// 班级下拉
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_EnCode)
                .ToListAsync();
        }

        /// <summary>
        /// 课程下拉
        /// </summary>
        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        /// <summary>
        /// 教室下拉（含教学楼、容量）
        /// </summary>
        public async Task<List<ClassroomOptionExtend>> GetClassroomList()
        {
            return await repository.SqlQueryable<ClassroomOptionExtend>(@"
SELECT c.F_Id, c.F_Name, c.F_Capacity, b.F_Name AS F_BuildingName
FROM school_classroom c
LEFT JOIN school_building b ON c.F_BuildingId = b.F_Id COLLATE Chinese_PRC_CI_AS
ORDER BY b.F_Name, c.F_Name").ToListAsync();
        }

        /// <summary>
        /// 考试时间段下拉
        /// </summary>
        public async Task<List<PeriodOptionExtend>> GetPeriodList()
        {
            return await repository.SqlQueryable<PeriodOptionExtend>(@"
SELECT p.F_Id, p.F_Name,
       CONVERT(varchar(5), p.F_StartTime, 108) AS F_StartTime,
       CONVERT(varchar(5), p.F_EndTime, 108) AS F_EndTime
FROM school_exam_period p
ORDER BY p.F_StartTime").ToListAsync();
        }

        /// <summary>
        /// 教师下拉（非学生、非删除的启用用户）
        /// </summary>
        public async Task<List<UserEntity>> GetTeacherList(string keyword)
        {
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && (a.F_RoleId == null || !a.F_RoleId.Contains(StudentRoleId)));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_RealName.Contains(kw) || a.F_Account.Contains(kw));
            }
            return await query.OrderBy(a => a.F_Account).Take(200).ToListAsync();
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 从成绩表生成补考名单（审核通过且不及格，排除已在补考名单中的学生）
        /// </summary>
        public async Task<int> GenerateList(string classId, string courseId, string semester)
        {
            var sb = new StringBuilder(@"
SELECT s.F_Id AS F_ScoreId, s.F_StudentId, s.F_ClassId, s.F_CourseId
FROM school_scores s
WHERE ISNULL(s.F_DeleteMark,0) = 0 AND ISNULL(s.F_AuditStatus,0) = 1 AND s.F_Score < 60
  AND NOT EXISTS (SELECT 1 FROM school_makeup_exam m
                  WHERE ISNULL(m.F_DeleteMark,0) = 0
                    AND m.F_StudentId = s.F_StudentId COLLATE Chinese_PRC_CI_AS
                    AND m.F_CourseId = s.F_CourseId COLLATE Chinese_PRC_CI_AS)");
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND s.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND s.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!string.IsNullOrEmpty(semester))
            {
                sb.Append(" AND s.F_Semester = '" + Q(semester.Trim()) + "'");
            }
            var rows = await repository.SqlQueryable<MakeupExamEntity>(sb.ToString()).ToListAsync();
            if (rows.Count == 0)
            {
                throw new Exception("没有需要补考的学生");
            }
            var list = new List<MakeupExamEntity>();
            foreach (var row in rows)
            {
                var entity = new MakeupExamEntity
                {
                    F_StudentId = row.F_StudentId,
                    F_ClassId = row.F_ClassId,
                    F_CourseId = row.F_CourseId,
                    F_ScoreId = row.F_ScoreId,
                    F_Status = 0,
                    F_DeleteMark = false,
                    F_EnabledMark = true
                };
                entity.Create();
                list.Add(entity);
            }
            await repository.Insert(list);
            return list.Count;
        }

        /// <summary>
        /// 仅允许修改安排信息与录分
        /// </summary>
        public async Task SubmitForm(MakeupExamEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("补考记录请通过生成补考名单创建");
            }
            var data = await repository.FindEntity(keyValue);
            if (data == null || data.F_DeleteMark == true)
            {
                throw new Exception("记录不存在");
            }
            data.F_ExamDate = entity.F_ExamDate;
            data.F_PeriodId = entity.F_PeriodId;
            data.F_ClassroomId = entity.F_ClassroomId;
            data.F_MainInvigilatorId = entity.F_MainInvigilatorId;
            data.F_SubInvigilatorId = entity.F_SubInvigilatorId;
            data.F_Remark = entity.F_Remark;
            if (entity.F_NewScore != null)
            {
                //录分
                data.F_NewScore = entity.F_NewScore;
                data.F_Status = 2;
            }
            else if (data.F_ExamDate != null && !string.IsNullOrEmpty(data.F_PeriodId)
                && !string.IsNullOrEmpty(data.F_ClassroomId) && !string.IsNullOrEmpty(data.F_MainInvigilatorId))
            {
                //安排信息齐全
                data.F_Status = 1;
            }
            //教室占用冲突（日期/教室/时段齐全时检测）
            if (data.F_ExamDate != null && !string.IsNullOrEmpty(data.F_ClassroomId) && !string.IsNullOrEmpty(data.F_PeriodId))
            {
                var conflict = await repository.IQueryable()
                    .Where(a => a.F_ClassroomId == data.F_ClassroomId && a.F_ExamDate == data.F_ExamDate && a.F_PeriodId == data.F_PeriodId
                        && a.F_DeleteMark == false && a.F_Id != keyValue)
                    .AnyAsync();
                if (conflict)
                {
                    throw new Exception("该教室在此时间段已被占用");
                }
            }
            data.Modify(keyValue);
            await repository.Update(data);
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new MakeupExamEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        /// <summary>
        /// 补考名单公共查询SQL
        /// </summary>
        private static StringBuilder BuildBaseSql()
        {
            return new StringBuilder(@"
SELECT a.F_Id, a.F_StudentId, a.F_CourseId, a.F_ClassId, a.F_ScoreId, a.F_ExamDate, a.F_PeriodId,
       a.F_ClassroomId, a.F_MainInvigilatorId, a.F_SubInvigilatorId, a.F_Status, a.F_NewScore, a.F_Remark, a.F_CreatorTime,
       st.F_RealName AS F_StudentName, st.F_Account,
       o.F_FullName AS F_ClassName,
       c.F_Name AS F_CourseName,
       ISNULL(b.F_Name,'') + '-' + ISNULL(cr.F_Name,'') AS F_ClassroomName,
       ISNULL(p.F_Name,'') + '(' + CONVERT(varchar(5), p.F_StartTime, 108) + '-' + CONVERT(varchar(5), p.F_EndTime, 108) + ')' AS F_PeriodName,
       mu.F_RealName AS F_MainInvigilatorName,
       su.F_RealName AS F_SubInvigilatorName,
       sc.F_Score AS F_OldScore
FROM school_makeup_exam a
LEFT JOIN sys_user st ON a.F_StudentId = st.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_ClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_building b ON cr.F_BuildingId = b.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_exam_period p ON a.F_PeriodId = p.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user mu ON a.F_MainInvigilatorId = mu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user su ON a.F_SubInvigilatorId = su.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_scores sc ON a.F_ScoreId = sc.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
