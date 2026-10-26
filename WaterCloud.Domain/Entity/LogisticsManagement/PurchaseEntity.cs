using System;
using SqlSugar;

namespace WaterCloud.Domain.LogisticsManagement
{
    /// <summary>
    /// 采购申请
    /// </summary>
    [SugarTable("lg_purchase")]
    public class PurchaseEntity : IEntity<PurchaseEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_ItemName", ColumnDescription = "物品名称", ColumnDataType = "nvarchar(200)")]
        public string F_ItemName { get; set; }

        [SugarColumn(ColumnName = "F_Spec", ColumnDescription = "规格型号", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_Spec { get; set; }

        [SugarColumn(ColumnName = "F_Quantity", ColumnDescription = "数量", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_Quantity { get; set; }

        [SugarColumn(ColumnName = "F_Unit", ColumnDescription = "单位", ColumnDataType = "nvarchar(20)", IsNullable = true)]
        public string F_Unit { get; set; }

        [SugarColumn(ColumnName = "F_BudgetAmount", ColumnDescription = "预算金额", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_BudgetAmount { get; set; }

        [SugarColumn(ColumnName = "F_Supplier", ColumnDescription = "供应商", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_Supplier { get; set; }

        /// <summary>
        /// 0=申请中，1=已批准，2=已采购，3=已驳回
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0申请中1已批准2已采购3已驳回)", ColumnDataType = "int", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_ApplyDate", ColumnDescription = "申请日期", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_ApplyDate { get; set; }

        [SugarColumn(ColumnName = "F_ApplicantId", ColumnDescription = "申请人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ApplicantId { get; set; }

        [SugarColumn(ColumnName = "F_ApplicantName", ColumnDescription = "申请人姓名", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ApplicantName { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

        [SugarColumn(ColumnName = "F_EnabledMark", ColumnDescription = "有效标记", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }

        [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标记", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_DeleteMark { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }

        [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_DeleteTime { get; set; }

        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_DeleteUserId { get; set; }
    }
}
