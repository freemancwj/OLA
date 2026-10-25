using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 考试答题明细（每道题的作答与判分结果）
    /// </summary>
    [SugarTable("school_exam_answer")]
    public class ExamAnswerEntity : IEntity<ExamAnswerEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_ExamId", ColumnDescription = "考试Id", ColumnDataType = "nvarchar(50)")]
        public string F_ExamId { get; set; }

        [SugarColumn(ColumnName = "F_RecordId", ColumnDescription = "答题记录Id", ColumnDataType = "nvarchar(50)")]
        public string F_RecordId { get; set; }

        [SugarColumn(ColumnName = "F_QuestionId", ColumnDescription = "题目Id", ColumnDataType = "nvarchar(50)")]
        public string F_QuestionId { get; set; }

        [SugarColumn(ColumnName = "F_QuestionType", ColumnDescription = "题型(1单选2多选3判断4简答)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_QuestionType { get; set; }

        [SugarColumn(ColumnName = "F_Answer", ColumnDescription = "学生答案", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Answer { get; set; }

        /// <summary>
        /// 1=正确，0=错误，2=待批改
        /// </summary>
        [SugarColumn(ColumnName = "F_IsCorrect", ColumnDescription = "是否正确(1对0错2待批改)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_IsCorrect { get; set; }

        [SugarColumn(ColumnName = "F_Score", ColumnDescription = "本题得分", ColumnDataType = "decimal(5,1)", IsNullable = true)]
        public decimal? F_Score { get; set; }

        [SugarColumn(ColumnName = "F_FullScore", ColumnDescription = "本题满分", ColumnDataType = "decimal(5,1)", IsNullable = true)]
        public decimal? F_FullScore { get; set; }

        [SugarColumn(ColumnName = "F_Comment", ColumnDescription = "批改评语", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Comment { get; set; }

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
