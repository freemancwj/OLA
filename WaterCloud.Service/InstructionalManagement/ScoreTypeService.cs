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
    /// 成绩类型服务
    /// </summary>
    public class ScoreTypeService : BaseService<ScoreTypeEntity>, IDenpendency
    {
        public ScoreTypeService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 成绩类型列表（分页）
        /// </summary>
        public async Task<List<ScoreTypeEntity>> GetLookList(SoulPage<ScoreTypeEntity> pagination, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_Name", "F_IncludeStat", "F_SortCode", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_SortCode";
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
                query = query.Where(a => a.F_Name.Contains(kw));
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<ScoreTypeEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ScoreTypeEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("类型名称不能为空");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_Name == entity.F_Name && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("类型名称已存在");
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
                data.F_IncludeStat = entity.F_IncludeStat;
                data.F_SortCode = entity.F_SortCode;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ScoreTypeEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
