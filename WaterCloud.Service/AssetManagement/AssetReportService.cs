using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;

namespace WaterCloud.Service.AssetManagement
{
    /// <summary>资产统计报表服务</summary>
    public class AstReportService : BaseService<AstAssetEntity>, IDenpendency
    {
        public AstReportService(ISqlSugarClient context) : base(context) { }

        /// <summary>全校资产总台账</summary>
        public async Task<List<AstAssetEntity>> GetAssetLedger(string keyword, string categoryId, string status)
        {
            var q = repository.Db.Queryable<AstAssetEntity>().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where(a => a.F_AssetName.Contains(keyword) || a.F_AssetCode.Contains(keyword));
            if (!string.IsNullOrEmpty(categoryId)) q = q.Where(a => a.F_CategoryId == categoryId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(a => a.F_Status == status);
            return await q.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToListAsync();
        }

        /// <summary>按分类汇总数量与原值</summary>
        public async Task<object> GetCategorySummary()
        {
            var list = await repository.Db.Queryable<AstAssetEntity, AstCategoryEntity>((a, c) => new object[] { JoinType.Left, a.F_CategoryId == c.F_Id })
                .Where((a, c) => a.F_DeleteMark == false)
                .GroupBy((a, c) => new { c.F_CategoryName })
                .Select((a, c) => new
                {
                    CategoryName = c.F_CategoryName,
                    Count = SqlFunc.AggregateCount(a.F_Id),
                    TotalValue = SqlFunc.AggregateSum(a.F_PurchasePrice),
                    NetValue = SqlFunc.AggregateSum(a.F_NetValue)
                }).ToListAsync();
            return list;
        }

        /// <summary>各班公物损坏统计</summary>
        public async Task<object> GetClassDamageSummary()
        {
            var list = await repository.Db.Queryable<AstAssetEntity, AstClassEntity>((a, c) => new object[] { JoinType.Left, a.F_ClassId == c.F_Id })
                .Where((a, c) => a.F_DeleteMark == false && a.F_DamageCount > 0)
                .GroupBy((a, c) => new { c.F_ClassName })
                .Select((a, c) => new
                {
                    ClassName = c.F_ClassName,
                    DamageCount = SqlFunc.AggregateSum(a.F_DamageCount),
                    AssetCount = SqlFunc.AggregateCount(a.F_Id)
                }).ToListAsync();
            return list;
        }

        /// <summary>闲置资产统计</summary>
        public async Task<List<AstAssetEntity>> GetIdleAssets()
            => await repository.Db.Queryable<AstAssetEntity>().Where(a => a.F_DeleteMark == false && a.F_Status == "闲置").ToListAsync();

        /// <summary>折旧台账</summary>
        public async Task<List<AstDepreciationEntity>> GetDepreciationLedger(string period)
        {
            var q = repository.Db.Queryable<AstDepreciationEntity>();
            if (!string.IsNullOrEmpty(period)) q = q.Where(a => a.F_Period == period);
            return await q.OrderBy(a => a.F_Period, OrderByType.Desc).ToListAsync();
        }
    }
}
