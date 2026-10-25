using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 题库
    /// </summary>
    [SugarTable("school_question")]
    public class QuestionEntity : IEntity<QuestionEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "科目Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        /// <summary>
        /// 1=单选，2=多选，3=判断，4=简答
        /// </summary>
        [SugarColumn(ColumnName = "F_Type", ColumnDescription = "题型(1单选2多选3判断4简答)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Type { get; set; }

        [SugarColumn(ColumnName = "F_Content", ColumnDescription = "题目内容", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Content { get; set; }

        [SugarColumn(ColumnName = "F_OptionA", ColumnDescription = "选项A", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_OptionA { get; set; }

        [SugarColumn(ColumnName = "F_OptionB", ColumnDescription = "选项B", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_OptionB { get; set; }

        [SugarColumn(ColumnName = "F_OptionC", ColumnDescription = "选项C", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_OptionC { get; set; }

        [SugarColumn(ColumnName = "F_OptionD", ColumnDescription = "选项D", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_OptionD { get; set; }

        /// <summary>
        /// 单选存字母；多选存大写字母串（如 ABD）；判断存 A(正确)/B(错误)；简答存参考答案
        /// </summary>
        [SugarColumn(ColumnName = "F_Answer", ColumnDescription = "答案", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Answer { get; set; }

        [SugarColumn(ColumnName = "F_Analysis", ColumnDescription = "解析", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Analysis { get; set; }

        /// <summary>
        /// 1=易，2=中，3=难
        /// </summary>
        [SugarColumn(ColumnName = "F_Difficulty", ColumnDescription = "难度(1易2中3难)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Difficulty { get; set; }

        [SugarColumn(ColumnName = "F_Score", ColumnDescription = "默认分值", ColumnDataType = "decimal(5,1)", IsNullable = true)]
        public decimal? F_Score { get; set; }

        [SugarColumn(ColumnName = "F_EnabledMark", ColumnDescription = "有效标记", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }

        [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标记", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public bool? F_DeleteMark { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", ColumnDataType = "datetime2", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", ColumnDataType = "datetime2", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }

        [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间", ColumnDataType = "datetime2", IsNullable = true)]
        public DateTime? F_DeleteTime { get; set; }

        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_DeleteUserId { get; set; }
    }
}
