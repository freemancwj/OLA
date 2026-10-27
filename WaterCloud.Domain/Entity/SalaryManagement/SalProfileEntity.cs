using System;
using SqlSugar;

namespace WaterCloud.Domain.SalaryManagement
{
    /// <summary>
    /// 教职工薪酬档案（一人一档，算薪基础）
    /// </summary>
    [SugarTable("sal_profile")]
    public class SalProfileEntity : IEntity<SalProfileEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_UserId", ColumnDescription = "用户Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_UserId { get; set; }

        [SugarColumn(ColumnName = "F_JobNumber", ColumnDescription = "工号", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_JobNumber { get; set; }

        [SugarColumn(ColumnName = "F_EntryDate", ColumnDescription = "入职时间", IsNullable = true)]
        public DateTime? F_EntryDate { get; set; }

        [SugarColumn(ColumnName = "F_TeachingYears", ColumnDescription = "教龄(年)", IsNullable = true)]
        public int? F_TeachingYears { get; set; }

        /// <summary>岗位属性 1专任教师 2行政 3后勤 4校医 5临聘代课</summary>
        [SugarColumn(ColumnName = "F_PostType", ColumnDescription = "岗位属性", IsNullable = true)]
        public int? F_PostType { get; set; }

        /// <summary>职称 1二级 2一级 3高级 4正高级</summary>
        [SugarColumn(ColumnName = "F_TitleLevel", ColumnDescription = "职称等级", IsNullable = true)]
        public int? F_TitleLevel { get; set; }

        [SugarColumn(ColumnName = "F_IsHeadTeacher", ColumnDescription = "是否班主任", IsNullable = true)]
        public bool? F_IsHeadTeacher { get; set; }

        [SugarColumn(ColumnName = "F_IsDeputyHead", ColumnDescription = "是否副班主任", IsNullable = true)]
        public bool? F_IsDeputyHead { get; set; }

        [SugarColumn(ColumnName = "F_IsGroupLeader", ColumnDescription = "是否教研组长", IsNullable = true)]
        public bool? F_IsGroupLeader { get; set; }

        [SugarColumn(ColumnName = "F_IsLessonLeader", ColumnDescription = "是否备课组长", IsNullable = true)]
        public bool? F_IsLessonLeader { get; set; }

        [SugarColumn(ColumnName = "F_IsGraduationTeacher", ColumnDescription = "是否毕业班任课", IsNullable = true)]
        public bool? F_IsGraduationTeacher { get; set; }

        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班主任所在班级", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

        [SugarColumn(ColumnName = "F_SocialBase", ColumnDescription = "社保公积金基数", IsNullable = true)]
        public decimal? F_SocialBase { get; set; }

        [SugarColumn(ColumnName = "F_TaxDeduction", ColumnDescription = "个税专项附加扣除/月", IsNullable = true)]
        public decimal? F_TaxDeduction { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

        [SugarColumn(ColumnName = "F_EnabledMark", ColumnDescription = "有效标记", IsNullable = true)]
        public bool? F_EnabledMark { get; set; }
        [SugarColumn(ColumnName = "F_DeleteMark", ColumnDescription = "删除标记", IsNullable = true)]
        public bool? F_DeleteMark { get; set; }
        [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间", IsNullable = true)]
        public DateTime? F_CreatorTime { get; set; }
        [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CreatorUserId { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间", IsNullable = true)]
        public DateTime? F_LastModifyTime { get; set; }
        [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_LastModifyUserId { get; set; }
        [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间", IsNullable = true)]
        public DateTime? F_DeleteTime { get; set; }
        [SugarColumn(ColumnName = "F_DeleteUserId", ColumnDescription = "删除人", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>薪酬档案列表视图模型</summary>
    public class SalProfileExtend : SalProfileEntity
    {
        public string F_Account { get; set; }
        public string F_RealName { get; set; }
        public string F_OrganizeName { get; set; }
        public string F_ClassName { get; set; }
    }
}
