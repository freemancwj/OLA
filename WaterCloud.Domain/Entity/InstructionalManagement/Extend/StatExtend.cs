namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 成绩统计行（按学期+课程+班级分组）
    /// </summary>
    public class ScoreStatModel
    {
        public string F_Semester { get; set; }
        public string F_CourseName { get; set; }
        public string F_ClassName { get; set; }
        public int F_TotalCount { get; set; }
        public decimal? F_AvgScore { get; set; }
        public decimal? F_MaxScore { get; set; }
        public decimal? F_MinScore { get; set; }
        public int F_PassCount { get; set; }
        public int F_ExcellentCount { get; set; }
        public int F_Seg90 { get; set; }
        public int F_Seg80 { get; set; }
        public int F_Seg70 { get; set; }
        public int F_Seg60 { get; set; }
        public int F_SegFail { get; set; }
    }

    /// <summary>
    /// 选课统计行（按课程班次）
    /// </summary>
    public class ElectiveStatModel
    {
        public string F_RoundName { get; set; }
        public string F_CourseName { get; set; }
        public string F_TeacherName { get; set; }
        public string F_ClassroomName { get; set; }
        public int? F_WeekDay { get; set; }
        public int? F_Section { get; set; }
        public int? F_Capacity { get; set; }
        public int F_SelectedCount { get; set; }
    }

    /// <summary>
    /// 教师工作量统计行
    /// </summary>
    public class WorkloadStatModel
    {
        public string F_TeacherId { get; set; }
        public string F_TeacherName { get; set; }
        public string F_Account { get; set; }
        public int F_CourseCount { get; set; }
        public int F_LessonCount { get; set; }
        public int F_InvigilateCount { get; set; }
    }

    /// <summary>
    /// 补考清考统计行（按学期+课程分组）
    /// </summary>
    public class ExamStatModel
    {
        public string F_Semester { get; set; }
        public string F_CourseName { get; set; }
        public int F_MakeupCount { get; set; }
        public int F_MakeupPassCount { get; set; }
        public int F_ClearCount { get; set; }
        public int F_ClearPassCount { get; set; }
    }

    /// <summary>
    /// 学期下拉项
    /// </summary>
    public class SemesterItem
    {
        public string F_Semester { get; set; }
    }

    /// <summary>
    /// 监考次数临时模型
    /// </summary>
    public class InvigilateCountModel
    {
        public string F_TeacherId { get; set; }
        public int F_Count { get; set; }
    }
}
