using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 清考管理
    /// </summary>
    [SugarTable("school_clear_exam")]
    public class ClearExamEntity : IEntity<ClearExamEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_StudentId", ColumnDescription = "学生Id", ColumnDataType = "nvarchar(50)")]
        public string F_StudentId { get; set; }

        [SugarColumn(ColumnName = "F_CourseId", ColumnDescription = "课程Id", ColumnDataType = "nvarchar(50)")]
        public string F_CourseId { get; set; }

        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

        [SugarColumn(ColumnName = "F_ScoreId", ColumnDescription = "来源成绩Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ScoreId { get; set; }

        [SugarColumn(ColumnName = "F_ExamDate", ColumnDescription = "考试日期", ColumnDataType = "datetime2(27)", IsNullable = true)]
        public DateTime? F_ExamDate { get; set; }

        [SugarColumn(ColumnName = "F_PeriodId", ColumnDescription = "考试时间段Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_PeriodId { get; set; }

        [SugarColumn(ColumnName = "F_ClassroomId", ColumnDescription = "教室Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassroomId { get; set; }

        [SugarColumn(ColumnName = "F_MainInvigilatorId", ColumnDescription = "主监考Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_MainInvigilatorId { get; set; }

        [SugarColumn(ColumnName = "F_SubInvigilatorId", ColumnDescription = "副监考Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_SubInvigilatorId { get; set; }

        /// <summary>
        /// 0待安排 1已安排 2已录分
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0待安排1已安排2已录分)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_NewScore", ColumnDescription = "清考成绩", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_NewScore { get; set; }

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
