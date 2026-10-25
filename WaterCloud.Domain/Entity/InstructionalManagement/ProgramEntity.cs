using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 培养方案（专业+学期+课程）
    /// </summary>
    [SugarTable("school_program")]
    public class ProgramEntity : IEntity<ProgramEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_MajorId", ColumnDescription = "专业Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_MajorId { get; set; }

        [SugarColumn(ColumnName = "F_Semester", ColumnDescription = "学期", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_Semester { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_Credit", ColumnDescription = "学分", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_Credit { get; set; }

        [SugarColumn(ColumnName = "F_Period", ColumnDescription = "学时", IsNullable = true)]
        public int? F_Period { get; set; }

        [SugarColumn(ColumnName = "F_Nature", ColumnDescription = "课程性质(必修/选修/限选)", ColumnDataType = "nvarchar(20)", IsNullable = true)]
        public string F_Nature { get; set; }

        [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "备注", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Remark { get; set; }

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
