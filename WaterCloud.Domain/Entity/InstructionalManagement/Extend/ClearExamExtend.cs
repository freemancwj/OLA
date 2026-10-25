namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 清考管理扩展（含学生、班级、课程、教室、时间段、监考名称及原成绩）
    /// </summary>
    public class ClearExamExtend : ClearExamEntity
    {
        public string F_StudentName { get; set; }
        public string F_Account { get; set; }
        public string F_ClassName { get; set; }
        public string F_CourseName { get; set; }
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
        /// <summary>
        /// 原成绩（不及格分）
        /// </summary>
        public decimal? F_OldScore { get; set; }
    }
}
