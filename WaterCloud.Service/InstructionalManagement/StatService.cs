using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 统计查询服务（成绩/选课/教师工作量/补考清考）
    /// </summary>
    public class StatService : BaseService<ScoresEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public StatService(ISqlSugarClient context) : base(context)
        {
        }

        #region 成绩统计

        /// <summary>
        /// 成绩统计（仅统计审核通过的成绩，按学期+课程+班级分组）
        /// </summary>
        public async Task<List<ScoreStatModel>> GetScoreStat(string semester, string courseId, string classId)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT s.F_Semester, c.F_Name AS F_CourseName, o.F_FullName AS F_ClassName,
	COUNT(1) AS F_TotalCount,
	CAST(AVG(CAST(s.F_Score AS float)) AS decimal(18,2)) AS F_AvgScore,
	MAX(s.F_Score) AS F_MaxScore,
	MIN(s.F_Score) AS F_MinScore,
	SUM(CASE WHEN s.F_Score >= 60 THEN 1 ELSE 0 END) AS F_PassCount,
	SUM(CASE WHEN s.F_Score >= 90 THEN 1 ELSE 0 END) AS F_ExcellentCount,
	SUM(CASE WHEN s.F_Score >= 90 THEN 1 ELSE 0 END) AS F_Seg90,
	SUM(CASE WHEN s.F_Score >= 80 AND s.F_Score < 90 THEN 1 ELSE 0 END) AS F_Seg80,
	SUM(CASE WHEN s.F_Score >= 70 AND s.F_Score < 80 THEN 1 ELSE 0 END) AS F_Seg70,
	SUM(CASE WHEN s.F_Score >= 60 AND s.F_Score < 70 THEN 1 ELSE 0 END) AS F_Seg60,
	SUM(CASE WHEN s.F_Score < 60 THEN 1 ELSE 0 END) AS F_SegFail
