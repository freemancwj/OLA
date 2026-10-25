using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 作业
    /// </summary>
    [SugarTable("school_homework")]
    public class HomeworkEntity : IEntity<HomeworkEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_Title", ColumnDescription = "作业标题", ColumnDataType = "nvarchar(200)")]
        public string F_Title { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "科目Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

        [SugarColumn(ColumnName = "F_Content", ColumnDescription = "作业要求", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Content { get; set; }

        [SugarColumn(ColumnName = "F_FileUrl", ColumnDescription = "附件地址", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_FileUrl { get; set; }

        [SugarColumn(ColumnName = "F_FileName", ColumnDescription = "附件名称", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_FileName { get; set; }

        [SugarColumn(ColumnName = "F_FileSize", ColumnDescription = "附件大小", ColumnDataType = "bigint", IsNullable = true)]
        public long? F_FileSize { get; set; }

        [SugarColumn(ColumnName = "F_Deadline", ColumnDescription = "截止时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_Deadline { get; set; }

        [SugarColumn(ColumnName = "F_FullScore", ColumnDescription = "满分", ColumnDataType = "decimal(5,1)", IsNullable = true)]
        public decimal? F_FullScore { get; set; }

        /// <summary>
        /// 0=草稿，1=已发布
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0草稿1已发布)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

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
