using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 培养方案扩展（含专业/课程名称）
    /// </summary>
    public class ProgramExtend : ProgramEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_MajorName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
    }

    /// <summary>
    /// 教学任务扩展（含教师/课程/班级名称）
    /// </summary>
    public class TeachingTaskExtend : TeachingTaskEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_TeacherName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_ClassName { get; set; }
    }

    /// <summary>
    /// 不排课时间扩展（含对象名称）
    /// </summary>
    public class NoScheduleExtend : NoScheduleEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_TargetName { get; set; }
    }

    /// <summary>
    /// 调课申请扩展（含名称信息）
    /// </summary>
    public class ScheduleChangeExtend : ScheduleChangeEntity
    {
        [SugarColumn(IsIgnore = true)]
        public string F_ClassName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_CourseName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_TeacherName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_ApplicantName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_NewClassroomName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string F_AuditUserName { get; set; }
    }
}
