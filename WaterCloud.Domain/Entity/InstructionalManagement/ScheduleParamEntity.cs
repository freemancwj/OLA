using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 排课参数（单条配置）
    /// </summary>
    [SugarTable("school_schedule_param")]
    public class ScheduleParamEntity : IEntity<ScheduleParamEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_DaysPerWeek", ColumnDescription = "每周排课天数", IsNullable = true)]
        public int? F_DaysPerWeek { get; set; }

        [SugarColumn(ColumnName = "F_SectionsPerDay", ColumnDescription = "每天节数", IsNullable = true)]
        public int? F_SectionsPerDay { get; set; }

        [SugarColumn(ColumnName = "F_MaxSameCourseDaily", ColumnDescription = "同科每天最多节数", IsNullable = true)]
        public int? F_MaxSameCourseDaily { get; set; }

        [SugarColumn(ColumnName = "F_MaxConsecutive", ColumnDescription = "同科最大连排节数(0不限制)", IsNullable = true)]
        public int? F_MaxConsecutive { get; set; }

        [SugarColumn(ColumnName = "F_Description", ColumnDescription = "说明", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Description { get; set; }

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
