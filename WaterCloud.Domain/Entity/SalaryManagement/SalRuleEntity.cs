using System;
using SqlSugar;

namespace WaterCloud.Domain.SalaryManagement
{
    /// <summary>
    /// 薪酬规则参数（单行配置，所有单价可调，不需要改代码）
    /// </summary>
    [SugarTable("sal_rule")]
    public class SalRuleEntity : IEntity<SalRuleEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        // 课时费职称单价（元/节）
        [SugarColumn(ColumnName = "F_RateTitle1", IsNullable = true)] public decimal? F_RateTitle1 { get; set; }
        [SugarColumn(ColumnName = "F_RateTitle2", IsNullable = true)] public decimal? F_RateTitle2 { get; set; }
        [SugarColumn(ColumnName = "F_RateTitle3", IsNullable = true)] public decimal? F_RateTitle3 { get; set; }
        [SugarColumn(ColumnName = "F_RateTitle4", IsNullable = true)] public decimal? F_RateTitle4 { get; set; }
        [SugarColumn(ColumnName = "F_StandardWeeklyPeriods", IsNullable = true)] public int? F_StandardWeeklyPeriods { get; set; }
        [SugarColumn(ColumnName = "F_OvertimeRate", IsNullable = true)] public decimal? F_OvertimeRate { get; set; }
        [SugarColumn(ColumnName = "F_MorningReadRate", IsNullable = true)] public decimal? F_MorningReadRate { get; set; }
        [SugarColumn(ColumnName = "F_EveningStudyRate", IsNullable = true)] public decimal? F_EveningStudyRate { get; set; }
        [SugarColumn(ColumnName = "F_SubstituteRate", IsNullable = true)] public decimal? F_SubstituteRate { get; set; }
        // 班主任津贴分档
        [SugarColumn(ColumnName = "F_ClassTier1Max", IsNullable = true)] public int? F_ClassTier1Max { get; set; }
        [SugarColumn(ColumnName = "F_ClassTier1Amount", IsNullable = true)] public decimal? F_ClassTier1Amount { get; set; }
        [SugarColumn(ColumnName = "F_ClassTier2Max", IsNullable = true)] public int? F_ClassTier2Max { get; set; }
        [SugarColumn(ColumnName = "F_ClassTier2Amount", IsNullable = true)] public decimal? F_ClassTier2Amount { get; set; }
        [SugarColumn(ColumnName = "F_ClassTier3Amount", IsNullable = true)] public decimal? F_ClassTier3Amount { get; set; }
        [SugarColumn(ColumnName = "F_GraduationBonusRate", IsNullable = true)] public decimal? F_GraduationBonusRate { get; set; }
        [SugarColumn(ColumnName = "F_DeputyRate", IsNullable = true)] public decimal? F_DeputyRate { get; set; }
        // 班主任考核发放比例
        [SugarColumn(ColumnName = "F_AssessExcellent", IsNullable = true)] public decimal? F_AssessExcellent { get; set; }
        [SugarColumn(ColumnName = "F_AssessGood", IsNullable = true)] public decimal? F_AssessGood { get; set; }
        [SugarColumn(ColumnName = "F_AssessPoor", IsNullable = true)] public decimal? F_AssessPoor { get; set; }
        // 课后服务费单价
        [SugarColumn(ColumnName = "F_AfterSchoolHomework", IsNullable = true)] public decimal? F_AfterSchoolHomework { get; set; }
        [SugarColumn(ColumnName = "F_AfterSchoolClub", IsNullable = true)] public decimal? F_AfterSchoolClub { get; set; }
        [SugarColumn(ColumnName = "F_AfterSchoolCare", IsNullable = true)] public decimal? F_AfterSchoolCare { get; set; }
        // 其他专项津贴
        [SugarColumn(ColumnName = "F_GroupLeaderAllowance", IsNullable = true)] public decimal? F_GroupLeaderAllowance { get; set; }
        [SugarColumn(ColumnName = "F_LessonLeaderAllowance", IsNullable = true)] public decimal? F_LessonLeaderAllowance { get; set; }
        [SugarColumn(ColumnName = "F_InvigilateRate", IsNullable = true)] public decimal? F_InvigilateRate { get; set; }
        [SugarColumn(ColumnName = "F_DutyRate", IsNullable = true)] public decimal? F_DutyRate { get; set; }
        // 考勤扣款
        [SugarColumn(ColumnName = "F_LateDeduct", IsNullable = true)] public decimal? F_LateDeduct { get; set; }
        [SugarColumn(ColumnName = "F_SickLeaveDeduct", IsNullable = true)] public decimal? F_SickLeaveDeduct { get; set; }
        [SugarColumn(ColumnName = "F_PersonalLeaveDeduct", IsNullable = true)] public decimal? F_PersonalLeaveDeduct { get; set; }
        [SugarColumn(ColumnName = "F_AbsentDeduct", IsNullable = true)] public decimal? F_AbsentDeduct { get; set; }
        // 社保公积金/工会比例
        [SugarColumn(ColumnName = "F_SocialRate", IsNullable = true)] public decimal? F_SocialRate { get; set; }
        [SugarColumn(ColumnName = "F_HousingFundRate", IsNullable = true)] public decimal? F_HousingFundRate { get; set; }
        [SugarColumn(ColumnName = "F_UnionFeeRate", IsNullable = true)] public decimal? F_UnionFeeRate { get; set; }

        [SugarColumn(ColumnName = "F_EnabledMark", IsNullable = true)] public bool? F_EnabledMark { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", IsNullable = true)] public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", IsNullable = true)] public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", IsNullable = true)] public string F_LastModifyUserId { get; set; }
    }
}
