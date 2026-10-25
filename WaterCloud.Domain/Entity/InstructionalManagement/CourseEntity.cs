using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 课程
    /// </summary>
    [SugarTable("school_course")]
    public class CourseEntity : IEntity<CourseEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        [SugarColumn(ColumnName = "F_CategoryId", ColumnDescription = "课程分类Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_CategoryId { get; set; }

        [SugarColumn(ColumnName = "F_Name", ColumnDescription = "课程名称", ColumnDataType = "nvarchar(200)")]
        public string F_Name { get; set; }

        [SugarColumn(ColumnName = "F_Cover", ColumnDescription = "封面图片", ColumnDataType = "nvarchar(500)", IsNullable = true)]
        public string F_Cover { get; set; }

        [SugarColumn(ColumnName = "F_TeacherId", ColumnDescription = "授课教师Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TeacherId { get; set; }

        /// <summary>
        /// 所属专业Id（sys_organize zhuanye 节点）
        /// </summary>
        [SugarColumn(ColumnName = "F_MajorId", ColumnDescription = "所属专业Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_MajorId { get; set; }

        /// <summary>
        /// 学分
        /// </summary>
        [SugarColumn(ColumnName = "F_Credit", ColumnDescription = "学分", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_Credit { get; set; }

        /// <summary>
        /// 学时
        /// </summary>
        [SugarColumn(ColumnName = "F_Period", ColumnDescription = "学时", IsNullable = true)]
        public int? F_Period { get; set; }

        /// <summary>
        /// 课程性质（必修/选修/公选）
        /// </summary>
        [SugarColumn(ColumnName = "F_Nature", ColumnDescription = "课程性质", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_Nature { get; set; }

        /// <summary>
        /// 考试分数（满分）
        /// </summary>
        [SugarColumn(ColumnName = "F_ExamScore", ColumnDescription = "考试分数", ColumnDataType = "decimal(18,2)", IsNullable = true)]
        public decimal? F_ExamScore { get; set; }

        [SugarColumn(ColumnName = "F_Intro", ColumnDescription = "课程简介", ColumnDataType = "nvarchar(max)", IsNullable = true)]
        public string F_Intro { get; set; }

        /// <summary>
        /// 0=下架，1=上架
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", ColumnDescription = "状态(0下架1上架)", ColumnDataType = "tinyint(3)", IsNullable = true)]
        public int? F_Status { get; set; }

        [SugarColumn(ColumnName = "F_SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int? F_SortCode { get; set; }

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
