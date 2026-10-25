using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Domain.SystemOrganize
{
    /// <summary>
    /// 请假申请实体（教师/学生通用，走工作流审批）
    /// </summary>
    [SugarTable("sys_leave")]
    public class LeaveApplicationEntity : IEntity<LeaveApplicationEntity>, ICreationAudited, IModificationAudited
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true, ColumnDescription = "主键Id")]
        public string F_Id { get; set; }

        /// <summary>
        /// 流程实例ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_FlowInstanceId", ColumnDataType = "nvarchar(50)", ColumnDescription = "流程实例ID")]
        public string F_FlowInstanceId { get; set; }

        /// <summary>
        /// 申请人类型 1教师 2学生
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "申请人类型 1教师 2学生")]
        public int F_ApplicantType { get; set; }

        /// <summary>
        /// 申请人Id（教师或学生的用户Id）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_EmployeeId", ColumnDataType = "nvarchar(50)", ColumnDescription = "申请人Id")]
        public string F_EmployeeId { get; set; }

        /// <summary>
        /// 请假类型 1年假 2病假 3事假 4婚假 5产假
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "请假类型")]
        public int F_LeaveType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "开始时间")]
        public DateTime F_StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "结束时间")]
        public DateTime F_EndTime { get; set; }

        /// <summary>
        /// 请假天数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "请假天数")]
        public double F_LeaveDays { get; set; }

        /// <summary>
        /// 请假原因
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_Reason", ColumnDataType = "nvarchar(500)", ColumnDescription = "请假原因")]
        public string F_Reason { get; set; }

        /// <summary>
        /// 审批状态 0审批中 1已通过 2未通过 3已驳回 4已撤回
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "审批状态 0审批中 1已通过 2未通过 3已驳回 4已撤回")]
        public int F_Status { get; set; }

        /// <summary>
        /// 当前审批人
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_CurrentApprover", ColumnDataType = "nvarchar(200)", ColumnDescription = "当前审批人")]
        public string F_CurrentApprover { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "创建时间")]
        public DateTime? F_CreatorTime { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_CreatorUserId", ColumnDataType = "nvarchar(50)", ColumnDescription = "创建人Id")]
        public string F_CreatorUserId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "最后修改时间")]
        public DateTime? F_LastModifyTime { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "F_LastModifyUserId", ColumnDataType = "nvarchar(50)", ColumnDescription = "最后修改人Id")]
        public string F_LastModifyUserId { get; set; }

        /// <summary>
        /// 申请人名称（扩展属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string F_ApplicantName { get; set; }
    }
}
