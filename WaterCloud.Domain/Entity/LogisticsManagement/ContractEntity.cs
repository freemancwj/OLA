using System;
using SqlSugar;

namespace WaterCloud.Domain.LogisticsManagement
{
    /// <summary>
    /// 合同管理
    /// </summary>
    [SugarTable("lg_contract")]
    public class ContractEntity : IEntity<ContractEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_ContractNo", ColumnDescription = "合同编号", ColumnDataType = "nvarchar(50)")]
        public string F_ContractNo { get; set; }

        [SugarColumn(ColumnName = "F_Name", ColumnDescription = "合同名称", ColumnDataType = "nvarchar(200)")]
        public string F_Name { get; set; }

        [SugarColumn(ColumnName = "F_PartyA", ColumnDescription = "甲方", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_PartyA { get; set; }

        [SugarColumn(ColumnName = "F_PartyB", ColumnDescription = "乙方", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_PartyB { get; set; }

        [SugarColumn(ColumnName = "F_Amount", ColumnDescription = "合同金额", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_Amount { get; set; }

        [SugarColumn(ColumnName = "F_SignDate", ColumnDescription = "签订日期", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_SignDate { get; set; }

        [SugarColumn(ColumnName = "F_ExpireDate", ColumnDescription = "到期日期", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_ExpireDate { get; set; }

        /// <summary>
        /// 0=履行中，1=已到期，2=已终止
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0履行中1已到期2已终止)", ColumnDataType = "int", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_FileUrl", ColumnDescription = "附件地址", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_FileUrl { get; set; }

        [SugarColumn(ColumnName = "F_FileName", ColumnDescription = "附件名称", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_FileName { get; set; }

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
