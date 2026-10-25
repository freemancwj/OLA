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
    /// 节次起止时间服务
    /// </summary>
    public class PeriodTimeService : BaseService<PeriodTimeEntity>, IDenpendency
    {
        public PeriodTimeService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<PeriodTimeEntity>> GetList()
        {
            return await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false)
                .OrderBy(a => a.F_Section)
                .ToListAsync();
        }

        public async Task<PeriodTimeEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(PeriodTimeEntity entity, string keyValue)
        {
            if (entity == null || entity.F_Section == null || entity.F_Section <= 0)
            {
                throw new Exception("请填写第几节");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_Section == entity.F_Section && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该节次已存在，请勿重复添加");
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
                data.F_Section = entity.F_Section;
                data.F_StartTime = entity.F_StartTime;
                data.F_EndTime = entity.F_EndTime;
                data.F_SortCode = entity.F_SortCode;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new PeriodTimeEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
