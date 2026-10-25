using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 考试答题记录（一名学生对应一场考试仅一条）
    /// </summary>
    [SugarTable("school_exam_record")]
    public class ExamRecordEntity : IEntity<ExamRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_ExamId", ColumnDescription = "考试Id", ColumnDataType = "nvarchar(50)")]
        public string F_ExamId { get; set; }

        [SugarColumn(ColumnName = "F_StudentId", ColumnDescription = "学生Id", ColumnDataType = "nvarchar(50)")]
        public string F_StudentId { get; set; }

        [SugarColumn(ColumnName = "F_StartTime", ColumnDescription = "开始答题时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_StartTime { get; set; }

        [SugarColumn(ColumnName = "F_SubmitTime", ColumnDescription = "提交时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_SubmitTime { get; set; }

        [SugarColumn(ColumnName = "F_ObjectiveScore", ColumnDescription = "客观分", ColumnDataType = "decimal(6,1)", IsNullable = true)]
        public decimal? F_ObjectiveScore { get; set; }

        [SugarColumn(ColumnName = "F_SubjectiveScore", ColumnDescription = "主观分", ColumnDataType = "decimal(6,1)", IsNullable = true)]
        public decimal? F_SubjectiveScore { get; set; }

        [SugarColumn(ColumnName = "F_TotalScore", ColumnDescription = "总分", ColumnDataType = "decimal(6,1)", IsNullable = true)]
        public decimal? F_TotalScore { get; set; }

        /// <summary>
        /// 0=待批改，1=已完成
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0待批改1已完成)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_IsPassed", ColumnDescription = "是否及格(1是0否)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_IsPassed { get; set; }

        [SugarColumn(ColumnName = "F_GraderId", ColumnDescription = "批改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_GraderId { get; set; }

        [SugarColumn(ColumnName = "F_GraderName", ColumnDescription = "批改人姓名", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_GraderName { get; set; }

        [SugarColumn(ColumnName = "F_GradeTime", ColumnDescription = "批改时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_GradeTime { get; set; }

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
