using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;


namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 创 建：超级管理员
    /// 日 期：2025-05-28 20:29
    /// 描 述：Entity实体类
    /// </summary>
    [SugarTable("school_scores")]
    public class ScoresEntity : IEntity<ScoresEntity>,ICreationAudited,IModificationAudited,IDeleteAudited
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        /// <summary>
        /// 学生ID
        /// </summary>
        [SugarColumn(ColumnName = "F_StudentId", ColumnDescription = "学生ID", ColumnDataType = "nvarchar(50)")]
        public string F_StudentId { get; set; }
        /// <summary>
        /// 班级ID
        /// </summary>
        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级ID", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }
        /// <summary>
        /// 课程ID
        /// </summary>
        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程ID", ColumnDataType = "nvarchar(50)")]
        public string F_CourseId { get; set; }
        /// <summary>
        /// 分数
        /// </summary>
        [SugarColumn(ColumnName = "F_Score", ColumnDescription = "分数", ColumnDataType = "decimal(6,2)", IsNullable = true)]
        public decimal? F_Score { get; set; }
        /// <summary>
        /// 考试时间
        /// </summary>
        [SugarColumn(ColumnName = "F_ExamTime", ColumnDescription = "考试时间", ColumnDataType = "datetime(23)", IsNullable = true)]
        public DateTime? F_ExamTime { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        [SugarColumn(ColumnName = "F_Semester", ColumnDescription = "学期", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_Semester { get; set; }
        /// <summary>
        /// 成绩类型（期中考试、期末考试、平时成绩等）
        /// </summary>
        [SugarColumn(ColumnName = "F_ScoreType", ColumnDescription = "成绩类型", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ScoreType { get; set; }
        /// <summary>
        /// 成绩等级
        /// </summary>
        [SugarColumn(ColumnName = "F_ScoreLevel", ColumnDescription = "成绩等级", ColumnDataType = "nvarchar(20)", IsNullable = true)]
        public string F_ScoreLevel { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "F_Remak", ColumnDescription = "备注", ColumnDataType = "nvarchar(150)", IsNullable = true)]
        public string F_Remak { get; set; }
        /// <summary>
        /// 审核状态（0待审核 1已通过 2已退回）
        /// </summary>
        [SugarColumn(ColumnName = "F_AuditStatus", ColumnDescription = "审核状态", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_AuditStatus { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        [SugarColumn(ColumnName = "F_AuditUserId", ColumnDescription = "审核人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_AuditUserId { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        [SugarColumn(ColumnName = "F_AuditTime", ColumnDescription = "审核时间", ColumnDataType = "datetime(23)", IsNullable = true)]
        public DateTime? F_AuditTime { get; set; }
        /// <summary>
        /// 审核意见
        /// </summary>
        [SugarColumn(ColumnName = "F_AuditRemark", ColumnDescription = "审核意见", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_AuditRemark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_EnabledMark", ColumnDescription = "",ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_DeleteMark", ColumnDescription = "",ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_DeleteMark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_CreatorTime", ColumnDescription = "",ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_CreatorUserId", ColumnDescription = "",ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_LastModifyTime", ColumnDescription = "",ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_LastModifyUserId", ColumnDescription = "",ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_DeleteTime", ColumnDescription = "",ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_DeleteTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(ColumnName="F_DeleteUserId", ColumnDescription = "",ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_DeleteUserId { get; set; }
    
    }
}
