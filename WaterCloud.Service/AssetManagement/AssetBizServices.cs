using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;

namespace WaterCloud.Service.AssetManagement
{
    /// <summary>器材借用归还</summary>
    public class AstBorrowService : BaseService<AstBorrowEntity>, IDenpendency
    {
        public AstBorrowService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstBorrowEntity>> GetList(SoulPage<AstBorrowEntity> pagination, string keyword, string status)
        {
            var q = repository.Db.Queryable<AstBorrowEntity, AstAssetEntity>((b, a) => new object[] { JoinType.Left, b.F_AssetId == a.F_Id })
                .Where((b, a) => b.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where((b, a) => a.F_AssetName.Contains(keyword) || b.F_BorrowerName.Contains(keyword));
            if (!string.IsNullOrEmpty(status)) q = q.Where((b, a) => b.F_Status == status);
            pagination.count = await q.CountAsync();
            return await q.OrderBy((b, a) => b.F_CreatorTime, OrderByType.Desc)
                .Select((b, a) => b).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstBorrowEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstBorrowEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_BorrowDate = DateTime.Now;
                entity.F_Status = "借出";
                entity.F_DeleteMark = false;
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
                await repository.Db.Updateable<AstAssetEntity>()
                    .SetColumns(a => new AstAssetEntity { F_Status = "借用中", F_LastModifyTime = DateTime.Now })
                    .Where(a => a.F_Id == entity.F_AssetId).ExecuteCommandAsync();
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue) => await repository.Delete(a => a.F_Id == keyValue);

        public async Task ReturnAsset(string keyValue, string damageNote)
        {
            var b = await repository.FindEntity(keyValue);
            if (b == null) throw new Exception("借用记录不存在");
            b.F_ActualReturnDate = DateTime.Now;
            b.F_Status = string.IsNullOrEmpty(damageNote) ? "已归还" : "损坏";
            b.F_DamageNote = damageNote;
            b.F_LastModifyTime = DateTime.Now;
            b.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(b);
            await repository.Db.Updateable<AstAssetEntity>()
                .SetColumns(a => new AstAssetEntity { F_Status = "在用", F_LastModifyTime = DateTime.Now })
                .Where(a => a.F_Id == b.F_AssetId).ExecuteCommandAsync();
            if (!string.IsNullOrEmpty(damageNote))
            {
                await repository.Db.Updateable<AstAssetEntity>()
                    .SetColumns(a => new AstAssetEntity { F_DamageCount = (a.F_DamageCount ?? 0) + 1 })
                    .Where(a => a.F_Id == b.F_AssetId).ExecuteCommandAsync();
            }
        }
    }

    /// <summary>公物报修</summary>
    public class AstRepairService : BaseService<AstRepairEntity>, IDenpendency
    {
        public AstRepairService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstRepairEntity>> GetList(SoulPage<AstRepairEntity> pagination, string keyword, string status)
        {
            var q = repository.Db.Queryable<AstRepairEntity, AstAssetEntity>((r, a) => new object[] { JoinType.Left, r.F_AssetId == a.F_Id })
                .Where((r, a) => r.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where((r, a) => a.F_AssetName.Contains(keyword) || r.F_ReporterName.Contains(keyword));
            if (!string.IsNullOrEmpty(status)) q = q.Where((r, a) => r.F_Status == status);
            pagination.count = await q.CountAsync();
            return await q.OrderBy((r, a) => r.F_CreatorTime, OrderByType.Desc)
                .Select((r, a) => r).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstRepairEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstRepairEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_ReportTime = DateTime.Now;
                entity.F_Status = "待受理";
                entity.F_DeleteMark = false;
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue) => await repository.Delete(a => a.F_Id == keyValue);

        public async Task AcceptRepair(string keyValue, string repairer)
        {
            var r = await repository.FindEntity(keyValue);
            if (r == null) throw new Exception("报修单不存在");
            r.F_Status = "维修中";
            r.F_Repairer = repairer;
            r.F_LastModifyTime = DateTime.Now;
            r.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(r);
            await repository.Db.Updateable<AstAssetEntity>()
                .SetColumns(a => new AstAssetEntity { F_Status = "维修中" })
                .Where(a => a.F_Id == r.F_AssetId).ExecuteCommandAsync();
        }

        public async Task FinishRepair(string keyValue, decimal cost, string parts, string result)
        {
            var r = await repository.FindEntity(keyValue);
            if (r == null) throw new Exception("报修单不存在");
            r.F_Status = "已完成";
            r.F_RepairCost = cost;
            r.F_PartsReplaced = parts;
            r.F_RepairResult = result;
            r.F_RepairTime = DateTime.Now;
            r.F_LastModifyTime = DateTime.Now;
            r.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(r);
            await repository.Db.Updateable<AstAssetEntity>()
                .SetColumns(a => new AstAssetEntity { F_Status = "在用", F_DamageCount = (a.F_DamageCount ?? 0) + 1 })
                .Where(a => a.F_Id == r.F_AssetId).ExecuteCommandAsync();
        }
    }

    /// <summary>设备维保</summary>
    public class AstMaintenanceService : BaseService<AstMaintenanceEntity>, IDenpendency
    {
        public AstMaintenanceService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstMaintenanceEntity>> GetList(SoulPage<AstMaintenanceEntity> pagination, string assetId)
        {
            var q = repository.Db.Queryable<AstMaintenanceEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(assetId)) q = q.Where(a => a.F_AssetId == assetId);
            pagination.count = await q.CountAsync();
            return await q.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstMaintenanceEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstMaintenanceEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_DeleteMark = false;
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue) => await repository.Delete(a => a.F_Id == keyValue);
    }

