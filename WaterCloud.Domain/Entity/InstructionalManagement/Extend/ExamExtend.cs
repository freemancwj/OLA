using System;
using System.Collections.Generic;
using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 考试列表扩展（教师端/学生端列表使用）
    /// </summary>
    [SugarTable("school_exam")]
    public class ExamExtend : ExamEntity
    {
        /// <summary>
        /// 科目名称
        /// </summary>
        public string F_CourseName { get; set; }

        /// <summary>
        /// 班级名称
        /// </summary>
        public string F_ClassName { get; set; }

        /// <summary>
        /// 试卷题目数
        /// </summary>
        public int F_QuestionCount { get; set; }

        /// <summary>
        /// 参考人数
        /// </summary>
        public int F_RecordCount { get; set; }

        /// <summary>
        /// 学生端：本人考试记录Id（未参加为空）
        /// </summary>
        public string F_RecordId { get; set; }

        /// <summary>
        /// 学生端：本人考试记录状态（0待批改，1已完成）
        /// </summary>
        public int? F_RecordStatus { get; set; }

        /// <summary>
        /// 学生端：本人得分
        /// </summary>
        public decimal? F_GotScore { get; set; }

        /// <summary>
        /// 学生端：本人是否及格（1是 0否）
        /// </summary>
        public int? F_IsPassed { get; set; }
    }

    /// <summary>
    /// 试卷题目扩展（题库列表与卷面题目共用）
    /// </summary>
    [SugarTable("school_question")]
    public class ExamQuestionExtend
    {
        /// <summary>
        /// 试卷题目行Id（题库列表中为题目Id）
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 题库题目Id
        /// </summary>
        public string F_QuestionId { get; set; }

        /// <summary>
        /// 题型(1单选2多选3判断4简答)
        /// </summary>
        public int? F_Type { get; set; }

        /// <summary>
        /// 题型名称
        /// </summary>
        public string F_TypeName { get; set; }

        /// <summary>
        /// 题干
        /// </summary>
        public string F_Content { get; set; }

        public string F_OptionA { get; set; }

        public string F_OptionB { get; set; }

        public string F_OptionC { get; set; }

        public string F_OptionD { get; set; }

        /// <summary>
        /// 正确答案
        /// </summary>
        public string F_Answer { get; set; }

        /// <summary>
        /// 答案解析
        /// </summary>
        public string F_Analysis { get; set; }

        /// <summary>
        /// 卷面分值
        /// </summary>
        public decimal? F_Score { get; set; }

        /// <summary>
        /// 题库默认分值
        /// </summary>
        public decimal? F_FullScore { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int? F_SortCode { get; set; }

        /// <summary>
        /// 科目名称
        /// </summary>
        public string F_CourseName { get; set; }
    }

    /// <summary>
    /// 考试表单模型（考试基本信息 + 已选试卷题目）
    /// </summary>
    public class ExamFormModel : ExamEntity
    {
        /// <summary>
        /// 试卷题目
        /// </summary>
        public List<ExamQuestionExtend> Questions { get; set; } = new List<ExamQuestionExtend>();
    }

    /// <summary>
    /// 考试记录扩展（成绩/批改列表使用）
    /// </summary>
    [SugarTable("school_exam_record")]
    public class ExamRecordExtend
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public string F_Id { get; set; }

        public string F_ExamId { get; set; }

        public string F_ExamTitle { get; set; }

        public string F_StudentId { get; set; }

        /// <summary>
        /// 学生账号
        /// </summary>
        public string F_Account { get; set; }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public string F_RealName { get; set; }

        /// <summary>
        /// 学生班级
        /// </summary>
        public string F_ClassName { get; set; }

        public DateTime? F_StartTime { get; set; }

        public DateTime? F_SubmitTime { get; set; }

        public decimal? F_ObjectiveScore { get; set; }

        public decimal? F_SubjectiveScore { get; set; }

        public decimal? F_TotalScore { get; set; }

        /// <summary>
        /// 记录状态(0待批改1已完成)
        /// </summary>
        public int? F_Status { get; set; }

        /// <summary>
        /// 是否及格(1是0否)
        /// </summary>
        public int? F_IsPassed { get; set; }

        public DateTime? F_GradeTime { get; set; }

        public string F_GraderId { get; set; }

        public string F_GraderName { get; set; }

        /// <summary>
        /// 试卷总分
        /// </summary>
        public decimal? F_ExamTotalScore { get; set; }

        /// <summary>
        /// 及格分
        /// </summary>
        public decimal? F_PassScore { get; set; }

        /// <summary>
        /// 试卷题目数
        /// </summary>
        public int F_QuestionCount { get; set; }

        /// <summary>
        /// 考试时长(分钟)
        /// </summary>
        public int? F_Duration { get; set; }

        /// <summary>
        /// 答题明细
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ExamAnswerExtend> Answers { get; set; } = new List<ExamAnswerExtend>();
    }

    /// <summary>
    /// 答题明细扩展（批改页/成绩详情使用）
    /// </summary>
    [SugarTable("school_exam_answer")]
    public class ExamAnswerExtend
    {
        public string F_Id { get; set; }

        public string F_QuestionId { get; set; }

        /// <summary>
        /// 题型(1单选2多选3判断4简答)
        /// </summary>
        public int? F_QuestionType { get; set; }

        /// <summary>
        /// 题型名称
        /// </summary>
        public string F_TypeName { get; set; }

        /// <summary>
        /// 题干
        /// </summary>
        public string F_Content { get; set; }

        public string F_OptionA { get; set; }

        public string F_OptionB { get; set; }

        public string F_OptionC { get; set; }

        public string F_OptionD { get; set; }

        /// <summary>
        /// 正确答案（学生端成绩详情使用）
        /// </summary>
        public string F_Answer { get; set; }

        /// <summary>
        /// 答案解析
        /// </summary>
        public string F_Analysis { get; set; }

        /// <summary>
        /// 学生答案
        /// </summary>
        public string F_StudentAnswer { get; set; }

        /// <summary>
        /// 是否正确(1对0错2待批改)
        /// </summary>
        public int? F_IsCorrect { get; set; }

        /// <summary>
        /// 本题得分
        /// </summary>
        public decimal? F_Score { get; set; }

        /// <summary>
        /// 本题满分
        /// </summary>
        public decimal? F_FullScore { get; set; }

        /// <summary>
        /// 批改评语
        /// </summary>
        public string F_Comment { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int? F_SortCode { get; set; }
    }

    /// <summary>
    /// 学生答题模型（答题页面使用，题目不含答案与解析）
    /// </summary>
    public class ExamTakeModel
    {
        /// <summary>
        /// 考试记录Id
        /// </summary>
        public string F_RecordId { get; set; }

        public string F_ExamId { get; set; }

        public string F_Title { get; set; }

        /// <summary>
        /// 考试时长(分钟)
        /// </summary>
        public int? F_Duration { get; set; }

        /// <summary>
        /// 剩余答题秒数
        /// </summary>
        public int F_RemainingSeconds { get; set; }

        /// <summary>
        /// 试卷总分
        /// </summary>
        public decimal? F_TotalScore { get; set; }

        /// <summary>
        /// 试卷题目（不含正确答案与解析）
        /// </summary>
        public List<ExamQuestionExtend> Questions { get; set; } = new List<ExamQuestionExtend>();
    }

    /// <summary>
    /// 错题扩展（学生错题本使用）
    /// </summary>
    [SugarTable("school_exam_answer")]
    public class WrongQuestionExtend
    {
        /// <summary>
        /// 答题明细Id
        /// </summary>
        public string F_Id { get; set; }

        public string F_QuestionId { get; set; }

        public string F_ExamTitle { get; set; }

        public string F_CourseName { get; set; }

        /// <summary>
        /// 题型(1单选2多选3判断4简答)
        /// </summary>
        public int? F_QuestionType { get; set; }

        /// <summary>
        /// 题干
        /// </summary>
        public string F_Content { get; set; }

        public string F_OptionA { get; set; }

        public string F_OptionB { get; set; }

        public string F_OptionC { get; set; }

        public string F_OptionD { get; set; }

        /// <summary>
        /// 正确答案
        /// </summary>
        public string F_Answer { get; set; }

        /// <summary>
        /// 学生答案
        /// </summary>
        public string F_StudentAnswer { get; set; }

        /// <summary>
        /// 答案解析
        /// </summary>
        public string F_Analysis { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? F_CreateTime { get; set; }

        /// <summary>
        /// 学生姓名（教师端查看）
        /// </summary>
        public string F_StudentName { get; set; }
    }
}
