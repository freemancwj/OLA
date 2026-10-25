using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 课时（章节下的视频小节）
    /// </summary>
    [SugarTable("school_course_lesson")]
    public class CourseLessonEntity : IEntity<CourseLessonEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)")]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_ChapterId", ColumnDescription = "章节Id", ColumnDataType = "nvarchar(50)")]
        public string F_ChapterId { get; set; }

        [SugarColumn(ColumnName = "F_Name", ColumnDescription = "课时名称", ColumnDataType = "nvarchar(200)")]
        public string F_Name { get; set; }

        [SugarColumn(ColumnName = "F_VideoUrl", ColumnDescription = "视频地址", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_VideoUrl { get; set; }

        [SugarColumn(ColumnName = "F_VideoDuration", ColumnDescription = "视频时长(秒)", IsNullable = true)]
        public int? F_VideoDuration { get; set; }

        [SugarColumn(ColumnName = "F_VideoSize", ColumnDescription = "视频大小(字节)", IsNullable = true)]
        public long? F_VideoSize { get; set; }

        [SugarColumn(ColumnName = "F_SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int? F_SortCode { get; set; }

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
