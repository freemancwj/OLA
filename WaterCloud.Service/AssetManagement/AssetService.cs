using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.AssetManagement
{
    /// <summary>资产卡片服务（含班级绑定、调拨、报废、折旧）</summary>
    public class AstAssetService : BaseService<AstAssetEntity>, IDenpendency
    {
        public AstAssetService(ISqlSugarClient context) : base(context) { }

        public async Task<List<AstAssetEntity>> GetList(SoulPage<AstAssetEntity> pagination, string keyword, string categoryId, string status)
        {
            var queryable = repository.Db.Queryable<AstAssetEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword))
                queryable = queryable.Where(a => (a.F_AssetName ?? "").Contains(keyword) || (a.F_AssetCode ?? "").Contains(keyword) || (a.F_SerialNumber ?? "").Contains(keyword));
            if (!string.IsNullOrEmpty(categoryId)) queryable = queryable.Where(a => a.F_CategoryId == categoryId);
            if (!string.IsNullOrEmpty(status)) queryable = queryable.Where(a => a.F_Status == status);
            pagination.count = await queryable.CountAsync();
            return await queryable.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<AstAssetEntity> GetForm(string keyValue) => await repository.FindEntity(keyValue);

        public async Task SubmitForm(AstAssetEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                entity.F_DeleteMark = false;
                if (string.IsNullOrEmpty(entity.F_Status)) entity.F_Status = "在用";
                if (entity.F_NetValue == null) entity.F_NetValue = entity.F_PurchasePrice ?? 0;
                if (entity.F_DamageCount == null) entity.F_DamageCount = 0;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            await repository.Delete(a => ids.Contains(a.F_Id));
        }

        #region 班级资产绑定
        public async Task<List<AstClassRelationEntity>> GetClassRelationList(SoulPage<AstClassRelationEntity> pagination, string classId, string term)
        {
            var q = repository.Db.Queryable<AstClassRelationEntity, AstAssetEntity>((r, a) => new object[] { JoinType.Left, r.F_AssetId == a.F_Id })
                .Where((r, a) => r.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(classId)) q = q.Where((r, a) => r.F_ClassId == classId);
            if (!string.IsNullOrEmpty(term)) q = q.Where((r, a) => r.F_Term == term);
            pagination.count = await q.CountAsync();
            return await q.OrderBy((r, a) => r.F_CreatorTime, OrderByType.Desc)
                .Select((r, a) => r).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task BindClassAsset(AstClassRelationEntity entity)
        {
            entity.F_Id = Utils.GuId();
            entity.F_BindTime = DateTime.Now;
            entity.F_Status = "在用";
            entity.F_DeleteMark = false;
            entity.F_CreatorTime = DateTime.Now;
            entity.F_CreatorUserId = currentuser.UserId;
            await repository.Db.Insertable(entity).ExecuteCommandAsync();
            // 同步更新资产的班级归属
            await repository.Db.Updateable<AstAssetEntity>()
                .SetColumns(a => new AstAssetEntity { F_ClassId = entity.F_ClassId, F_GradeId = entity.F_GradeId, F_LastModifyTime = DateTime.Now, F_LastModifyUserId = currentuser.UserId })
                .Where(a => a.F_Id == entity.F_AssetId).ExecuteCommandAsync();
        }

        public async Task UnbindClassAsset(string relationId)
        {
            var r = await repository.Db.Queryable<AstClassRelationEntity>().Where(a => a.F_Id == relationId).FirstAsync();
            if (r != null)
            {
                r.F_Status = "已解绑";
                r.F_UnbindTime = DateTime.Now;
                r.F_LastModifyTime = DateTime.Now;
                r.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(r).ExecuteCommandAsync();
            }
        }
        #endregion

        #region 调拨
        public async Task<List<AstTransferEntity>> GetTransferList(SoulPage<AstTransferEntity> pagination, string keyword)
        {
            var q = repository.Db.Queryable<AstTransferEntity, AstAssetEntity>((t, a) => new object[] { JoinType.Left, t.F_AssetId == a.F_Id })
                .Where((t, a) => t.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where((t, a) => a.F_AssetName.Contains(keyword) || a.F_AssetCode.Contains(keyword));
            pagination.count = await q.CountAsync();
            return await q.OrderBy((t, a) => t.F_CreatorTime, OrderByType.Desc)
                .Select((t, a) => t).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task TransferAsset(AstTransferEntity entity)
        {
            entity.F_Id = Utils.GuId();
            entity.F_TransferDate = DateTime.Now;
            entity.F_OperatorId = currentuser.UserId;
            entity.F_Status = "已完成";
            entity.F_DeleteMark = false;
            entity.F_CreatorTime = DateTime.Now;
            entity.F_CreatorUserId = currentuser.UserId;
            await repository.Db.Insertable(entity).ExecuteCommandAsync();
            // 更新资产归属
            var upd = repository.Db.Updateable<AstAssetEntity>()
                .SetColumns(a => new AstAssetEntity { F_LastModifyTime = DateTime.Now, F_LastModifyUserId = currentuser.UserId });
            if (!string.IsNullOrEmpty(entity.F_ToClassId)) upd = upd.SetColumns(a => new AstAssetEntity { F_ClassId = entity.F_ToClassId });
            if (!string.IsNullOrEmpty(entity.F_ToRoomId)) upd = upd.SetColumns(a => new AstAssetEntity { F_RoomId = entity.F_ToRoomId });
            if (!string.IsNullOrEmpty(entity.F_ToUserId)) upd = upd.SetColumns(a => new AstAssetEntity { F_ResponsibleUserId = entity.F_ToUserId });
            await upd.Where(a => a.F_Id == entity.F_AssetId).ExecuteCommandAsync();
        }
        #endregion

        #region 报废
        public async Task<List<AstScrapEntity>> GetScrapList(SoulPage<AstScrapEntity> pagination, string status)
        {
            var q = repository.Db.Queryable<AstScrapEntity, AstAssetEntity>((s, a) => new object[] { JoinType.Left, s.F_AssetId == a.F_Id })
                .Where((s, a) => s.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(status)) q = q.Where((s, a) => s.F_Status == status);
            pagination.count = await q.CountAsync();
            return await q.OrderBy((s, a) => s.F_CreatorTime, OrderByType.Desc)
                .Select((s, a) => s).ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task ApplyScrap(AstScrapEntity entity)
        {
            var asset = await repository.Db.Queryable<AstAssetEntity>().Where(a => a.F_Id == entity.F_AssetId).FirstAsync();
            entity.F_Id = Utils.GuId();
            entity.F_ApplyTime = DateTime.Now;
            entity.F_ApplyUserId = currentuser.UserId;
            entity.F_Status = "待审批";
            entity.F_OriginalValue = asset?.F_PurchasePrice ?? 0;
            entity.F_NetValue = asset?.F_NetValue ?? 0;
            entity.F_DeleteMark = false;
            entity.F_CreatorTime = DateTime.Now;
            entity.F_CreatorUserId = currentuser.UserId;
            await repository.Db.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task ApproveScrap(string keyValue, bool approve, string remark)
        {
            var s = await repository.Db.Queryable<AstScrapEntity>().Where(a => a.F_Id == keyValue).FirstAsync();
            if (s == null) throw new Exception("报废申请不存在");
            s.F_Status = approve ? "已批准" : "已驳回";
            s.F_ApproverId = currentuser.UserId;
            s.F_ApproveTime = DateTime.Now;
            s.F_ApproveRemark = remark;
            s.F_LastModifyTime = DateTime.Now;
            s.F_LastModifyUserId = currentuser.UserId;
            await repository.Db.Updateable(s).ExecuteCommandAsync();
            if (approve)
            {
                await repository.Db.Updateable<AstAssetEntity>()
                    .SetColumns(a => new AstAssetEntity { F_Status = "报废", F_LastModifyTime = DateTime.Now, F_LastModifyUserId = currentuser.UserId })
                    .Where(a => a.F_Id == s.F_AssetId).ExecuteCommandAsync();
            }
        }
        #endregion

        #region 折旧
        /// <summary>按月计提折旧：直线法，月折旧 = 原值 / (年限*12)，净值 = 原值 - 累计</summary>
        public async Task<string> CalcDepreciation(string period)
        {
            var assets = await repository.Db.Queryable<AstAssetEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_Status != "报废" && a.F_DepreciationYears > 0 && a.F_PurchasePrice > 0)
                .ToListAsync();
            int count = 0;
            foreach (var a in assets)
            {
                // 跳过本月已计提
                var exists = await repository.Db.Queryable<AstDepreciationEntity>()
                    .Where(d => d.F_AssetId == a.F_Id && d.F_Period == period).AnyAsync();
                if (exists) continue;
                decimal price = a.F_PurchasePrice ?? 0m;
                int years = a.F_DepreciationYears ?? 0;
                decimal monthly = price / (years * 12);
                var acc = await repository.Db.Queryable<AstDepreciationEntity>()
                    .Where(d => d.F_AssetId == a.F_Id).SumAsync(d => d.F_MonthlyDepreciation);
                decimal accTotal = (acc ?? 0m) + monthly;
                decimal net = Math.Max(0m, price - accTotal);
                var dep = new AstDepreciationEntity
                {
                    F_Id = Utils.GuId(),
                    F_AssetId = a.F_Id,
                    F_Period = period,
                    F_MonthlyDepreciation = Math.Round(monthly, 2),
                    F_AccumulatedDepreciation = Math.Round(accTotal, 2),
                    F_NetValue = Math.Round(net, 2),
                    F_CreateTime = DateTime.Now
                };
                await repository.Db.Insertable(dep).ExecuteCommandAsync();
                await repository.Db.Updateable<AstAssetEntity>()
                    .SetColumns(x => new AstAssetEntity { F_NetValue = dep.F_NetValue, F_LastModifyTime = DateTime.Now })
                    .Where(x => x.F_Id == a.F_Id).ExecuteCommandAsync();
                count++;
            }
            return $"折旧计提完成，共处理 {count} 项资产。";
        }
        #endregion
    }
}
