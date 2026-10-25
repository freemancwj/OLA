using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 学生列表展示扩展（sys_user + school_student）
    /// </summary>
    [SugarTable("sys_user")]
    public class StudentExtend
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_Account { get; set; }
        public string F_RealName { get; set; }
        public bool? F_Gender { get; set; }
        public DateTime? F_Birthday { get; set; }
        public string F_MobilePhone { get; set; }
        public string F_OrganizeId { get; set; }
        public bool? F_EnabledMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        /// <summary>
        /// 附加信息主键
        /// </summary>
        public string F_StudentInfoId { get; set; }
        /// <summary>
        /// 学籍号
        /// </summary>
        public string F_StudentNo { get; set; }
        /// <summary>
        /// 籍贯
        /// </summary>
        public string F_NativePlace { get; set; }
        /// <summary>
        /// 家长姓名
        /// </summary>
        public string F_ParentName { get; set; }
        /// <summary>
        /// 家长手机
        /// </summary>
        public string F_ParentPhone { get; set; }
        /// <summary>
        /// 班级名称
        /// </summary>
        public string F_ClassName { get; set; }
    }

    /// <summary>
    /// 学生新增/编辑表单
    /// </summary>
    public class StudentFormModel
    {
        /// <summary>
        /// 用户Id（空表示新增）
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        public string F_Account { get; set; }
        /// <summary>
        /// 密码（新增必填，修改留空表示不修改）
        /// </summary>
        public string F_UserPassword { get; set; }
        /// <summary>
        /// 学校
        /// </summary>
        public string F_CompanyId { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string F_RoleId { get; set; }
        /// <summary>
        /// 角色名称（仅用于展示）
        /// </summary>
        public string F_RoleName { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string F_DutyId { get; set; }
        /// <summary>
        /// 学生姓名
        /// </summary>
        public string F_RealName { get; set; }
        /// <summary>
        /// 所属班级
        /// </summary>
        public string F_OrganizeId { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public bool? F_Gender { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? F_Birthday { get; set; }
        /// <summary>
        /// 籍贯
        /// </summary>
        public string F_NativePlace { get; set; }
        /// <summary>
        /// 家长姓名
        /// </summary>
        public string F_ParentName { get; set; }
        /// <summary>
        /// 家长手机
        /// </summary>
        public string F_ParentPhone { get; set; }
        /// <summary>
        /// 学籍号
        /// </summary>
        public string F_StudentNo { get; set; }
        /// <summary>
        /// 学生手机号（非必填）
        /// </summary>
        public string F_MobilePhone { get; set; }
    }
}
