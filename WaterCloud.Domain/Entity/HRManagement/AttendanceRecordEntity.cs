using System;
using SqlSugar;

namespace WaterCloud.Domain.HRManagement
{
    /// <summary>
    /// 打卡记录
    /// </summary>
    [SugarTable("hr_attendance_record")]
    public class AttendanceRecordEntity : IEntity<AttendanceRecordEntity>
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_UserId", ColumnDescription = "用户Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_UserId { get; set; }

        /// <summary>
        /// 考勤日期（yyyy-MM-dd）
        /// </summary>
        [SugarColumn(ColumnName = "F_Date", ColumnDescription = "考勤日期", IsNullable = true)]
        public DateTime? F_Date { get; set; }

        [SugarColumn(ColumnName = "F_ClockInTime", ColumnDescription = "上班打卡时间", IsNullable = true)]
        public DateTime? F_ClockInTime { get; set; }

        /// <summary>
        /// 上班打卡状态（0正常 1迟到 2缺卡）
        /// </summary>
        [SugarColumn(ColumnName = "F_ClockInStatus", ColumnDescription = "上班状态", IsNullable = true)]
        public int? F_ClockInStatus { get; set; }

        [SugarColumn(ColumnName = "F_ClockOutTime", ColumnDescription = "下班打卡时间", IsNullable = true)]
        public DateTime? F_ClockOutTime { get; set; }

        /// <summary>
        /// 下班打卡状态（0正常 1早退 2缺卡）
        /// </summary>
        [SugarColumn(ColumnName = "F_ClockOutStatus", ColumnDescription = "下班状态", IsNullable = true)]
        public int? F_ClockOutStatus { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }
    }

    /// <summary>
    /// 打卡记录列表视图模型（联查 sys_user）
    /// </summary>
    public class AttendanceRecordExtend : AttendanceRecordEntity
    {
        public string F_Account { get; set; }
        public string F_RealName { get; set; }
        public string F_OrganizeName { get; set; }
    }
}
