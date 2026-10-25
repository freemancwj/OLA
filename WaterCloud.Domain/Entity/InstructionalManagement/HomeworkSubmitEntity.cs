using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 作业提交记录
    /// </summary>
    [SugarTable("school_homework_submit")]
    public class HomeworkSubmitEntity : IEntity<HomeworkSubmitEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_HomeworkId", ColumnDescription = "作业Id", ColumnDataType = "nvarchar(50)")]
        public string F_HomeworkId { get; set; }

        [SugarColumn(ColumnName = "F_StudentId", ColumnDescription = "学生Id", ColumnDataType = "nvarchar(50)")]
        public string F_StudentId { get; set; }

        [SugarColumn(ColumnName = "F_Content", ColumnDescription = "作答内容", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Content { get; set; }

        [SugarColumn(ColumnName = "F_FileUrl", ColumnDescription = "附件地址", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_FileUrl { get; set; }

        [SugarColumn(ColumnName = "F_FileName", ColumnDescription = "附件名称", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_FileName { get; set; }

        [SugarColumn(ColumnName = "F_FileSize", ColumnDescription = "附件大小", ColumnDataType = "bigint", IsNullable = true)]
        public long? F_FileSize { get; set; }

        [SugarColumn(ColumnName = "F_SubmitTime", ColumnDescription = "提交时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_SubmitTime { get; set; }

        /// <summary>
        /// 0=待批改，1=已批改
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0待批改1已批改)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_Score", ColumnDescription = "分数", ColumnDataType = "decimal(5,1)", IsNullable = true)]
        public decimal? F_Score { get; set; }

        [SugarColumn(ColumnName = "F_Comment", ColumnDescription = "评语", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Comment { get; set; }

        [SugarColumn(ColumnName = "F_GradeTime", ColumnDescription = "批改时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_GradeTime { get; set; }

        [SugarColumn(ColumnName = "F_GraderId", ColumnDescription = "批改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_GraderId { get; set; }

        [SugarColumn(ColumnName = "F_GraderName", ColumnDescription = "批改人姓名", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_GraderName { get; set; }

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
