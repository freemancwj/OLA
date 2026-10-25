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
    /// 考务安排服务
    /// </summary>
    public class ExamArrangeService : BaseService<ExamArrangeEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public ExamArrangeService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 考务安排列表（分页）
        /// </summary>
        public async Task<List<ExamArrangeExtend>> GetLookList(SoulPage<ExamArrangeExtend> pagination, string keyword, string classId, string courseId, string examDate)
        {
            SetOrder(pagination);
            var sb = BuildBaseSql();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_ExamName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!string.IsNullOrEmpty(examDate))
            {
                sb.Append(" AND CONVERT(varchar(10), a.F_ExamDate, 23) = '" + Q(examDate) + "'");
            }
            return await repository.Db.SqlQueryable<ExamArrangeExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 监考管理列表（非管理员强制只查自己监考的场次）
        /// </summary>
        public async Task<List<ExamArrangeExtend>> GetInvigilatorList(SoulPage<ExamArrangeExtend> pagination, string teacherId, string keyword)
        {
            SetOrder(pagination);
            var sb = BuildBaseSql();
            if (currentuser == null || (!currentuser.IsSuperAdmin && !currentuser.IsAdmin))
            {
                var uid = Q(currentuser == null ? "" : currentuser.UserId);
                sb.Append(" AND (a.F_MainInvigilatorId = '" + uid + "' OR a.F_SubInvigilatorId = '" + uid + "')");
            }
            else if (!string.IsNullOrEmpty(teacherId))
            {
                sb.Append(" AND (a.F_MainInvigilatorId = '" + Q(teacherId) + "' OR a.F_SubInvigilatorId = '" + Q(teacherId) + "')");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_ExamName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.Db.SqlQueryable<ExamArrangeExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<ExamArrangeEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
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
        /// 教室下拉（含教学楼、容量）
        /// </summary>
        public async Task<List<ClassroomOptionExtend>> GetClassroomList()
        {
            return await repository.Db.SqlQueryable<ClassroomOptionExtend>(@"
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
            return await repository.Db.SqlQueryable<PeriodOptionExtend>(@"
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

        public async Task SubmitForm(ExamArrangeEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_ExamName))
            {
                throw new Exception("请填写考试名称");
            }
            if (string.IsNullOrWhiteSpace(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            if (string.IsNullOrWhiteSpace(entity.F_ClassId))
            {
                throw new Exception("请选择班级");
            }
            if (string.IsNullOrWhiteSpace(entity.F_ClassroomId))
            {
                throw new Exception("请选择教室");
            }
            if (entity.F_ExamDate == null)
            {
                throw new Exception("请选择考试日期");
            }
            if (string.IsNullOrWhiteSpace(entity.F_PeriodId))
            {
                throw new Exception("请选择考试时间段");
            }
            if (string.IsNullOrWhiteSpace(entity.F_MainInvigilatorId))
            {
                throw new Exception("请选择主监考");
            }
            //教室占用冲突：同教室+同日期+同时间段
            var roomConflict = await repository.IQueryable()
                .Where(a => a.F_ClassroomId == entity.F_ClassroomId && a.F_ExamDate == entity.F_ExamDate && a.F_PeriodId == entity.F_PeriodId
                    && a.F_DeleteMark == false && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (roomConflict)
            {
                throw new Exception("该教室在此时间段已被占用");
            }
            //班级安排冲突：同班级+同日期+同时间段
            var classConflict = await repository.IQueryable()
                .Where(a => a.F_ClassId == entity.F_ClassId && a.F_ExamDate == entity.F_ExamDate && a.F_PeriodId == entity.F_PeriodId
                    && a.F_DeleteMark == false && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (classConflict)
            {
                throw new Exception("该班级在此时间段已有考试安排");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Status = entity.F_Status ?? 0;
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                var data = await repository.FindEntity(keyValue);
                if (data == null || data.F_DeleteMark == true)
                {
                    throw new Exception("记录不存在");
                }
                data.F_ExamName = entity.F_ExamName;
                data.F_CourseId = entity.F_CourseId;
                data.F_ClassId = entity.F_ClassId;
                data.F_ClassroomId = entity.F_ClassroomId;
                data.F_ExamDate = entity.F_ExamDate;
                data.F_PeriodId = entity.F_PeriodId;
                data.F_MainInvigilatorId = entity.F_MainInvigilatorId;
                data.F_SubInvigilatorId = entity.F_SubInvigilatorId;
                data.F_Status = entity.F_Status ?? 0;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ExamArrangeEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        /// <summary>
        /// 考务安排公共查询SQL（列表与监考管理共用）
        /// </summary>
        private static StringBuilder BuildBaseSql()
        {
            return new StringBuilder(@"
SELECT a.F_Id, a.F_ExamName, a.F_CourseId, a.F_ClassId, a.F_ClassroomId, a.F_ExamDate, a.F_PeriodId,
       a.F_MainInvigilatorId, a.F_SubInvigilatorId, a.F_Status, a.F_Remark, a.F_CreatorTime,
       c.F_Name AS F_CourseName,
       o.F_FullName AS F_ClassName,
       ISNULL(b.F_Name,'') + '-' + ISNULL(cr.F_Name,'') AS F_ClassroomName,
       ISNULL(p.F_Name,'') + '(' + CONVERT(varchar(5), p.F_StartTime, 108) + '-' + CONVERT(varchar(5), p.F_EndTime, 108) + ')' AS F_PeriodName,
       mu.F_RealName AS F_MainInvigilatorName,
       su.F_RealName AS F_SubInvigilatorName
FROM school_exam_arrange a
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_ClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_building b ON cr.F_BuildingId = b.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_exam_period p ON a.F_PeriodId = p.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user mu ON a.F_MainInvigilatorId = mu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user su ON a.F_SubInvigilatorId = su.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
        }

        /// <summary>
        /// 排序白名单
        /// </summary>
        private static void SetOrder(SoulPage<ExamArrangeExtend> pagination)
        {
            var allowedFields = new HashSet<string> { "F_ExamName", "F_ExamDate", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_ExamDate";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
