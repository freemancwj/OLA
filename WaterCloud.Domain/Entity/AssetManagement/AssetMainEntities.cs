using System;
using SqlSugar;

namespace WaterCloud.Domain.AssetManagement
{
    /// <summary>资产卡片主表（一物一码）</summary>
    [SugarTable("ast_asset")]
    public class AstAssetEntity : IEntity<AstAssetEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        /// <summary>资产编号/二维码</summary>
        public string F_AssetCode { get; set; }
        public string F_AssetName { get; set; }
        public string F_CategoryId { get; set; }
        public string F_SupplierId { get; set; }
        public string F_SpecModel { get; set; }
        public string F_SerialNumber { get; set; }
        public DateTime? F_PurchaseDate { get; set; }
        public decimal? F_PurchasePrice { get; set; }
        /// <summary>财政拨付/自购/捐赠</summary>
        public string F_PurchaseSource { get; set; }
        public int? F_DepreciationYears { get; set; }
        /// <summary>在用/闲置/维修中/借用中/报废</summary>
        public string F_Status { get; set; }
        public bool? F_IsTeachingDevice { get; set; }
        public string F_GradeId { get; set; }
        public string F_ClassId { get; set; }
        public string F_RoomId { get; set; }
        /// <summary>保管人/责任人</summary>
        public string F_ResponsibleUserId { get; set; }
        public string F_Location { get; set; }
        public int? F_DamageCount { get; set; }
        public decimal? F_NetValue { get; set; }
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

    /// <summary>班级资产绑定</summary>
    [SugarTable("ast_class_relation")]
    public class AstClassRelationEntity : IEntity<AstClassRelationEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        public string F_GradeId { get; set; }
        public string F_ClassId { get; set; }
        public string F_Term { get; set; }
        public DateTime? F_BindTime { get; set; }
        public DateTime? F_UnbindTime { get; set; }
        /// <summary>在用/已解绑</summary>
        public string F_Status { get; set; }
        public string F_Remark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>校园资产调拨</summary>
    [SugarTable("ast_transfer")]
    public class AstTransferEntity : IEntity<AstTransferEntity>, ICreationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        public string F_FromClassId { get; set; }
        public string F_ToClassId { get; set; }
        public string F_FromRoomId { get; set; }
        public string F_ToRoomId { get; set; }
        public string F_FromUserId { get; set; }
        public string F_ToUserId { get; set; }
        public DateTime? F_TransferDate { get; set; }
        public string F_OperatorId { get; set; }
        public string F_Reason { get; set; }
        public string F_Status { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
    }

    /// <summary>报废处置</summary>
    [SugarTable("ast_scrap")]
    public class AstScrapEntity : IEntity<AstScrapEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        public string F_ApplyUserId { get; set; }
        public DateTime? F_ApplyTime { get; set; }
        public string F_Reason { get; set; }
        /// <summary>待审批/已批准/已驳回/已处置</summary>
        public string F_Status { get; set; }
        public string F_ApproverId { get; set; }
        public DateTime? F_ApproveTime { get; set; }
        public string F_ApproveRemark { get; set; }
        public decimal? F_OriginalValue { get; set; }
        public decimal? F_NetValue { get; set; }
        public string F_DisposalMethod { get; set; }
        public DateTime? F_DisposalDate { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
    }

    /// <summary>折旧记录</summary>
    [SugarTable("ast_depreciation")]
    public class AstDepreciationEntity : IEntity<AstDepreciationEntity>
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_AssetId { get; set; }
        /// <summary>yyyy-MM</summary>
        public string F_Period { get; set; }
        public decimal? F_MonthlyDepreciation { get; set; }
        public decimal? F_AccumulatedDepreciation { get; set; }
        public decimal? F_NetValue { get; set; }
        public DateTime? F_CreateTime { get; set; }
    }
}
