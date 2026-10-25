using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 教室服务
    /// </summary>
    public class ClassroomService : BaseService<ClassroomEntity>, IDenpendency
    {
        public ClassroomService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 教室列表（分页，联查教学楼名称）
        /// </summary>
        public async Task<List<ClassroomExtend>> GetLookList(SoulPage<ClassroomExtend> pagination, string keyword, string buildingId)
        {
            var allowedFields = new HashSet<string> { "F_Name", "F_BuildingName", "F_Type", "F_Capacity", "F_CreatorTime" };
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
SELECT a.*, b.F_Name AS F_BuildingName
FROM school_classroom a
LEFT JOIN school_building b ON a.F_BuildingId = b.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR b.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR a.F_Type COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(buildingId))
            {
                sb.Append(" AND a.F_BuildingId = '" + Q(buildingId) + "'");
            }
            return await repository.Db.SqlQueryable<ClassroomExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<ClassroomEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ClassroomEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_BuildingId))
            {
                throw new Exception("请选择教学楼");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("教室名称不能为空");
            }
            //同一教学楼内教室名唯一
            var exists = await repository.IQueryable()
                .Where(a => a.F_BuildingId == entity.F_BuildingId && a.F_Name == entity.F_Name && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该教学楼内已存在同名教室");
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
                data.F_BuildingId = entity.F_BuildingId;
                data.F_Name = entity.F_Name;
                data.F_Capacity = entity.F_Capacity;
                data.F_Type = entity.F_Type;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ClassroomEntity
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