FROM school_scores s
LEFT JOIN school_course c ON c.F_Id COLLATE Chinese_PRC_CI_AS = s.F_CourseId COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON o.F_Id COLLATE Chinese_PRC_CI_AS = s.F_ClassId COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(s.F_DeleteMark,0) = 0 AND ISNULL(s.F_AuditStatus,0) = 1");
            if (!string.IsNullOrWhiteSpace(semester))
            {
                sb.Append(" AND s.F_Semester = '" + Q(semester) + "'");
            }
            if (!string.IsNullOrWhiteSpace(courseId))
            {
                sb.Append(" AND s.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(classId))
            {
                sb.Append(" AND s.F_ClassId = '" + Q(classId) + "'");
            }
            sb.Append(" GROUP BY s.F_Semester, c.F_Name, o.F_FullName ORDER BY s.F_Semester DESC, c.F_Name, o.F_FullName");
            return await repository.SqlQueryable<ScoreStatModel>(sb.ToString()).ToListAsync();
        }

        #endregion

        #region 选课统计

        /// <summary>
        /// 选课统计（按课程班次统计已选/容量）
        /// </summary>
        public async Task<List<ElectiveStatModel>> GetElectiveStat(string roundId)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT r.F_Name AS F_RoundName, c.F_Name AS F_CourseName, u.F_RealName AS F_TeacherName,
	cr.F_Name AS F_ClassroomName, ec.F_WeekDay, ec.F_Section, ec.F_Capacity,
	(SELECT COUNT(1) FROM school_elective_record er
		WHERE er.F_ElectiveCourseId COLLATE Chinese_PRC_CI_AS = ec.F_Id
		AND ISNULL(er.F_Status,1) = 1 AND ISNULL(er.F_DeleteMark,0) = 0) AS F_SelectedCount
FROM school_elective_course ec
LEFT JOIN school_elective_round r ON r.F_Id COLLATE Chinese_PRC_CI_AS = ec.F_RoundId COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON c.F_Id COLLATE Chinese_PRC_CI_AS = ec.F_CourseId COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON u.F_Id COLLATE Chinese_PRC_CI_AS = ec.F_TeacherId COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON cr.F_Id COLLATE Chinese_PRC_CI_AS = ec.F_ClassroomId COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(ec.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(roundId))
            {
                sb.Append(" AND ec.F_RoundId = '" + Q(roundId) + "'");
            }
            sb.Append(" ORDER BY r.F_Name, c.F_Name, u.F_RealName");
            return await repository.SqlQueryable<ElectiveStatModel>(sb.ToString()).ToListAsync();
        }

        #endregion

        #region 教师工作量统计

        /// <summary>
        /// 教师工作量（课时按学期统计，监考次数累计统计）
        /// </summary>
        public async Task<List<WorkloadStatModel>> GetWorkloadStat(string semester)
        {
            //全部教师（排除学生角色）
            var teachers = await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && (a.F_RoleId == null || !a.F_RoleId.Contains(StudentRoleId)))
                .OrderBy(a => a.F_Account)
                .ToListAsync();

            //课时（课表格数 + 课程门数）
            var lessonSb = new StringBuilder();
            lessonSb.Append(@"SELECT t.F_TeacherId, COUNT(DISTINCT t.F_CourseId) AS F_CourseCount, COUNT(1) AS F_LessonCount
FROM school_timetable t
WHERE ISNULL(t.F_DeleteMark,0) = 0 AND t.F_TeacherId IS NOT NULL");
            if (!string.IsNullOrWhiteSpace(semester))
            {
                lessonSb.Append(" AND t.F_Semester = '" + Q(semester) + "'");
            }
            lessonSb.Append(" GROUP BY t.F_TeacherId");
            var lessons = await repository.SqlQueryable<WorkloadStatModel>(lessonSb.ToString()).ToListAsync();
            var lessonMap = lessons.Where(a => !string.IsNullOrEmpty(a.F_TeacherId))
                .ToDictionary(a => a.F_TeacherId, a => a, StringComparer.OrdinalIgnoreCase);

            //监考次数（考务+补考+清考，主/副监考累计）
            var invSql = @"SELECT tid AS F_TeacherId, COUNT(1) AS F_Count FROM (
	SELECT F_MainInvigilatorId COLLATE Chinese_PRC_CI_AS AS tid FROM school_exam_arrange WHERE ISNULL(F_DeleteMark,0)=0 AND F_MainInvigilatorId IS NOT NULL
	UNION ALL SELECT F_SubInvigilatorId COLLATE Chinese_PRC_CI_AS FROM school_exam_arrange WHERE ISNULL(F_DeleteMark,0)=0 AND F_SubInvigilatorId IS NOT NULL
	UNION ALL SELECT F_MainInvigilatorId COLLATE Chinese_PRC_CI_AS FROM school_makeup_exam WHERE ISNULL(F_DeleteMark,0)=0 AND F_MainInvigilatorId IS NOT NULL
	UNION ALL SELECT F_SubInvigilatorId COLLATE Chinese_PRC_CI_AS FROM school_makeup_exam WHERE ISNULL(F_DeleteMark,0)=0 AND F_SubInvigilatorId IS NOT NULL
	UNION ALL SELECT F_MainInvigilatorId COLLATE Chinese_PRC_CI_AS FROM school_clear_exam WHERE ISNULL(F_DeleteMark,0)=0 AND F_MainInvigilatorId IS NOT NULL
	UNION ALL SELECT F_SubInvigilatorId COLLATE Chinese_PRC_CI_AS FROM school_clear_exam WHERE ISNULL(F_DeleteMark,0)=0 AND F_SubInvigilatorId IS NOT NULL
) x GROUP BY tid";
            var invList = await repository.SqlQueryable<InvigilateCountModel>(invSql).ToListAsync();
            var invMap = invList.Where(a => !string.IsNullOrEmpty(a.F_TeacherId))
                .ToDictionary(a => a.F_TeacherId, a => a.F_Count, StringComparer.OrdinalIgnoreCase);

            var result = new List<WorkloadStatModel>();
            foreach (var t in teachers)
            {
                var row = new WorkloadStatModel
                {
                    F_TeacherId = t.F_Id,
                    F_TeacherName = t.F_RealName,
                    F_Account = t.F_Account
                };
                if (lessonMap.TryGetValue(t.F_Id, out var lm))
                {
                    row.F_CourseCount = lm.F_CourseCount;
                    row.F_LessonCount = lm.F_LessonCount;
                }
                if (invMap.TryGetValue(t.F_Id, out var cnt))
                {
                    row.F_InvigilateCount = cnt;
                }
                result.Add(row);
            }
            return result.OrderByDescending(a => a.F_LessonCount).ToList();
        }

        #endregion

        #region 补考清考统计

        /// <summary>
        /// 补考清考统计（按学期+课程分组，学期取原成绩记录的学期）
        /// </summary>
        public async Task<List<ExamStatModel>> GetExamStat(string semester)
        {
            var makeupSb = new StringBuilder();
            makeupSb.Append(@"SELECT s.F_Semester, c.F_Name AS F_CourseName,
	COUNT(1) AS F_MakeupCount,
	SUM(CASE WHEN m.F_NewScore >= 60 THEN 1 ELSE 0 END) AS F_MakeupPassCount
FROM school_makeup_exam m
LEFT JOIN school_scores s ON s.F_Id COLLATE Chinese_PRC_CI_AS = m.F_ScoreId COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON c.F_Id COLLATE Chinese_PRC_CI_AS = m.F_CourseId COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(m.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(semester))
            {
                makeupSb.Append(" AND s.F_Semester = '" + Q(semester) + "'");
            }
            makeupSb.Append(" GROUP BY s.F_Semester, c.F_Name");
            var makeupList = await repository.SqlQueryable<ExamStatModel>(makeupSb.ToString()).ToListAsync();

            var clearSb = new StringBuilder();
            clearSb.Append(@"SELECT s.F_Semester, c.F_Name AS F_CourseName,
	COUNT(1) AS F_ClearCount,
	SUM(CASE WHEN m.F_NewScore >= 60 THEN 1 ELSE 0 END) AS F_ClearPassCount
FROM school_clear_exam m
LEFT JOIN school_scores s ON s.F_Id COLLATE Chinese_PRC_CI_AS = m.F_ScoreId COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON c.F_Id COLLATE Chinese_PRC_CI_AS = m.F_CourseId COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(m.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(semester))
            {
                clearSb.Append(" AND s.F_Semester = '" + Q(semester) + "'");
            }
            clearSb.Append(" GROUP BY s.F_Semester, c.F_Name");
            var clearList = await repository.SqlQueryable<ExamStatModel>(clearSb.ToString()).ToListAsync();

            //按 学期+课程 合并
            var map = new Dictionary<string, ExamStatModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in makeupList)
            {
                var key = (m.F_Semester ?? "") + "|" + (m.F_CourseName ?? "");
                map[key] = m;
            }
            foreach (var c in clearList)
            {
                var key = (c.F_Semester ?? "") + "|" + (c.F_CourseName ?? "");
                if (map.TryGetValue(key, out var row))
                {
                    row.F_ClearCount = c.F_ClearCount;
                    row.F_ClearPassCount = c.F_ClearPassCount;
                }
                else
                {
                    map[key] = c;
                }
            }
            return map.Values.OrderByDescending(a => a.F_Semester).ThenBy(a => a.F_CourseName).ToList();
        }

        #endregion

        #region 下拉数据

        /// <summary>
        /// 学期下拉（成绩/课表/选课批次中出现过的学期）
        /// </summary>
        public async Task<List<SemesterItem>> GetSemesterList()
        {
            var sql = @"SELECT DISTINCT sem AS F_Semester FROM (
	SELECT F_Semester COLLATE Chinese_PRC_CI_AS AS sem FROM school_scores WHERE ISNULL(F_DeleteMark,0)=0 AND F_Semester IS NOT NULL AND F_Semester <> ''
	UNION
	SELECT F_Semester COLLATE Chinese_PRC_CI_AS FROM school_timetable WHERE ISNULL(F_DeleteMark,0)=0 AND F_Semester IS NOT NULL AND F_Semester <> ''
	UNION
	SELECT F_Semester COLLATE Chinese_PRC_CI_AS FROM school_elective_round WHERE ISNULL(F_DeleteMark,0)=0 AND F_Semester IS NOT NULL AND F_Semester <> ''
) x ORDER BY sem DESC";
            return await repository.SqlQueryable<SemesterItem>(sql).ToListAsync();
        }

        /// <summary>
        /// 课程下拉
        /// </summary>
        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        /// <summary>
        /// 班级下拉
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true && a.F_CategoryId == "banji")
                .OrderBy(a => a.F_FullName)
                .ToListAsync();
        }

        /// <summary>
        /// 选课批次下拉
        /// </summary>
        public async Task<List<ElectiveRoundEntity>> GetRoundList()
        {
            return await repository.Db.Queryable<ElectiveRoundEntity>()
                .Where(a => a.F_DeleteMark == false)
                .OrderBy(a => a.F_CreatorTime, OrderByType.Desc)
                .ToListAsync();
        }

        #endregion

        /// <summary>
        /// 转义单引号防注入
        /// </summary>
        private string Q(string value)
        {
            return (value ?? "").Replace("'", "''");
        }
    }
}
