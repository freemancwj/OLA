using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 在线考试
    /// </summary>
    [SugarTable("school_exam")]
    public class ExamEntity : IEntity<ExamEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_Title", ColumnDescription = "考试标题", ColumnDataType = "nvarchar(200)")]
        public string F_Title { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "科目Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

        /// <summary>
        /// 1=章节测试，2=模拟考试
        /// </summary>
        [SugarColumn(ColumnName = "F_Type", ColumnDescription = "类型(1章节测试2模拟考试)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Type { get; set; }

        [SugarColumn(ColumnName = "F_StartTime", ColumnDescription = "开始时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_StartTime { get; set; }

        [SugarColumn(ColumnName = "F_EndTime", ColumnDescription = "结束时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_EndTime { get; set; }

        [SugarColumn(ColumnName = "F_Duration", ColumnDescription = "考试时长(分钟)", IsNullable = true)]
        public int? F_Duration { get; set; }

        [SugarColumn(ColumnName = "F_TotalScore", ColumnDescription = "试卷总分", ColumnDataType = "decimal(6,1)", IsNullable = true)]
        public decimal? F_TotalScore { get; set; }

        [SugarColumn(ColumnName = "F_PassScore", ColumnDescription = "及格分", ColumnDataType = "decimal(6,1)", IsNullable = true)]
        public decimal? F_PassScore { get; set; }

        /// <summary>
        /// 0=草稿，1=已发布，2=已结束
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0草稿1已发布2已结束)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

        [SugarColumn(ColumnName = "F_PublisherId", ColumnDescription = "发布人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_PublisherId { get; set; }

        [SugarColumn(ColumnName = "F_PublisherName", ColumnDescription = "发布人姓名", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_PublisherName { get; set; }

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
