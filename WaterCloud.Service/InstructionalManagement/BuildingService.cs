using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 教学楼服务
    /// </summary>
    public class BuildingService : BaseService<BuildingEntity>, IDenpendency
    {
        public BuildingService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 教学楼列表（分页）
        /// </summary>
        public async Task<List<BuildingEntity>> GetLookList(SoulPage<BuildingEntity> pagination, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_Name", "F_Location", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_Name";
                pagination.order = "asc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "asc";
            }
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_Name.Contains(kw) || a.F_Location.Contains(kw));
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<BuildingEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 教学楼下拉列表
        /// </summary>
        public async Task<List<BuildingEntity>> GetBuildingList()
        {
            return await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(BuildingEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("教学楼名称不能为空");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_Name == entity.F_Name && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("教学楼名称已存在");
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
                data.F_Name = entity.F_Name;
                data.F_Location = entity.F_Location;
                data.F_Description = entity.F_Description;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new BuildingEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
