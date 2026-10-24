using SqlSugar;
using System.Collections.Generic;

namespace WaterCloud.Domain.InstructionalManagement
{
    [SugarTable("sys_user")]
    public class ScoresExtend : ScoresEntity
    {
        //使用导入错误信息
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public string F_RealName { get; set; }
        /// <summary>
        /// 学号（账号）
        /// </summary>
        public string F_Account { get; set; }
        /// <summary>
        /// 课程名称
        /// </summary>
        public string F_CourseName { get; set; }
        /// <summary>
        /// 成绩类型名称
        /// </summary>
        public string F_ScoreTypeName { get; set; }
        /// <summary>
        /// 学生班级
        /// </summary>
        public string F_ClassName { get; set; }
        /// <summary>
        /// 录入人
        /// </summary>
        public string F_CreatorUserName { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string F_AuditUserName { get; set; }
    }

    /// <summary>
    /// 当前登录用户在成绩管理中的角色信息
    /// </summary>
    public class ScoresRoleInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 是否管理员（可审核全部成绩）
        /// </summary>
        public bool IsAdmin { get; set; }
        /// <summary>
        /// 是否学生（只能查看自己已审核通过的成绩）
        /// </summary>
        public bool IsStudent { get; set; }
        /// <summary>
        /// 是否可录入/修改成绩
        /// </summary>
        public bool CanEntry { get; set; }
        /// <summary>
        /// 是否可审核成绩
        /// </summary>
        public bool CanAudit { get; set; }
    }

    /// <summary>
    /// 批量录入时返回的学生行
    /// </summary>
    public class ScoresBatchStudent
    {
        public string F_StudentId { get; set; }
        public string F_Account { get; set; }
        public string F_RealName { get; set; }
        public string F_ClassName { get; set; }
        /// <summary>
        /// 已录入的成绩，未录入为 null
        /// </summary>
        public decimal? F_Score { get; set; }
        public string F_Remak { get; set; }
        public string F_ScoreId { get; set; }
        public int? F_AuditStatus { get; set; }
    }

    /// <summary>
    /// 批量录入提交的成绩行
    /// </summary>
    public class ScoresBatchRow
    {
        public string F_StudentId { get; set; }
        public decimal? F_Score { get; set; }
        public string F_Remak { get; set; }
    }

    /// <summary>
    /// 成绩统计概览
    /// </summary>
    public class ScoresStatSummary
    {
        public int Total { get; set; }
        public int ScoredCount { get; set; }
        public decimal AvgScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal MinScore { get; set; }
        public int PassCount { get; set; }
        public int ExcellentCount { get; set; }
        /// <summary>
        /// 及格率（百分比）
        /// </summary>
        public decimal PassRate { get; set; }
        /// <summary>
        /// 优秀率（百分比）
        /// </summary>
        public decimal ExcellentRate { get; set; }
    }

    /// <summary>
    /// 成绩分组统计项（按课程、班级、学期分组）
    /// </summary>
    public class ScoresStatItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal AvgScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal MinScore { get; set; }
        public decimal PassRate { get; set; }
    }

    /// <summary>
    /// 成绩分布统计项
    /// </summary>
    public class ScoresStatDistribution
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// 成绩导出数据行
    /// </summary>
    public class ScoresExportRow
    {
        public string 学号 { get; set; }
        public string 姓名 { get; set; }
        public string 班级 { get; set; }
        public string 课程 { get; set; }
        public string 成绩类型 { get; set; }
        public string 分数 { get; set; }
        public string 等级 { get; set; }
        public string 学期 { get; set; }
        public string 考试时间 { get; set; }
        public string 审核状态 { get; set; }
        public string 录入人 { get; set; }
        public string 备注 { get; set; }
    }

    /// <summary>
    /// 成绩统计结果
    /// </summary>
    public class ScoresStatisticsResult
    {
        public ScoresStatSummary Summary { get; set; }
        public List<ScoresStatItem> CourseStats { get; set; }
        public List<ScoresStatItem> ClassStats { get; set; }
        public List<ScoresStatDistribution> ScoreDistribution { get; set; }
        public List<ScoresStatDistribution> LevelDistribution { get; set; }
        public List<ScoresStatItem> SemesterStats { get; set; }
    }
}
