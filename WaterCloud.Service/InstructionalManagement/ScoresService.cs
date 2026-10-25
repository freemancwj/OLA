using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemManage;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 成绩管理服务类
    /// </summary>
    public class ScoresService : BaseService<ScoresEntity>, IDenpendency
    {
        //课程字典
        private const string CourseItemId = "08dd9f77-fcba-446c-8a54-fb2b95717c33";
        //成绩类型字典
        private const string ScoreTypeItemId = "08dde7aa-d604-4397-82ad-902671433107";

        private ScoresRoleInfo _roleInfo;

        /// <summary>
        /// 任课班级服务（按教师带班过滤数据范围）
        /// </summary>
        public TeacherClassService teacherClassApp { get; set; }

        public ScoresService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 当前登录用户的成绩管理角色
        /// </summary>
        public ScoresRoleInfo RoleInfo
        {
            get
            {
                if (_roleInfo != null)
                {
                    return _roleInfo;
                }
                var names = new List<string>();
                if (!string.IsNullOrEmpty(currentuser.RoleId))
                {
                    var ids = currentuser.RoleId.Split(',');
                    names = repository.Db.Queryable<RoleEntity>()
                        .Where(a => ids.Contains(a.F_Id))
                        .Select(a => a.F_FullName)
                        .ToList();
                }
                var isAdmin = currentuser.IsAdmin || names.Any(a => a.Contains("管理员"));
                var isStudent = names.Any(a => a == "学生");
                _roleInfo = new ScoresRoleInfo
                {
                    UserId = currentuser.UserId,
                    UserName = currentuser.UserName,
                    IsAdmin = isAdmin,
                    IsStudent = isStudent,
                    CanEntry = !isStudent,
                    CanAudit = isAdmin || names.Any(a => a == "校长")
                };
                return _roleInfo;
            }
        }

        #region 获取数据

        /// <summary>
        /// 成绩列表（分页）
        /// </summary>
        public async Task<List<ScoresExtend>> GetLookList(SoulPage<ScoresExtend> pagination, string keyword, string classId = "", string courseId = "", string semester = "", string scoreType = "", string auditStatus = "", string minScore = "", string maxScore = "")
        {
            var allowedFields = new HashSet<string>
            {
                "F_RealName", "F_Account", "F_ClassName", "F_CourseName", "F_ScoreTypeName", "F_Score",
                "F_ScoreLevel", "F_ExamTime", "F_Semester", "F_CreatorTime", "F_AuditTime", "F_AuditStatus"
            };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_ExamTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var sql = BuildQuerySql(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore, visibleClassIds);
            var query = repository.SqlQueryable<ScoresExtend>(sql);
            return await query.ToPageListAsync(pagination);
        }

        /// <summary>
        /// 成绩列表（不分页，用于导出和统计）
        /// </summary>
        public async Task<List<ScoresExtend>> GetLookListAll(string keyword, string classId = "", string courseId = "", string semester = "", string scoreType = "", string auditStatus = "", string minScore = "", string maxScore = "")
        {
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var sql = BuildQuerySql(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore, visibleClassIds);
            return await repository.SqlQueryable<ScoresExtend>(sql).ToListAsync();
        }

        private string BuildQuerySql(string keyword, string classId, string courseId, string semester, string scoreType, string auditStatus, string minScore, string maxScore, List<string> visibleClassIds)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.F_Id, a.F_StudentId, a.F_ClassId, a.F_CourseId, a.F_Score, a.F_ExamTime, a.F_Semester,
       a.F_ScoreType, a.F_ScoreLevel, a.F_Remak, a.F_EnabledMark, a.F_DeleteMark,
       a.F_CreatorTime, a.F_CreatorUserId, a.F_LastModifyTime, a.F_LastModifyUserId,
       a.F_DeleteTime, a.F_DeleteUserId, a.F_AuditStatus, a.F_AuditUserId, a.F_AuditTime, a.F_AuditRemark,
       b.F_RealName, b.F_Account,
       o.F_FullName AS F_ClassName,
       c.F_ItemName AS F_CourseName,
       st.F_ItemName AS F_ScoreTypeName,
       cu.F_RealName AS F_CreatorUserName,
       au.F_RealName AS F_AuditUserName
FROM school_scores a
LEFT JOIN sys_user b ON a.F_StudentId = b.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail st ON a.F_ScoreType = st.F_ItemCode COLLATE Chinese_PRC_CI_AS AND st.F_ItemId = '" + ScoreTypeItemId + @"'
LEFT JOIN sys_user cu ON a.F_CreatorUserId = cu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user au ON a.F_AuditUserId = au.F_Id COLLATE Chinese_PRC_CI_AS
WHERE a.F_DeleteMark = 0");

            //学生只能看到自己审核通过的成绩
            if (RoleInfo.IsStudent)
            {
                sb.Append(" AND a.F_StudentId = '" + Q(currentuser.UserId) + "' AND ISNULL(a.F_AuditStatus,0) = 1");
            }
            else if (!string.IsNullOrEmpty(auditStatus) && int.TryParse(auditStatus, out var audit))
            {
                sb.Append(" AND ISNULL(a.F_AuditStatus,0) = " + audit);
            }
            //按教师任课班级限制数据范围（null 不限制，空列表表示无任何可见班级）
            if (visibleClassIds != null)
            {
                if (visibleClassIds.Count == 0)
                {
                    sb.Append(" AND 1 = 0");
                }
                else
                {
                    sb.Append(" AND a.F_ClassId IN ('" + string.Join("','", visibleClassIds.Select(Q)) + "')");
                }
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (b.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR b.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR a.F_Semester COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(semester))
            {
                sb.Append(" AND a.F_Semester COLLATE Chinese_PRC_CI_AS LIKE N'%" + Q(semester.Trim()) + "%'");
            }
            if (!string.IsNullOrEmpty(scoreType))
            {
                sb.Append(" AND a.F_ScoreType = '" + Q(scoreType) + "'");
            }
            if (decimal.TryParse(minScore, NumberStyles.Any, CultureInfo.InvariantCulture, out var min))
            {
                sb.Append(" AND a.F_Score >= " + min.ToString(CultureInfo.InvariantCulture));
            }
            if (decimal.TryParse(maxScore, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
            {
                sb.Append(" AND a.F_Score <= " + max.ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        public async Task<ScoresEntity> GetForm(string keyValue)
        {
            var data = await repository.FindEntity(keyValue);
            if (data != null && RoleInfo.IsStudent && data.F_StudentId != currentuser.UserId)
            {
                throw new Exception("只能查看自己的成绩！");
            }
            //非管理员只能查看任课班级的成绩
            if (data != null && !RoleInfo.IsStudent)
            {
                var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
                if (visibleClassIds != null && (string.IsNullOrEmpty(data.F_ClassId) || !visibleClassIds.Contains(data.F_ClassId)))
                {
                    throw new Exception("只能查看任课班级的成绩！");
                }
            }
            return data;
        }

        public async Task<ScoresEntity> GetLookForm(string keyValue)
        {
            var data = await GetForm(keyValue);
            return GetFieldsFilterData(data);
        }

        /// <summary>
        /// 班级列表（非管理员仅返回任课/本班班级）
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var query = repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true);
            if (visibleClassIds != null)
            {
                query = query.Where(a => visibleClassIds.Contains(a.F_Id));
            }
            return await query.OrderBy(a => a.F_EnCode).ToListAsync();
        }

        /// <summary>
        /// 课程列表
        /// </summary>
        public async Task<List<ItemsDetailEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_ItemId == CourseItemId && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        /// <summary>
        /// 成绩类型列表
        /// </summary>
        public async Task<List<ItemsDetailEntity>> GetScoreTypeList()
        {
            return await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_ItemId == ScoreTypeItemId && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        private string GetStudentRoleId()
        {
            return repository.Db.Queryable<RoleEntity>()
                .Where(a => a.F_FullName == "学生" && a.F_EnabledMark == true)
                .Select(a => a.F_Id)
                .First();
        }

        /// <summary>
        /// 学生列表，可按班级过滤
        /// </summary>
        public async Task<List<ScoresBatchStudent>> GetStudentList(string classId)
        {
            var studentRoleId = GetStudentRoleId();
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true);
            if (!string.IsNullOrEmpty(studentRoleId))
            {
                query = query.Where(a => a.F_RoleId != null && a.F_RoleId.Contains(studentRoleId));
            }
            if (!string.IsNullOrEmpty(classId))
            {
                //非管理员指定班级时，该班必须在可见范围内，防止遍历班级越权
                if (visibleClassIds != null && !visibleClassIds.Contains(classId))
                {
                    return new List<ScoresBatchStudent>();
                }
                query = query.Where(a => a.F_OrganizeId != null && (a.F_OrganizeId == classId || a.F_OrganizeId.Contains(classId)));
            }
            else if (visibleClassIds != null)
            {
                //未指定班级时，非管理员只能看到任课/本班学生
                if (visibleClassIds.Count == 0)
                {
                    return new List<ScoresBatchStudent>();
                }
                query = query.Where(a => a.F_OrganizeId != null && visibleClassIds.Contains(a.F_OrganizeId));
            }
            var users = await query.OrderBy(a => a.F_Account).ToListAsync();
            var classList = await GetClassList();
            var classMap = classList.GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_FullName);
            var list = new List<ScoresBatchStudent>();
            foreach (var user in users)
            {
                var orgId = (user.F_OrganizeId ?? string.Empty).Split(',').Select(a => a.Trim()).FirstOrDefault(a => classMap.ContainsKey(a));
                list.Add(new ScoresBatchStudent
                {
                    F_StudentId = user.F_Id,
                    F_Account = user.F_Account,
                    F_RealName = user.F_RealName,
                    F_ClassName = orgId == null ? "" : classMap[orgId]
                });
            }
            return list;
        }

        /// <summary>
        /// 批量录入页面数据，带出已录入成绩
        /// </summary>
        public async Task<List<ScoresBatchStudent>> GetBatchStudents(string classId, string courseId = "", string scoreType = "", string semester = "")
        {
            var students = await GetStudentList(classId);
            if (students.Count == 0 || string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(scoreType) || string.IsNullOrWhiteSpace(semester))
            {
                return students;
            }
            var studentIds = students.Select(a => a.F_StudentId).ToList();
            var existList = await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_CourseId == courseId && a.F_ScoreType == scoreType
                    && a.F_Semester == semester && studentIds.Contains(a.F_StudentId))
                .ToListAsync();
            var scoreMap = existList.GroupBy(a => a.F_StudentId).ToDictionary(a => a.Key, a => a.First());
            foreach (var student in students)
            {
                if (student.F_StudentId != null && scoreMap.ContainsKey(student.F_StudentId))
                {
                    var score = scoreMap[student.F_StudentId];
                    student.F_Score = score.F_Score;
                    student.F_Remak = score.F_Remak;
                    student.F_ScoreId = score.F_Id;
                    student.F_AuditStatus = score.F_AuditStatus;
                }
            }
            return students;
        }

        /// <summary>
        /// 成绩概览
        /// </summary>
        public async Task<ScoresStatSummary> GetSummary(string keyword, string classId = "", string courseId = "", string semester = "", string scoreType = "", string auditStatus = "", string minScore = "", string maxScore = "")
        {
            var data = await GetLookListAll(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            return BuildSummary(data);
        }

        /// <summary>
        /// 成绩统计分析
        /// </summary>
        public async Task<ScoresStatisticsResult> GetStatistics(string keyword, string classId = "", string courseId = "", string semester = "", string scoreType = "", string auditStatus = "", string minScore = "", string maxScore = "")
        {
            var data = await GetLookListAll(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            var scored = data.Where(a => a.F_Score.HasValue).ToList();
            var result = new ScoresStatisticsResult
            {
                Summary = BuildSummary(data),
                CourseStats = BuildGroupStats(scored, a => string.IsNullOrEmpty(a.F_CourseName) ? "未设置课程" : a.F_CourseName),
                ClassStats = BuildGroupStats(scored, a => string.IsNullOrEmpty(a.F_ClassName) ? "未分配班级" : a.F_ClassName),
                ScoreDistribution = BuildScoreDistribution(scored),
                LevelDistribution = scored.GroupBy(a => string.IsNullOrEmpty(a.F_ScoreLevel) ? "未评定" : a.F_ScoreLevel)
                    .Select(a => new ScoresStatDistribution { Name = a.Key, Count = a.Count() })
                    .OrderByDescending(a => a.Count).ToList(),
                SemesterStats = BuildGroupStats(scored, a => string.IsNullOrEmpty(a.F_Semester) ? "未设置学期" : a.F_Semester)
            };
            result.SemesterStats = result.SemesterStats.OrderBy(a => a.Name).ToList();
            return result;
        }

        private ScoresStatSummary BuildSummary(List<ScoresExtend> data)
        {
            var scored = data.Where(a => a.F_Score.HasValue).ToList();
            var summary = new ScoresStatSummary { Total = data.Count, ScoredCount = scored.Count };
            if (scored.Count > 0)
            {
                summary.AvgScore = Math.Round(scored.Average(a => a.F_Score.Value), 2);
                summary.MaxScore = scored.Max(a => a.F_Score.Value);
                summary.MinScore = scored.Min(a => a.F_Score.Value);
                summary.PassCount = scored.Count(a => a.F_Score.Value >= 60);
                summary.ExcellentCount = scored.Count(a => a.F_Score.Value >= 90);
                summary.PassRate = Math.Round(summary.PassCount * 100m / scored.Count, 2);
                summary.ExcellentRate = Math.Round(summary.ExcellentCount * 100m / scored.Count, 2);
            }
            return summary;
        }

        private List<ScoresStatItem> BuildGroupStats(List<ScoresExtend> scored, Func<ScoresExtend, string> keySelector)
        {
            return scored.GroupBy(keySelector)
                .Select(g => new ScoresStatItem
                {
                    Name = g.Key,
                    Count = g.Count(),
                    AvgScore = Math.Round(g.Average(a => a.F_Score.Value), 2),
                    MaxScore = g.Max(a => a.F_Score.Value),
                    MinScore = g.Min(a => a.F_Score.Value),
                    PassRate = Math.Round(g.Count(a => a.F_Score.Value >= 60) * 100m / g.Count(), 2)
                })
                .OrderByDescending(a => a.AvgScore).ToList();
        }

        private List<ScoresStatDistribution> BuildScoreDistribution(List<ScoresExtend> scored)
        {
            var labels = new[] { "60分以下", "60-69分", "70-79分", "80-89分", "90-99分", "100分以上" };
            var counts = new int[labels.Length];
            foreach (var item in scored)
            {
                var score = item.F_Score.Value;
                if (score < 60) counts[0]++;
                else if (score < 70) counts[1]++;
                else if (score < 80) counts[2]++;
                else if (score < 90) counts[3]++;
                else if (score < 100) counts[4]++;
                else counts[5]++;
            }
            var list = new List<ScoresStatDistribution>();
            for (var i = 0; i < labels.Length; i++)
            {
                list.Add(new ScoresStatDistribution { Name = labels[i], Count = counts[i] });
            }
            return list;
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ScoresEntity entity, string keyValue)
        {
            if (!RoleInfo.CanEntry)
            {
                throw new Exception("学生不能录入或修改成绩！");
            }
            if (entity == null || string.IsNullOrEmpty(entity.F_StudentId))
            {
                throw new Exception("请选择学生！");
            }
            if (string.IsNullOrEmpty(entity.F_CourseId))
            {
                throw new Exception("请选择课程！");
            }
            if (string.IsNullOrEmpty(entity.F_ScoreType))
            {
                throw new Exception("请选择成绩类型！");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Semester))
            {
                throw new Exception("请填写学期！");
            }
            if (!entity.F_Score.HasValue)
            {
                throw new Exception("请填写分数！");
            }
            if (entity.F_Score.Value < 0 || entity.F_Score.Value > 150)
            {
                throw new Exception("分数必须在0到150之间！");
            }
            var student = await repository.Db.Queryable<UserEntity>().Where(a => a.F_Id == entity.F_StudentId).FirstAsync();
            if (student == null)
            {
                throw new Exception("学生不存在！");
            }
            entity.F_ClassId = await GetStudentClassId(student);
            //非管理员只能给任课班级学生录入成绩
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            if (visibleClassIds != null && (string.IsNullOrEmpty(entity.F_ClassId) || !visibleClassIds.Contains(entity.F_ClassId)))
            {
                throw new Exception("只能给任课班级的学生录入成绩！");
            }
            entity.F_ScoreLevel = CalculateScoreLevel(entity.F_Score.Value);
            entity.F_ExamTime = entity.F_ExamTime ?? DateTime.Now;
            entity.F_Semester = entity.F_Semester.Trim();

            if (string.IsNullOrEmpty(keyValue))
            {
                var exist = await repository.IQueryable()
                    .Where(a => a.F_DeleteMark == false && a.F_StudentId == entity.F_StudentId && a.F_CourseId == entity.F_CourseId
                        && a.F_ScoreType == entity.F_ScoreType && a.F_Semester == entity.F_Semester)
                    .FirstAsync();
                if (exist != null)
                {
                    throw new Exception("该学生该课程该考试类型已存在成绩，请直接修改！");
                }
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.F_AuditStatus = 0;
                entity.F_AuditUserId = null;
                entity.F_AuditTime = null;
                entity.F_AuditRemark = null;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                var existing = await repository.FindEntity(keyValue);
                if (existing == null || existing.F_DeleteMark == true)
                {
                    throw new Exception("成绩记录不存在！");
                }
                if (!RoleInfo.IsAdmin && existing.F_CreatorUserId != currentuser.UserId)
                {
                    throw new Exception("只能修改自己录入的成绩！");
                }
                if (!RoleInfo.IsAdmin && existing.F_AuditStatus == 1)
                {
                    throw new Exception("成绩已审核通过，不能修改！");
                }
                var duplicate = await repository.IQueryable()
                    .Where(a => a.F_DeleteMark == false && a.F_Id != keyValue && a.F_StudentId == entity.F_StudentId
                        && a.F_CourseId == entity.F_CourseId && a.F_ScoreType == entity.F_ScoreType && a.F_Semester == entity.F_Semester)
                    .FirstAsync();
                if (duplicate != null)
                {
                    throw new Exception("该学生该课程该考试类型已存在成绩，请勿重复录入！");
                }
                existing.F_StudentId = entity.F_StudentId;
                existing.F_ClassId = entity.F_ClassId;
                existing.F_CourseId = entity.F_CourseId;
                existing.F_Score = entity.F_Score;
                existing.F_ExamTime = entity.F_ExamTime;
                existing.F_Semester = entity.F_Semester;
                existing.F_ScoreType = entity.F_ScoreType;
                existing.F_ScoreLevel = entity.F_ScoreLevel;
                existing.F_Remak = entity.F_Remak;
                if (!RoleInfo.IsAdmin)
                {
                    existing.F_AuditStatus = 0;
                    existing.F_AuditUserId = null;
                    existing.F_AuditTime = null;
                    existing.F_AuditRemark = null;
                }
                existing.Modify(keyValue);
                await repository.Update(existing);
            }
        }

        /// <summary>
        /// 批量录入成绩
        /// </summary>
        public async Task<int> BatchSubmitForm(string rowsJson, string classId, string courseId, string scoreType, string semester, string examTime)
        {
            if (!RoleInfo.CanEntry)
            {
                throw new Exception("学生不能录入成绩！");
            }
            if (string.IsNullOrEmpty(classId))
            {
                throw new Exception("请选择班级！");
            }
            if (string.IsNullOrEmpty(courseId))
            {
                throw new Exception("请选择课程！");
            }
            if (string.IsNullOrEmpty(scoreType))
            {
                throw new Exception("请选择成绩类型！");
            }
            if (string.IsNullOrWhiteSpace(semester))
            {
                throw new Exception("请填写学期！");
            }
            var rows = string.IsNullOrEmpty(rowsJson) ? new List<ScoresBatchRow>() : rowsJson.ToObject<List<ScoresBatchRow>>();
            if (rows == null || rows.Count == 0)
            {
                throw new Exception("没有可保存的成绩！");
            }
            var students = await GetStudentList(classId);
            var studentIds = students.Select(a => a.F_StudentId).ToList();
            var validRows = rows.Where(a => a.F_StudentId != null && a.F_Score.HasValue).ToList();
            if (validRows.Count == 0)
            {
                throw new Exception("请至少录入一名学生的成绩！");
            }
            foreach (var row in validRows)
            {
                if (!studentIds.Contains(row.F_StudentId))
                {
                    throw new Exception("存在不属于所选班级的学生，请刷新后重试！");
                }
                if (row.F_Score.Value < 0 || row.F_Score.Value > 150)
                {
                    throw new Exception("分数必须在0到150之间！");
                }
            }
            DateTime? exam = null;
            if (!string.IsNullOrEmpty(examTime) && DateTime.TryParse(examTime, out var examValue))
            {
                exam = examValue;
            }
            var existList = await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_CourseId == courseId && a.F_ScoreType == scoreType
                    && a.F_Semester == semester && studentIds.Contains(a.F_StudentId))
                .ToListAsync();
            var existMap = existList.GroupBy(a => a.F_StudentId).ToDictionary(a => a.Key, a => a.First());
            var count = 0;
            repository.Db.Ado.BeginTran();
            try
            {
                foreach (var row in validRows)
                {
                    if (existMap.ContainsKey(row.F_StudentId))
                    {
                        var existing = existMap[row.F_StudentId];
                        if (!RoleInfo.IsAdmin && existing.F_CreatorUserId != currentuser.UserId)
                        {
                            throw new Exception("存在他人录入的成绩，不能批量修改！");
                        }
                        if (!RoleInfo.IsAdmin && existing.F_AuditStatus == 1)
                        {
                            throw new Exception("存在已审核通过的成绩，不能批量修改！");
                        }
                        existing.F_Score = row.F_Score;
                        existing.F_ScoreLevel = CalculateScoreLevel(row.F_Score.Value);
                        existing.F_Remak = row.F_Remak;
                        existing.F_ExamTime = exam ?? existing.F_ExamTime ?? DateTime.Now;
                        existing.F_ClassId = classId;
                        if (!RoleInfo.IsAdmin)
                        {
                            existing.F_AuditStatus = 0;
                            existing.F_AuditUserId = null;
                            existing.F_AuditTime = null;
                            existing.F_AuditRemark = null;
                        }
                        existing.Modify(existing.F_Id);
                        await repository.Update(existing);
                    }
                    else
                    {
                        var entity = new ScoresEntity
                        {
                            F_StudentId = row.F_StudentId,
                            F_ClassId = classId,
                            F_CourseId = courseId,
                            F_Score = row.F_Score,
                            F_ScoreLevel = CalculateScoreLevel(row.F_Score.Value),
                            F_ExamTime = exam ?? DateTime.Now,
                            F_Semester = semester.Trim(),
                            F_ScoreType = scoreType,
                            F_Remak = row.F_Remak,
                            F_EnabledMark = true,
                            F_DeleteMark = false,
                            F_AuditStatus = 0
                        };
                        entity.Create();
                        await repository.Insert(entity);
                        existMap[row.F_StudentId] = entity;
                    }
                    count++;
                }
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
            return count;
        }

        /// <summary>
        /// 成绩审核
        /// </summary>
        public async Task AuditForm(string keyValue, int auditStatus, string remark)
        {
            if (!RoleInfo.CanAudit)
            {
                throw new Exception("没有成绩审核权限！");
            }
            if (auditStatus != 1 && auditStatus != 2)
            {
                throw new Exception("审核状态不正确！");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("请选择需要审核的成绩！");
            }
            if (auditStatus == 2 && string.IsNullOrWhiteSpace(remark))
            {
                throw new Exception("退回时请填写审核意见！");
            }
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var count = await repository.Update(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false, a => new ScoresEntity
            {
                F_AuditStatus = auditStatus,
                F_AuditUserId = currentuser.UserId,
                F_AuditTime = DateTime.Now,
                F_AuditRemark = remark
            });
            if (count == 0)
            {
                throw new Exception("没有找到需要审核的成绩！");
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            if (!RoleInfo.CanEntry)
            {
                throw new Exception("没有删除权限！");
            }
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = await repository.IQueryable().Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false).ToListAsync();
            foreach (var item in list)
            {
                if (!RoleInfo.IsAdmin && item.F_CreatorUserId != currentuser.UserId)
                {
                    throw new Exception("只能删除自己录入的成绩！");
                }
                if (!RoleInfo.IsAdmin && item.F_AuditStatus == 1)
                {
                    throw new Exception("已审核通过的成绩不能删除！");
                }
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ScoresEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取学生所属班级（取第一个班级类型部门）
        /// </summary>
        private async Task<string> GetStudentClassId(UserEntity student)
        {
            if (student == null || string.IsNullOrEmpty(student.F_OrganizeId))
            {
                return null;
            }
            var orgIds = student.F_OrganizeId.Split(',').Select(a => a.Trim()).Where(a => a.Length > 0).ToList();
            if (orgIds.Count == 0)
            {
                return null;
            }
            var classList = await GetClassList();
            foreach (var orgId in orgIds)
            {
                var match = classList.FirstOrDefault(a => string.Equals(a.F_Id, orgId, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match.F_Id;
                }
            }
            return null;
        }

        /// <summary>
        /// 计算成绩等级
        /// </summary>
        private string CalculateScoreLevel(decimal score)
        {
            if (score >= 90)
                return "优秀";
            if (score >= 80)
                return "良好";
            if (score >= 70)
                return "中等";
            if (score >= 60)
                return "及格";
            return "不及格";
        }

        #endregion
    }
}
