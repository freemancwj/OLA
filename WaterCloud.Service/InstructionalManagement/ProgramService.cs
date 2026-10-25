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
    /// 培养方案服务（专业+学期+课程）
    /// </summary>
    public class ProgramService : BaseService<ProgramEntity>, IDenpendency
    {
        public ProgramService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<ProgramExtend>> GetLookList(SoulPage<ProgramExtend> pagination, string keyword, string majorId, string semester)
        {
            var allowedFields = new HashSet<string> { "F_MajorName", "F_Semester", "F_CourseName", "F_Credit", "F_CreatorTime" };
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
SELECT a.*, o.F_FullName AS F_MajorName, c.F_Name AS F_CourseName
FROM school_program a
LEFT JOIN sys_organize o ON a.F_MajorId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR a.F_Semester COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(majorId))
            {
                sb.Append(" AND a.F_MajorId = '" + Q(majorId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(semester))
            {
                sb.Append(" AND a.F_Semester = N'" + Q(semester.Trim()) + "'");
            }
            return await repository.Db.SqlQueryable<ProgramExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<ProgramEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 专业下拉
        /// </summary>
        public async Task<List<OrganizeEntity>> GetMajorList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "zhuanye" && a.F_DeleteMark == false && a.F_EnabledMark == true)
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

        #endregion

        #region 提交数据

        public async Task SubmitForm(ProgramEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_MajorId))
            {
                throw new Exception("请选择专业");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Semester))
            {
                throw new Exception("请填写学期");
            }
            if (string.IsNullOrWhiteSpace(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_MajorId == entity.F_MajorId && a.F_Semester == entity.F_Semester
                    && a.F_CourseId == entity.F_CourseId && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该专业此学期已存在相同课程，请勿重复添加");
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
                data.F_MajorId = entity.F_MajorId;
                data.F_Semester = entity.F_Semester;
                data.F_CourseId = entity.F_CourseId;
                data.F_Credit = entity.F_Credit;
                data.F_Period = entity.F_Period;
                data.F_Nature = entity.F_Nature;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ProgramEntity
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
