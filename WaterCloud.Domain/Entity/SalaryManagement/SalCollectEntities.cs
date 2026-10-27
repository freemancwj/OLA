using System;
using SqlSugar;

namespace WaterCloud.Domain.SalaryManagement
{
    /// <summary>课时记录（教务按月录入）</summary>
    [SugarTable("sal_period_record")]
    public class SalPeriodRecordEntity : IEntity<SalPeriodRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_NormalPeriods", IsNullable = true)] public decimal? F_NormalPeriods { get; set; }
        [SugarColumn(ColumnName = "F_OvertimePeriods", IsNullable = true)] public decimal? F_OvertimePeriods { get; set; }
        [SugarColumn(ColumnName = "F_SubstitutePeriods", IsNullable = true)] public decimal? F_SubstitutePeriods { get; set; }
        [SugarColumn(ColumnName = "F_MorningReadCount", IsNullable = true)] public int? F_MorningReadCount { get; set; }
        [SugarColumn(ColumnName = "F_EveningStudyCount", IsNullable = true)] public int? F_EveningStudyCount { get; set; }
        [SugarColumn(ColumnName = "F_Remark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Remark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalPeriodRecordExtend : SalPeriodRecordEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>班主任考核记录（德育处按月录入）</summary>
    [SugarTable("sal_headteacher_assess")]
    public class SalHeadTeacherAssessEntity : IEntity<SalHeadTeacherAssessEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_ClassId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_ClassId { get; set; }
        [SugarColumn(ColumnName = "F_ClassSize", IsNullable = true)] public int? F_ClassSize { get; set; }
        [SugarColumn(ColumnName = "F_IsGraduation", IsNullable = true)] public bool? F_IsGraduation { get; set; }
        /// <summary>考核 1优秀 2合格 3不合格</summary>
        [SugarColumn(ColumnName = "F_AssessLevel", IsNullable = true)] public int? F_AssessLevel { get; set; }
        [SugarColumn(ColumnName = "F_Score", IsNullable = true)] public decimal? F_Score { get; set; }
        [SugarColumn(ColumnName = "F_Remark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Remark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalHeadTeacherAssessExtend : SalHeadTeacherAssessEntity { public string F_Account { get; set; } public string F_RealName { get; set; } public string F_ClassName { get; set; } }

    /// <summary>课后服务记录</summary>
    [SugarTable("sal_afterschool_record")]
    public class SalAfterSchoolRecordEntity : IEntity<SalAfterSchoolRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        /// <summary>1作业辅导 2社团指导 3延时托管</summary>
        [SugarColumn(ColumnName = "F_ServiceType", IsNullable = true)] public int? F_ServiceType { get; set; }
        [SugarColumn(ColumnName = "F_Periods", IsNullable = true)] public decimal? F_Periods { get; set; }
        [SugarColumn(ColumnName = "F_Remark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Remark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalAfterSchoolRecordExtend : SalAfterSchoolRecordEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>监考记录</summary>
    [SugarTable("sal_invigilate_record")]
    public class SalInvigilateRecordEntity : IEntity<SalInvigilateRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_ExamName", ColumnDataType = "nvarchar(100)", IsNullable = true)] public string F_ExamName { get; set; }
        [SugarColumn(ColumnName = "F_Sessions", IsNullable = true)] public int? F_Sessions { get; set; }
        [SugarColumn(ColumnName = "F_Remark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Remark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalInvigilateRecordExtend : SalInvigilateRecordEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>考勤汇总（按月）</summary>
    [SugarTable("sal_attendance_summary")]
    public class SalAttendanceSummaryEntity : IEntity<SalAttendanceSummaryEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_SickLeaveDays", IsNullable = true)] public decimal? F_SickLeaveDays { get; set; }
        [SugarColumn(ColumnName = "F_PersonalLeaveDays", IsNullable = true)] public decimal? F_PersonalLeaveDays { get; set; }
        [SugarColumn(ColumnName = "F_AbsentDays", IsNullable = true)] public decimal? F_AbsentDays { get; set; }
        [SugarColumn(ColumnName = "F_LateCount", IsNullable = true)] public int? F_LateCount { get; set; }
        [SugarColumn(ColumnName = "F_EarlyCount", IsNullable = true)] public int? F_EarlyCount { get; set; }
        [SugarColumn(ColumnName = "F_Remark", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Remark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalAttendanceSummaryExtend : SalAttendanceSummaryEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }

    /// <summary>薪酬手动调整项</summary>
    [SugarTable("sal_adjustment")]
    public class SalAdjustmentEntity : IEntity<SalAdjustmentEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)] public string F_Id { get; set; }
        [SugarColumn(ColumnName = "F_Month", ColumnDataType = "nvarchar(7)", IsNullable = true)] public string F_Month { get; set; }
        [SugarColumn(ColumnName = "F_UserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_UserId { get; set; }
        [SugarColumn(ColumnName = "F_ItemName", ColumnDataType = "nvarchar(100)", IsNullable = true)] public string F_ItemName { get; set; }
        [SugarColumn(ColumnName = "F_Amount", IsNullable = true)] public decimal? F_Amount { get; set; }
        [SugarColumn(ColumnName = "F_Reason", ColumnDataType = "nvarchar(500)", IsNullable = true)] public string F_Reason { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", IsNullable = true)] public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)] public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_DeleteUserId { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
    public class SalAdjustmentExtend : SalAdjustmentEntity { public string F_Account { get; set; } public string F_RealName { get; set; } }
}
