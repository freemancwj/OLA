using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 可选课程扩展（含名称与已选人数）
    /// </summary>
    public class ElectiveCourseExtend : ElectiveCourseEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_RoundName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_TeacherName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_ClassroomName { get; set; }
        /// <summary>
        /// 已选人数
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int F_SelectedCount { get; set; }
        /// <summary>
        /// 当前学生是否已选（学生端用）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool F_IsSelected { get; set; }
    }

    /// <summary>
    /// 选课记录扩展
    /// </summary>
    public class ElectiveRecordExtend : ElectiveRecordEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_StudentName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_Account { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_ClassName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_TeacherName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_RoundName { get; set; }
    }
}
