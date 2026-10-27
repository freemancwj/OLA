using System;
using SqlSugar;

namespace WaterCloud.Domain.AssetManagement
{
    /// <summary>器材借用归还</summary>
    [SugarTable("ast_borrow")]
    public class AstBorrowEntity : IEntity<AstBorrowEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        public string F_BorrowerId { get; set; }
        public string F_BorrowerName { get; set; }
        public string F_Purpose { get; set; }
        public DateTime? F_BorrowDate { get; set; }
        public DateTime? F_ExpectedReturnDate { get; set; }
        public DateTime? F_ActualReturnDate { get; set; }
        /// <summary>借出/已归还/逾期/损坏</summary>
        public string F_Status { get; set; }
        public string F_DamageNote { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>公物报修</summary>
    [SugarTable("ast_repair")]
    public class AstRepairEntity : IEntity<AstRepairEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        public string F_ReporterId { get; set; }
        public string F_ReporterName { get; set; }
        public DateTime? F_ReportTime { get; set; }
        public string F_FaultDescription { get; set; }
        /// <summary>待受理/维修中/已完成/已驳回</summary>
        public string F_Status { get; set; }
        public string F_Repairer { get; set; }
        public DateTime? F_RepairTime { get; set; }
        public decimal? F_RepairCost { get; set; }
        public string F_PartsReplaced { get; set; }
        public string F_RepairResult { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>设备维保</summary>
    [SugarTable("ast_maintenance")]
    public class AstMaintenanceEntity : IEntity<AstMaintenanceEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        /// <summary>定期/年检/故障</summary>
        public string F_MaintenanceType { get; set; }
        public DateTime? F_PlanDate { get; set; }
        public DateTime? F_ActualDate { get; set; }
        public string F_Operator { get; set; }
        public decimal? F_Cost { get; set; }
        public string F_Result { get; set; }
        public DateTime? F_NextDate { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>低值耗材库存</summary>
    [SugarTable("ast_consumable")]
    public class AstConsumableEntity : IEntity<AstConsumableEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_CategoryId { get; set; }
        public string F_Name { get; set; }
        public string F_Spec { get; set; }
        public string F_Unit { get; set; }
        public decimal? F_StockQty { get; set; }
        public decimal? F_StockWarnQty { get; set; }
        public decimal? F_UnitPrice { get; set; }
        public string F_SupplierId { get; set; }
        public string F_Remark { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>耗材出入库流水</summary>
    [SugarTable("ast_consumable_record")]
    public class AstConsumableRecordEntity : IEntity<AstConsumableRecordEntity>, ICreationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_ConsumableId { get; set; }
        /// <summary>入库/领用</summary>
        public string F_OperateType { get; set; }
        public decimal? F_Qty { get; set; }
        public string F_ClassId { get; set; }
        public string F_UserId { get; set; }
        public string F_OperatorId { get; set; }
        public DateTime? F_OperateTime { get; set; }
        public string F_Remark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
    }

    /// <summary>学期盘点主表</summary>
    [SugarTable("ast_inventory")]
    public class AstInventoryEntity : IEntity<AstInventoryEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_Term { get; set; }
        /// <summary>开学/期末</summary>
        public string F_InventoryType { get; set; }
        /// <summary>草稿/盘点中/已完成</summary>
        public string F_Status { get; set; }
        public DateTime? F_CreateTime { get; set; }
        public DateTime? F_FinishTime { get; set; }
        public string F_Remark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>盘点明细</summary>
    [SugarTable("ast_inventory_item")]
    public class AstInventoryItemEntity : IEntity<AstInventoryItemEntity>
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_InventoryId { get; set; }
        public string F_AssetId { get; set; }
        public string F_GradeId { get; set; }
        public string F_ClassId { get; set; }
        public string F_SystemStatus { get; set; }
        public string F_ActualStatus { get; set; }
        /// <summary>盘盈/盘亏/一致</summary>
        public string F_DiffResult { get; set; }
        public string F_HandlerId { get; set; }
        public DateTime? F_ConfirmTime { get; set; }
        public string F_Remark { get; set; }
    }
}
