using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 题库列表展示扩展
    /// </summary>
    [SugarTable("school_question")]
    public class QuestionExtend : QuestionEntity
    {
        /// <summary>
        /// 科目名称
        /// </summary>
        public string F_CourseName { get; set; }
        /// <summary>
        /// 题型名称
        /// </summary>
        public string F_TypeName { get; set; }
        /// <summary>
        /// 难度名称
        /// </summary>
        public string F_DifficultyName { get; set; }
    }
}
