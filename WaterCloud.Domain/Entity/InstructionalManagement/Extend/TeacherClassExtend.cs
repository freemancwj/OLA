using System;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 教师任课班级扩展（含教师、班级名称）
    /// </summary>
    public class TeacherClassExtend
    {
        public string F_Id { get; set; }
        public string F_TeacherId { get; set; }
        public string F_ClassId { get; set; }
        public string F_TeacherName { get; set; }
        public string F_Account { get; set; }
        public string F_ClassName { get; set; }
        public DateTime? F_CreatorTime { get; set; }
    }
}
