using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Domain.SystemOrganize
{
    /// <summary>
    /// 员工请假申请实体
    /// </summary>
    public class LeaveApplicationEntity : IEntity<LeaveApplicationEntity>, ICreationAudited, IModificationAudited
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        public string F_Id { get; set; }
   

        /// <summary>
        /// 流程实例ID
        /// </summary>
        public string F_FlowInstanceId { get; set; }

        /// <summary>
        /// 员工Id
        /// </summary>
        public string F_EmployeeId { get; set; }

        /// <summary>
        /// 请假类型
        /// </summary>
        public int F_LeaveType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime F_StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime F_EndTime { get; set; }

        /// <summary>
        /// 请假天数
        /// </summary>
        public double F_LeaveDays { get; set; }

        /// <summary>
        /// 请假原因
        /// </summary>
        public string F_Reason { get; set; }

        /// <summary>
        /// 审批状态
        /// </summary>
        public int F_Status { get; set; }

        /// <summary>
        /// 当前审批人
        /// </summary>
        public string F_CurrentApprover { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? F_CreatorTime { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string F_CreatorUserId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? F_LastModifyTime { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        public string F_LastModifyUserId { get; set; }

        /// <summary>
        /// 申请人名称（扩展属性，不映射到数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string F_ApplicantName { get; set; }
    }
}