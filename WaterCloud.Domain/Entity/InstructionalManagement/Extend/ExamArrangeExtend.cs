namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 考务安排扩展（含课程、班级、教室、时间段、监考名称）
    /// </summary>
    public class ExamArrangeExtend : ExamArrangeEntity
    {
        public string F_CourseName { get; set; }
        public string F_ClassName { get; set; }
        /// <summary>
        /// 教学楼-教室名
        /// </summary>
        public string F_ClassroomName { get; set; }
        /// <summary>
        /// 名称(起-止)
        /// </summary>
        public string F_PeriodName { get; set; }
        public string F_MainInvigilatorName { get; set; }
        public string F_SubInvigilatorName { get; set; }
    }

    /// <summary>
    /// 教室下拉选项（含教学楼）
    /// </summary>
    public class ClassroomOptionExtend
    {
        public string F_Id { get; set; }
        public string F_Name { get; set; }
        public string F_BuildingName { get; set; }
        public int? F_Capacity { get; set; }
    }

    /// <summary>
    /// 考试时间段下拉选项
    /// </summary>
    public class PeriodOptionExtend
    {
        public string F_Id { get; set; }
        public string F_Name { get; set; }
        public string F_StartTime { get; set; }
        public string F_EndTime { get; set; }
    }
}
