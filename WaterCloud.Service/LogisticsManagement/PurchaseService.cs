using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.LogisticsManagement;

namespace WaterCloud.Service.LogisticsManagement
{
    /// <summary>
    /// 采购申请服务
    /// </summary>
    public class PurchaseService : BaseService<PurchaseEntity>, IDenpendency
    {
        public PurchaseService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<PurchaseEntity>> GetLookList(SoulPage<PurchaseEntity> pagination, string keyword, string status)
        {
            var allowedFields = new HashSet<string> { "F_ItemName", "F_ApplyDate", "F_CreatorTime", "F_Status" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_ItemName.Contains(kw) || a.F_Supplier.Contains(kw) || a.F_ApplicantName.Contains(kw));
            }
            if (!string.IsNullOrEmpty(status))
            {
                var s = int.Parse(status);
                query = query.Where(a => a.F_Status == s);
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<PurchaseEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(PurchaseEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_ItemName))
            {
                throw new Exception("物品名称不能为空");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.F_Status = entity.F_Status ?? 0;
                entity.F_ApplyDate = entity.F_ApplyDate ?? DateTime.Now;
                entity.F_ApplicantId = currentuser.UserId;
                entity.F_ApplicantName = currentuser.UserName;
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
                data.F_ItemName = entity.F_ItemName;
                data.F_Spec = entity.F_Spec;
                data.F_Quantity = entity.F_Quantity;
                data.F_Unit = entity.F_Unit;
                data.F_BudgetAmount = entity.F_BudgetAmount;
                data.F_Supplier = entity.F_Supplier;
                data.F_Status = entity.F_Status;
                data.F_ApplyDate = entity.F_ApplyDate;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new PurchaseEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