    /// <summary>低值耗材 + 出入库流水</summary>
    public class AstConsumableService : BaseService<AstConsumableEntity>, IDenpendency
    {
        public AstConsumableService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstConsumableEntity>> GetList(SoulPage<AstConsumableEntity> pagination, string keyword)
        {
            var q = repository.Db.Queryable<AstConsumableEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where(a => a.F_Name.Contains(keyword) || a.F_Spec.Contains(keyword));
            pagination.count = await q.CountAsync();
            return await q.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstConsumableEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstConsumableEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_StockQty = entity.F_StockQty ?? 0;
                entity.F_DeleteMark = false;
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue) => await repository.Delete(a => a.F_Id == keyValue);

        public async Task OperateStock(AstConsumableRecordEntity record)
        {
            var c = await repository.Db.Queryable<AstConsumableEntity>().Where(a => a.F_Id == record.F_ConsumableId).FirstAsync();
            if (c == null) throw new Exception("耗材不存在");
            var qty = record.F_Qty ?? 0;
            if (record.F_OperateType == "领用" && (c.F_StockQty ?? 0) < qty)
                throw new Exception("库存不足");
            record.F_Id = Utils.GuId();
            record.F_OperatorId = currentuser.UserId;
            record.F_OperateTime = DateTime.Now;
            record.F_DeleteMark = false;
            record.F_CreatorTime = DateTime.Now;
            record.F_CreatorUserId = currentuser.UserId;
            await repository.Db.Insertable(record).ExecuteCommandAsync();
            var newQty = record.F_OperateType == "入库" ? (c.F_StockQty ?? 0) + qty : (c.F_StockQty ?? 0) - qty;
            await repository.Db.Updateable<AstConsumableEntity>()
                .SetColumns(a => new AstConsumableEntity { F_StockQty = newQty, F_LastModifyTime = DateTime.Now })
                .Where(a => a.F_Id == record.F_ConsumableId).ExecuteCommandAsync();
        }

        public async Task<List<AstConsumableRecordEntity>> GetRecordList(SoulPage<AstConsumableRecordEntity> pagination, string consumableId)
        {
            var q = repository.Db.Queryable<AstConsumableRecordEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(consumableId)) q = q.Where(a => a.F_ConsumableId == consumableId);
            pagination.count = await q.CountAsync();
            return await q.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToPageListAsync(pagination.page, pagination.rows);
        }
    }

    /// <summary>学期盘点（主表+明细）</summary>
    public class AstInventoryService : BaseService<AstInventoryEntity>, IDenpendency
    {
        public AstInventoryService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstInventoryEntity>> GetList(SoulPage<AstInventoryEntity> pagination, string keyword)
        {
            var q = repository.Db.Queryable<AstInventoryEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where(a => (a.F_Term ?? "").Contains(keyword));
            pagination.count = await q.CountAsync();
            return await q.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstInventoryEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstInventoryEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_Status = "草稿";
                entity.F_CreateTime = DateTime.Now;
                entity.F_DeleteMark = false;
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        /// <summary>生成盘点明细：按年级/班级拉取在用资产</summary>
        public async Task<string> GenerateItems(string inventoryId, string gradeId, string classId)
        {
            var inv = await repository.FindEntity(inventoryId);
            if (inv == null) throw new Exception("盘点单不存在");
            var q = repository.Db.Queryable<AstAssetEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_Status == "在用");
            if (!string.IsNullOrEmpty(gradeId)) q = q.Where(a => a.F_GradeId == gradeId);
            if (!string.IsNullOrEmpty(classId)) q = q.Where(a => a.F_ClassId == classId);
            var assets = await q.ToListAsync();
            int n = 0;
            foreach (var a in assets)
            {
                var item = new AstInventoryItemEntity
                {
                    F_Id = Utils.GuId(),
                    F_InventoryId = inventoryId,
                    F_AssetId = a.F_Id,
                    F_GradeId = a.F_GradeId,
                    F_ClassId = a.F_ClassId,
                    F_SystemStatus = a.F_Status,
                    F_DiffResult = "一致"
                };
                await repository.Db.Insertable(item).ExecuteCommandAsync();
                n++;
            }
            inv.F_Status = "盘点中";
            inv.F_LastModifyTime = DateTime.Now;
            await repository.Update(inv);
            return $"已生成 {n} 条盘点明细";
        }

        public async Task<List<AstInventoryItemEntity>> GetItems(string inventoryId)
            => await repository.Db.Queryable<AstInventoryItemEntity>().Where(a => a.F_InventoryId == inventoryId).ToListAsync();

        /// <summary>确认盘点结果（盘盈/盘亏/一致）</summary>
        public async Task ConfirmItem(string itemId, string actualStatus, string remark)
        {
            var item = await repository.Db.Queryable<AstInventoryItemEntity>().Where(a => a.F_Id == itemId).FirstAsync();
            if (item == null) throw new Exception("明细不存在");
            item.F_ActualStatus = actualStatus;
            item.F_DiffResult = actualStatus == item.F_SystemStatus ? "一致" : (actualStatus == "在用" ? "盘盈" : "盘亏");
            item.F_HandlerId = currentuser.UserId;
            item.F_ConfirmTime = DateTime.Now;
            item.F_Remark = remark;
            await repository.Db.Updateable(item).ExecuteCommandAsync();
        }

        public async Task FinishInventory(string inventoryId)
        {
            var inv = await repository.FindEntity(inventoryId);
            if (inv == null) throw new Exception("盘点单不存在");
            inv.F_Status = "已完成";
            inv.F_FinishTime = DateTime.Now;
            inv.F_LastModifyTime = DateTime.Now;
            inv.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(inv);
        }
    }
}
