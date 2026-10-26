using System;
using SqlSugar;

namespace WaterCloud.Domain.HRManagement
{
    /// <summary>
    /// 员工档案（扩展 sys_user 的人事信息）
    /// </summary>
    [SugarTable("hr_employee")]
    public class HrEmployeeEntity : IEntity<HrEmployeeEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        /// <summary>
        /// 关联用户Id（sys_user.F_Id，唯一）
        /// </summary>
        [SugarColumn(ColumnName = "F_UserId", ColumnDescription = "用户Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_UserId { get; set; }

        /// <summary>
        /// 学历（1高中/中专 2大专 3本科 4硕士 5博士 6其他）
        /// </summary>
        [SugarColumn(ColumnName = "F_Education", ColumnDescription = "学历", IsNullable = true)]
        public int? F_Education { get; set; }

        [SugarColumn(ColumnName = "F_EntryDate", ColumnDescription = "入职日期", IsNullable = true)]
        public DateTime? F_EntryDate { get; set; }

        [SugarColumn(ColumnName = "F_RegularDate", ColumnDescription = "转正日期", IsNullable = true)]
        public DateTime? F_RegularDate { get; set; }

        [SugarColumn(ColumnName = "F_Photo", ColumnDescription = "相片", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Photo { get; set; }

        [SugarColumn(ColumnName = "F_IdCard", ColumnDescription = "身份证号", ColumnDataType = "nvarchar(20)", IsNullable = true)]
        public string F_IdCard { get; set; }

        /// <summary>
        /// 婚姻状况（0未婚 1已婚 2其他）
        /// </summary>
        [SugarColumn(ColumnName = "F_MaritalStatus", ColumnDescription = "婚姻状况", IsNullable = true)]
        public int? F_MaritalStatus { get; set; }

        /// <summary>
        /// 政治面貌（0群众 1团员 2党员 3其他）
        /// </summary>
        [SugarColumn(ColumnName = "F_PoliticalStatus", ColumnDescription = "政治面貌", IsNullable = true)]
        public int? F_PoliticalStatus { get; set; }

        [SugarColumn(ColumnName = "F_GraduateSchool", ColumnDescription = "毕业院校", ColumnDataType = "nvarchar(100)", IsNullable = true)]
        public string F_GraduateSchool { get; set; }

        [SugarColumn(ColumnName = "F_Major", ColumnDescription = "专业", ColumnDataType = "nvarchar(100)", IsNullable = true)]
        public string F_Major { get; set; }

        [SugarColumn(ColumnName = "F_EmergencyContact", ColumnDescription = "紧急联系人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_EmergencyContact { get; set; }

        [SugarColumn(ColumnName = "F_EmergencyPhone", ColumnDescription = "紧急联系电话", ColumnDataType = "nvarchar(20)", IsNullable = true)]
        public string F_EmergencyPhone { get; set; }

        [SugarColumn(ColumnName = "F_Address", ColumnDescription = "家庭住址", ColumnDataType = "nvarchar(200)", IsNullable = true)]
        public string F_Address { get; set; }

        [SugarColumn(ColumnName = "F_ContractNo", ColumnDescription = "合同编号", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ContractNo { get; set; }

        [SugarColumn(ColumnName = "F_ContractStart", ColumnDescription = "合同开始日期", IsNullable = true)]
        public DateTime? F_ContractStart { get; set; }

        [SugarColumn(ColumnName = "F_ContractEnd", ColumnDescription = "合同结束日期", IsNullable = true)]
        public DateTime? F_ContractEnd { get; set; }

        [SugarColumn(ColumnName = "F_LeaveDate", ColumnDescription = "离职日期", IsNullable = true)]
        public DateTime? F_LeaveDate { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

        [SugarColumn(ColumnName = "F_EnabledMark", ColumnDescription = "有效标记", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }

        [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标记", IsNullable = true)]
        public bool? F_DeleteMark { get; set; }

        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }

        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }

        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }

        [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间", IsNullable = true)]
        public DateTime? F_DeleteTime { get; set; }

        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除人Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>
    /// 员工档案列表视图模型（联查 sys_user）
    /// </summary>
    public class HrEmployeeExtend : HrEmployeeEntity
    {
        public string F_Account { get; set; }
        public string F_RealName { get; set; }
        public bool? F_Gender { get; set; }
        public DateTime? F_Birthday { get; set; }
        public string F_MobilePhone { get; set; }
        public string F_Email { get; set; }
        public string F_OrganizeName { get; set; }
        public string F_RoleName { get; set; }
    }
}
