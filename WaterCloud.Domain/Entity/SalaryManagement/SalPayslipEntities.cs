using System;
using SqlSugar;

namespace WaterCloud.Domain.SalaryManagement
{
    /// <summary>工资单（每月每人一条，带审批状态）</summary>
    [SugarTable("sal_payslip")]
    public class SalPayslipEntity : IEntity<SalPayslipEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_FixedTotal", IsNullable = true)] public decimal? F_FixedTotal { get; set; }
        [SugarColumn(ColumnName = "F_PeriodTotal", IsNullable = true)] public decimal? F_PeriodTotal { get; set; }
        [SugarColumn(ColumnName = "F_HeadTeacherTotal", IsNullable = true)] public decimal? F_HeadTeacherTotal { get; set; }
        [SugarColumn(ColumnName = "F_AfterSchoolTotal", IsNullable = true)] public decimal? F_AfterSchoolTotal { get; set; }
        [SugarColumn(ColumnName = "F_SpecialTotal", IsNullable = true)] public decimal? F_SpecialTotal { get; set; }
        [SugarColumn(ColumnName = "F_AdjustmentTotal", IsNullable = true)] public decimal? F_AdjustmentTotal { get; set; }
        [SugarColumn(ColumnName = "F_AttendanceDeduct", IsNullable = true)] public decimal? F_AttendanceDeduct { get; set; }
        [SugarColumn(ColumnName = "F_SocialPersonal", IsNullable = true)] public decimal? F_SocialPersonal { get; set; }
        [SugarColumn(ColumnName = "F_UnionFee", IsNullable = true)] public decimal? F_UnionFee { get; set; }
        [SugarColumn(ColumnName = "F_Tax", IsNullable = true)] public decimal? F_Tax { get; set; }
        [SugarColumn(ColumnName = "F_GrossPay", IsNullable = true)] public decimal? F_GrossPay { get; set; }
        [SugarColumn(ColumnName = "F_NetPay", IsNullable = true)] public decimal? F_NetPay { get; set; }
        /// <summary>0草稿 1人事提交 2教务已核 3财务已核 4校长已批(归档锁定) 5驳回</summary>
        [SugarColumn(ColumnName = "F_Status", IsNullable = true)] public int? F_Status { get; set; }
        [SugarColumn(ColumnName = "F_SubmitTime", IsNullable = true)] public DateTime? F_SubmitTime { get; set; }
        [SugarColumn(ColumnName = "F_SubmitUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_SubmitUserId { get; set; }
        [SugarColumn(ColumnName = "F_ApproveRemark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_ApproveRemark { get; set; }
        [SugarColumn(ColumnName = "F_LockTime", IsNullable = true)] public DateTime? F_LockTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalPayslipExtend : SalPayslipEntity { public string F_Account { get; set; } public string F_RealName { get; set; } public string F_OrganizeName { get; set; } }
    public class SalPayslipDetailExtend : SalPayslipDetailEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>工资单明细（每一笔钱的来源）</summary>
    [SugarTable("sal_payslip_detail")]
    public class SalPayslipDetailEntity : IEntity<SalPayslipDetailEntity>, ICreationAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_PayslipId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_PayslipId { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_Category", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_Category { get; set; }
        [SugarColumn(ColumnName = "F_ItemName", ColumnDataType = "nvarchar(200)", IsNullable = true)] public string F_ItemName { get; set; }
        [SugarColumn(ColumnName = "F_Quantity", IsNullable = true)] public decimal? F_Quantity { get; set; }
        [SugarColumn(ColumnName = "F_UnitPrice", IsNullable = true)] public decimal? F_UnitPrice { get; set; }
        [SugarColumn(ColumnName = "F_Amount", IsNullable = true)] public decimal? F_Amount { get; set; }
        [SugarColumn(ColumnName = "F_SortCode", IsNullable = true)] public int? F_SortCode { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
    }

    /// <summary>薪酬申诉</summary>
    [SugarTable("sal_appeal")]
    public class SalAppealEntity : IEntity<SalAppealEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_PayslipId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_PayslipId { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_Content", ColumnDataType = "nvarchar(1000)", IsNullable = true)] public string F_Content { get; set; }
        /// <summary>0待处理 1已处理</summary>
        [SugarColumn(ColumnName = "F_Status", IsNullable = true)] public int? F_Status { get; set; }
        [SugarColumn(ColumnName = "F_Reply", ColumnDataType = "nvarchar(1000)", IsNullable = true)] public string F_Reply { get; set; }
        [SugarColumn(ColumnName = "F_ReplyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_ReplyUserId { get; set; }
        [SugarColumn(ColumnName = "F_ReplyTime", IsNullable = true)] public DateTime? F_ReplyTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalAppealExtend : SalAppealEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>薪酬操作审计日志</summary>
    [SugarTable("sal_audit_log")]
    public class SalAuditLogEntity : IEntity<SalAuditLogEntity>, ICreationAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_Action", ColumnDataType = "nvarchar(100)", IsNullable = true)] public string F_Action { get; set; }
        [SugarColumn(ColumnName = "F_Detail", ColumnDataType = "nvarchar(2000)", IsNullable = true)] public string F_Detail { get; set; }
        [SugarColumn(ColumnName = "F_OperatorId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_OperatorId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
    }
    public class SalAuditLogExtend : SalAuditLogEntity { public string F_RealName { get; set; } public string F_OperatorName { get; set; } }
}
