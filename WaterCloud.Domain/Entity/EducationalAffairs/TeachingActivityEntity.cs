using System;
using SqlSugar;

namespace WaterCloud.Domain.EducationalAffairs
{
    /// <summary>
    /// 教研活动培训通知
    /// </summary>
    [SugarTable("ea_teaching_activity")]
    public class TeachingActivityEntity : IEntity<TeachingActivityEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_Title", ColumnDescription = "活动标题", ColumnDataType = "nvarchar(200)")]
        public string F_Title { get; set; }

        /// <summary>
        /// 1=教研活动，2=培训
        /// </summary>
        [SugarColumn(ColumnName = "F_Type", ColumnDescription = "类型(1教研活动2培训)", ColumnDataType = "int", IsNullable = true)]
        public int? F_Type { get; set; }

        [SugarColumn(ColumnName = "F_StartTime", ColumnDescription = "开始时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_StartTime { get; set; }

        [SugarColumn(ColumnName = "F_EndTime", ColumnDescription = "结束时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_EndTime { get; set; }

        [SugarColumn(ColumnName = "F_Location", ColumnDescription = "活动地点", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_Location { get; set; }

        [SugarColumn(ColumnName = "F_Speaker", ColumnDescription = "主讲人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_Speaker { get; set; }

        [SugarColumn(ColumnName = "F_Content", ColumnDescription = "通知内容", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Content { get; set; }

        [SugarColumn(ColumnName = "F_FileUrl", ColumnDescription = "附件地址", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_FileUrl { get; set; }

        [SugarColumn(ColumnName = "F_FileName", ColumnDescription = "附件名称", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_FileName { get; set; }

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
