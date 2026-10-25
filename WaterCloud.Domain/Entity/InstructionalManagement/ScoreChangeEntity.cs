using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 成绩修改申请
    /// </summary>
    [SugarTable("school_score_change")]
    public class ScoreChangeEntity : IEntity<ScoreChangeEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        /// <summary>
        /// 成绩记录Id（school_scores.F_Id）
        /// </summary>
        [SugarColumn(ColumnName = "F_ScoreId", ColumnDescription = "成绩记录Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ScoreId { get; set; }

        /// <summary>
        /// 学生Id（快照）
        /// </summary>
        [SugarColumn(ColumnName = "F_StudentId", ColumnDescription = "学生Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_StudentId { get; set; }

        /// <summary>
        /// 课程Id（快照）
        /// </summary>
        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        /// <summary>
        /// 原分数（快照）
        /// </summary>
        [SugarColumn(ColumnName = "F_OldScore", ColumnDescription = "原分数", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_OldScore { get; set; }

        /// <summary>
        /// 新分数
        /// </summary>
        [SugarColumn(ColumnName = "F_NewScore", ColumnDescription = "新分数", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_NewScore { get; set; }

        /// <summary>
        /// 申请原因
        /// </summary>
        [SugarColumn(ColumnName = "F_Reason", ColumnDescription = "申请原因", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Reason { get; set; }

        /// <summary>
        /// 状态（0待审批 1已通过 2已拒绝）
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0待审批1已通过2已拒绝)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_AuditUserId", ColumnDescription = "审批人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_AuditUserId { get; set; }

        [SugarColumn(ColumnName = "F_AuditTime", ColumnDescription = "审批时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_AuditTime { get; set; }

        [SugarColumn(ColumnName = "F_AuditRemark", ColumnDescription = "审批意见", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_AuditRemark { get; set; }

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
