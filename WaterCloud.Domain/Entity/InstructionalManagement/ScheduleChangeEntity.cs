using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 调课申请（对已有课表格子的调整申请与审批）
    /// </summary>
    [SugarTable("school_schedule_change")]
    public class ScheduleChangeEntity : IEntity<ScheduleChangeEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_TimetableId", ColumnDescription = "原课表Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TimetableId { get; set; }

        [SugarColumn(ColumnName = "F_Semester", ColumnDescription = "学期", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_Semester { get; set; }

        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_TeacherId", ColumnDescription = "教师Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TeacherId { get; set; }

        [SugarColumn(ColumnName = "F_OldWeekDay", ColumnDescription = "原星期", IsNullable = true)]
        public int? F_OldWeekDay { get; set; }

        [SugarColumn(ColumnName = "F_OldSection", ColumnDescription = "原节次", IsNullable = true)]
        public int? F_OldSection { get; set; }

        [SugarColumn(ColumnName = "F_NewWeekDay", ColumnDescription = "新星期", IsNullable = true)]
        public int? F_NewWeekDay { get; set; }

        [SugarColumn(ColumnName = "F_NewSection", ColumnDescription = "新节次", IsNullable = true)]
        public int? F_NewSection { get; set; }

        [SugarColumn(ColumnName = "F_NewClassroomId", ColumnDescription = "新教室Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_NewClassroomId { get; set; }

        [SugarColumn(ColumnName = "F_Reason", ColumnDescription = "调课原因", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Reason { get; set; }

        [SugarColumn(ColumnName = "F_ApplicantId", ColumnDescription = "申请人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ApplicantId { get; set; }

        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0待审批 1已通过 2已拒绝)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_AuditUserId", ColumnDescription = "审批人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_AuditUserId { get; set; }

        [SugarColumn(ColumnName = "F_AuditTime", ColumnDescription = "审批时间", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_AuditTime { get; set; }

        [SugarColumn(ColumnName = "F_AuditRemark", ColumnDescription = "审批意见", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_AuditRemark { get; set; }

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
