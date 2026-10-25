using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 课时学习进度
    /// </summary>
    [SugarTable("school_course_progress")]
    public class CourseProgressEntity : IEntity<CourseProgressEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_LessonId", ColumnDescription = "课时Id", ColumnDataType = "nvarchar(50)")]
        public string F_LessonId { get; set; }

        [SugarColumn(ColumnName = "F_UserId", ColumnDescription = "用户Id", ColumnDataType = "nvarchar(50)")]
        public string F_UserId { get; set; }

        [SugarColumn(ColumnName = "F_Position", ColumnDescription = "已看位置(秒)", IsNullable = true)]
        public int? F_Position { get; set; }

        [SugarColumn(ColumnName = "F_Duration", ColumnDescription = "总时长(秒)", IsNullable = true)]
        public int? F_Duration { get; set; }

        [SugarColumn(ColumnName = "F_LastTime", ColumnDescription = "最近学习时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_LastTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }
    }
}
