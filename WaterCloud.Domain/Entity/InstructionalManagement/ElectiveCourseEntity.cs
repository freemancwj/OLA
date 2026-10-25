using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 可选课程（选课批次下开放的课程班次）
    /// </summary>
    [SugarTable("school_elective_course")]
    public class ElectiveCourseEntity : IEntity<ElectiveCourseEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_RoundId", ColumnDescription = "选课批次Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_RoundId { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_TeacherId", ColumnDescription = "授课教师Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TeacherId { get; set; }

        [SugarColumn(ColumnName = "F_Capacity", ColumnDescription = "容量", IsNullable = true)]
        public int? F_Capacity { get; set; }

        [SugarColumn(ColumnName = "F_WeekDay", ColumnDescription = "上课星期", IsNullable = true)]
        public int? F_WeekDay { get; set; }

        [SugarColumn(ColumnName = "F_Section", ColumnDescription = "上课节次", IsNullable = true)]
        public int? F_Section { get; set; }

        [SugarColumn(ColumnName = "F_ClassroomId", ColumnDescription = "教室Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassroomId { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

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
