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
    /// 可选课程服务（选课批次下开放的课程班次）
    /// </summary>
    public class ElectiveCourseService : BaseService<ElectiveCourseEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public ElectiveCourseService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<ElectiveCourseExtend>> GetLookList(SoulPage<ElectiveCourseExtend> pagination, string roundId, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_CourseName", "F_TeacherName", "F_Capacity", "F_SelectedCount", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, r.F_Name AS F_RoundName, c.F_Name AS F_CourseName, u.F_RealName AS F_TeacherName,
       cr.F_Name AS F_ClassroomName,
       (SELECT COUNT(1) FROM school_elective_record x WHERE x.F_ElectiveCourseId = a.F_Id COLLATE Chinese_PRC_CI_AS
           AND x.F_Status = 1 AND ISNULL(x.F_DeleteMark,0) = 0) AS F_SelectedCount
FROM school_elective_course a
LEFT JOIN school_elective_round r ON a.F_RoundId = r.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_ClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrEmpty(roundId))
            {
                sb.Append(" AND a.F_RoundId = '" + Q(roundId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.Db.SqlQueryable<ElectiveCourseExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<ElectiveCourseEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetTeacherList()
        {
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && (a.F_RoleId == null || !a.F_RoleId.Contains(StudentRoleId)));
            return await query.OrderBy(a => a.F_Account).ToListAsync();
        }

        public async Task<List<ClassroomEntity>> GetClassroomList()
        {
            return await repository.Db.Queryable<ClassroomEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ElectiveCourseEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_RoundId))
            {
                throw new Exception("请选择选课批次");
            }
            if (string.IsNullOrWhiteSpace(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            if (string.IsNullOrWhiteSpace(entity.F_TeacherId))
            {
                throw new Exception("请选择授课教师");
            }
            if (entity.F_Capacity == null || entity.F_Capacity <= 0)
            {
                throw new Exception("请填写容量");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_RoundId == entity.F_RoundId && a.F_CourseId == entity.F_CourseId
                    && a.F_TeacherId == entity.F_TeacherId && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该批次下已存在相同课程与教师的班次");
            }
            if (string.IsNullOrEmpty(entity.F_ClassroomId))
            {
                entity.F_ClassroomId = null;
            }
            if (string.IsNullOrEmpty(keyValue))
            {
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
                //已选人数超过新容量时不允许
                var selected = await repository.Db.Queryable<ElectiveRecordEntity>()
                    .Where(a => a.F_ElectiveCourseId == keyValue && a.F_Status == 1 && a.F_DeleteMark == false)
                    .CountAsync();
                if (entity.F_Capacity < selected)
                {
                    throw new Exception("已选人数（" + selected + "）超过新容量，无法修改");
                }
                data.F_RoundId = entity.F_RoundId;
                data.F_CourseId = entity.F_CourseId;
                data.F_TeacherId = entity.F_TeacherId;
                data.F_Capacity = entity.F_Capacity;
                data.F_WeekDay = entity.F_WeekDay;
                data.F_Section = entity.F_Section;
                data.F_ClassroomId = entity.F_ClassroomId;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var hasSelected = await repository.Db.Queryable<ElectiveRecordEntity>()
                .Where(a => ids.Contains(a.F_ElectiveCourseId) && a.F_Status == 1 && a.F_DeleteMark == false)
                .AnyAsync();
            if (hasSelected)
            {
                throw new Exception("已有学生选课的班次不允许删除");
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ElectiveCourseEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
