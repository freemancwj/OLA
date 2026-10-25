using SqlSugar;
using System.Collections.Generic;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 课程列表展示扩展
    /// </summary>
    [SugarTable("school_course")]
    public class CourseExtend : CourseEntity
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        public string F_CategoryName { get; set; }
        /// <summary>
        /// 授课教师姓名
        /// </summary>
        public string F_TeacherName { get; set; }
        /// <summary>
        /// 所属专业名称
        /// </summary>
        public string F_MajorName { get; set; }
        /// <summary>
        /// 章节数
        /// </summary>
        public int ChapterCount { get; set; }
        /// <summary>
        /// 课时数
        /// </summary>
        public int LessonCount { get; set; }
    }

    /// <summary>
    /// 学生端课程卡片
    /// </summary>
    public class CourseCardModel
    {
        public string F_Id { get; set; }
        public string F_Name { get; set; }
        public string F_Cover { get; set; }
        public string F_Intro { get; set; }
        public string F_TeacherName { get; set; }
        public string F_CategoryName { get; set; }
        public int LessonCount { get; set; }
        /// <summary>
        /// 当前用户学习进度百分比(0-100)
        /// </summary>
        public int ProgressPercent { get; set; }
    }

    /// <summary>
    /// 学生端章节+课时树节点
    /// </summary>
    public class CourseStudyChapter
    {
        public string F_Id { get; set; }
        public string F_Name { get; set; }
        public int? F_SortCode { get; set; }
        public List<CourseStudyLesson> Lessons { get; set; } = new List<CourseStudyLesson>();
    }

    /// <summary>
    /// 学生端课时项
    /// </summary>
    public class CourseStudyLesson
    {
        public string F_Id { get; set; }
        public string F_ChapterId { get; set; }
        public string F_Name { get; set; }
        public string F_VideoUrl { get; set; }
        public int? F_VideoDuration { get; set; }
        public int? F_SortCode { get; set; }
        /// <summary>
        /// 当前用户已看位置(秒)
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// 当前用户进度百分比(0-100)
        /// </summary>
        public int ProgressPercent { get; set; }
    }

    /// <summary>
    /// 学生端课程学习详情
    /// </summary>
    public class CourseStudyInfo
    {
        public CourseEntity Course { get; set; }
        public List<CourseStudyChapter> Chapters { get; set; } = new List<CourseStudyChapter>();
        public List<CourseWareEntity> Wares { get; set; } = new List<CourseWareEntity>();
        /// <summary>
        /// 当前用户课程整体进度百分比(0-100)
        /// </summary>
        public int ProgressPercent { get; set; }
    }
}
