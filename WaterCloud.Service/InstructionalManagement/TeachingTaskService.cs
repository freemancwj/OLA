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
    /// 教学任务服务（学期+教师+课程+班级+周学时）
    /// </summary>
    public class TeachingTaskService : BaseService<TeachingTaskEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public TeachingTaskService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<TeachingTaskExtend>> GetLookList(SoulPage<TeachingTaskExtend> pagination, string keyword, string semester, string classId)
        {
            var allowedFields = new HashSet<string> { "F_Semester", "F_TeacherName", "F_CourseName", "F_ClassName", "F_WeekPeriod", "F_CreatorTime" };
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
SELECT a.*, u.F_RealName AS F_TeacherName, c.F_Name AS F_CourseName, o.F_FullName AS F_ClassName
FROM school_teaching_task a
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrWhiteSpace(semester))
            {
                sb.Append(" AND a.F_Semester = N'" + Q(semester.Trim()) + "'");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            return await repository.Db.SqlQueryable<TeachingTaskExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<TeachingTaskEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

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

        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_EnCode)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(TeachingTaskEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Semester))
            {
                throw new Exception("请填写学期");
            }
            if (string.IsNullOrWhiteSpace(entity.F_TeacherId))
            {
                throw new Exception("请选择教师");
            }
            if (string.IsNullOrWhiteSpace(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            if (string.IsNullOrWhiteSpace(entity.F_ClassId))
            {
                throw new Exception("请选择班级");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_Semester == entity.F_Semester && a.F_TeacherId == entity.F_TeacherId
                    && a.F_CourseId == entity.F_CourseId && a.F_ClassId == entity.F_ClassId && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该学期已存在相同的教学任务，请勿重复添加");
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
                data.F_Semester = entity.F_Semester;
                data.F_TeacherId = entity.F_TeacherId;
                data.F_CourseId = entity.F_CourseId;
                data.F_ClassId = entity.F_ClassId;
                data.F_WeekPeriod = entity.F_WeekPeriod;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new TeachingTaskEntity
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
