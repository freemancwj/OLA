using System;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 教师任课班级（一个教师可带多个班）
    /// </summary>
    [SugarTable("school_teacher_class")]
    public class TeacherClassEntity : IEntity<TeacherClassEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", ColumnDataType = "nvarchar(50)", IsPrimaryKey = true)]
        public string F_Id { get; set; }

        /// <summary>
        /// 教师用户Id（sys_user.F_Id）
        /// </summary>
        [SugarColumn(ColumnName = "F_TeacherId", ColumnDescription = "教师Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_TeacherId { get; set; }

        /// <summary>
        /// 班级Id（sys_organize.F_Id，banji 类）
        /// </summary>
        [SugarColumn(ColumnName = "F_ClassId", ColumnDescription = "班级Id", ColumnDataType = "nvarchar(50)", IsNullable = true)]
        public string F_ClassId { get; set; }

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
