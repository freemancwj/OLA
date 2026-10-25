using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 不排课时间（全校/教室/教师 在指定星期节次不可排课）
    /// </summary>
    [SugarTable("school_no_schedule")]
    public class NoScheduleEntity : IEntity<NoScheduleEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_Type", ColumnDescription = "类型(1全校 2教室 3教师)", IsNullable = true)]
        public int? F_Type { get; set; }

        [SugarColumn(ColumnName = "F_TargetId", ColumnDescription = "对象Id(全校时为空)", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TargetId { get; set; }

        [SugarColumn(ColumnName = "F_WeekDay", ColumnDescription = "星期(1-7)", IsNullable = true)]
        public int? F_WeekDay { get; set; }

        [SugarColumn(ColumnName = "F_Section", ColumnDescription = "节次", IsNullable = true)]
        public int? F_Section { get; set; }

        [SugarColumn(ColumnName = "F_Reason", ColumnDescription = "原因", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_Reason { get; set; }

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
