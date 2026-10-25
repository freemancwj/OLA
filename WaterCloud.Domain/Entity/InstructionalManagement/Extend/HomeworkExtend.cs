using SqlSugar;
using System;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 作业列表展示扩展
    /// </summary>
    [SugarTable("school_homework")]
    public class HomeworkExtend : HomeworkEntity
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
        /// 已交人数
        /// </summary>
        public int F_SubmitCount { get; set; }
        /// <summary>
        /// 班级应交人数
        /// </summary>
        public int F_TotalCount { get; set; }
        /// <summary>
        /// 待批改数
        /// </summary>
        public int F_PendingCount { get; set; }
        /// <summary>
        /// 已批改数
        /// </summary>
        public int F_GradedCount { get; set; }

        /// <summary>
        /// 学生视角：本人提交记录Id
        /// </summary>
        public string F_SubmitId { get; set; }
        /// <summary>
        /// 学生视角：本人提交状态(0待批改1已批改)
        /// </summary>
        public int? F_SubmitStatus { get; set; }
        /// <summary>
        /// 学生视角：本人得分
        /// </summary>
        public decimal? F_Score { get; set; }
        /// <summary>
        /// 学生视角：本人提交时间
        /// </summary>
        public DateTime? F_SubmitTime { get; set; }
    }

    /// <summary>
    /// 作业提交记录展示扩展
    /// </summary>
    [SugarTable("school_homework_submit")]
    public class HomeworkSubmitExtend : HomeworkSubmitEntity
    {
        /// <summary>
        /// 学生账号
        /// </summary>
        public string F_Account { get; set; }
        /// <summary>
        /// 学生姓名
        /// </summary>
        public string F_RealName { get; set; }
        /// <summary>
        /// 班级名称
        /// </summary>
        public string F_ClassName { get; set; }
        /// <summary>
        /// 是否已提交
        /// </summary>
        public bool F_IsSubmitted { get; set; }
        /// <summary>
        /// 作业标题
        /// </summary>
        public string F_HomeworkTitle { get; set; }
        /// <summary>
        /// 作业要求
        /// </summary>
        public string F_HomeworkContent { get; set; }
        /// <summary>
        /// 作业截止时间
        /// </summary>
        public DateTime? F_HomeworkDeadline { get; set; }
        /// <summary>
        /// 作业满分
        /// </summary>
        public decimal? F_HomeworkFullScore { get; set; }
        /// <summary>
        /// 作业附件地址
        /// </summary>
        public string F_HomeworkFileUrl { get; set; }
        /// <summary>
        /// 作业附件名称
        /// </summary>
        public string F_HomeworkFileName { get; set; }
    }

    /// <summary>
    /// 学生端作业详情（作业信息 + 本人提交信息）
    /// </summary>
    public class HomeworkDetailModel
    {
        public string F_Id { get; set; }
        public string F_Title { get; set; }
        public string F_CourseName { get; set; }
        public string F_ClassName { get; set; }
        public string F_Content { get; set; }
        public string F_FileUrl { get; set; }
        public string F_FileName { get; set; }
        public long? F_FileSize { get; set; }
        public DateTime? F_Deadline { get; set; }
        public decimal? F_FullScore { get; set; }
        public int? F_Status { get; set; }

        public string F_MySubmitId { get; set; }
        public int? F_MyStatus { get; set; }
        public decimal? F_MyScore { get; set; }
        public string F_MyComment { get; set; }
        public string F_MyContent { get; set; }
        public string F_MyFileUrl { get; set; }
        public string F_MyFileName { get; set; }
        public long? F_MyFileSize { get; set; }
        public DateTime? F_MySubmitTime { get; set; }
    }
}
