using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 授课计划扩展（含班级/科目/教师名称）
    /// </summary>
    [SugarTable("school_timetable_plan")]
    public class TimetablePlanExtend : TimetablePlanEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_ClassName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_TeacherName { get; set; }
    }

    /// <summary>
    /// 课表格子（矩阵单元）
    /// </summary>
    public class TimetableCellModel
    {
        public string F_Id { get; set; }
        public int F_WeekDay { get; set; }
        public int F_Section { get; set; }
        public string F_CourseId { get; set; }
        public string F_CourseName { get; set; }
        public string F_TeacherId { get; set; }
        public string F_TeacherName { get; set; }
        public string F_ClassId { get; set; }
        public string F_ClassName { get; set; }
        public string F_Classroom { get; set; }
        public string F_ClassroomId { get; set; }
    }
}
