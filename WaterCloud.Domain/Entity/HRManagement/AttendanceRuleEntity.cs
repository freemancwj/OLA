using System;
using SqlSugar;

namespace WaterCloud.Domain.HRManagement
{
    /// <summary>
    /// 打卡规则（单行配置）
    /// </summary>
    [SugarTable("hr_attendance_rule")]
    public class AttendanceRuleEntity : IEntity<AttendanceRuleEntity>, ICreationAudited, IModificationAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        /// <summary>
        /// 上班时间（HH:mm）
        /// </summary>
        [SugarColumn(ColumnName = "F_StartTime", ColumnDescription = "上班时间", ColumnDataType = "nvarchar(5)", IsNullable = true)]
        public string F_StartTime { get; set; }

        /// <summary>
        /// 下班时间（HH:mm）
        /// </summary>
        [SugarColumn(ColumnName = "F_EndTime", ColumnDescription = "下班时间", ColumnDataType = "nvarchar(5)", IsNullable = true)]
        public string F_EndTime { get; set; }

        /// <summary>
        /// 迟到宽限（分钟），上班时间后超过宽限算迟到
        /// </summary>
        [SugarColumn(ColumnName = "F_LateGrace", ColumnDescription = "迟到宽限分钟", IsNullable = true)]
        public int? F_LateGrace { get; set; }

        /// <summary>
        /// 早退宽限（分钟），下班时间前超过宽限算早退
        /// </summary>
        [SugarColumn(ColumnName = "F_EarlyGrace", ColumnDescription = "早退宽限分钟", IsNullable = true)]
        public int? F_EarlyGrace { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "F_EnabledMark", ColumnDescription = "是否启用", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }

        /// <summary>
        /// 打卡范围中心点经度（高德坐标系）
        /// </summary>
        [SugarColumn(ColumnName = "F_Longitude", ColumnDescription = "打卡中心点经度", ColumnDataType = "decimal(12,8)", IsNullable = true)]
        public decimal? F_Longitude { get; set; }

        /// <summary>
        /// 打卡范围中心点纬度（高德坐标系）
        /// </summary>
        [SugarColumn(ColumnName = "F_Latitude", ColumnDescription = "打卡中心点纬度", ColumnDataType = "decimal(12,8)", IsNullable = true)]
        public decimal? F_Latitude { get; set; }

        /// <summary>
        /// 中心点地址名称
        /// </summary>
        [SugarColumn(ColumnName = "F_PoiName", ColumnDescription = "打卡中心点地址", ColumnDataType = "nvarchar(300)", IsNullable = true)]
        public string F_PoiName { get; set; }

        /// <summary>
        /// 打卡有效范围（米），GPS定位在半径内算合法打卡
        /// </summary>
        [SugarColumn(ColumnName = "F_Radius", ColumnDescription = "打卡有效范围米", IsNullable = true)]
        public int? F_Radius { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }
    }
}
