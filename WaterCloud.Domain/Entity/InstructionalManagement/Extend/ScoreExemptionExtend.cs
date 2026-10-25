using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 免缓缺登记列表展示扩展
    /// </summary>
    [SugarTable("school_score_exemption")]
    public class ScoreExemptionExtend : ScoreExemptionEntity
    {
        /// <summary>
        /// 学生姓名
        /// </summary>
        public string F_StudentName { get; set; }
        /// <summary>
        /// 学生账号
        /// </summary>
        public string F_Account { get; set; }
        /// <summary>
        /// 班级名称
        /// </summary>
        public string F_ClassName { get; set; }
        /// <summary>
        /// 课程名称
        /// </summary>
        public string F_CourseName { get; set; }
        /// <summary>
        /// 审批人姓名
        /// </summary>
        public string F_AuditUserName { get; set; }
    }
}
