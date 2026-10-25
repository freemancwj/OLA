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
    /// 课程服务
    /// </summary>
    public class CourseService : BaseService<CourseEntity>, IDenpendency
    {
        public CourseService(ISqlSugarClient context) : base(context)
        {
        }

        #region 列表

        public async Task<List<CourseExtend>> GetLookList(SoulPage<CourseExtend> pagination, string keyword, string categoryId, string status)
        {
            var allowed = new HashSet<string> { "F_Name", "F_CategoryName", "F_TeacherName", "F_Credit", "F_Status", "F_SortCode", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
            {
                pagination.field = "F_SortCode";
                pagination.order = "asc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "asc";
            }
            var sql = BuildQuerySql(keyword, categoryId, status, false);
            return await repository.Db.SqlQueryable<CourseExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildQuerySql(string keyword, string categoryId, string status, bool onlyOnline)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*,
       c.F_Name AS F_CategoryName,
       u.F_RealName AS F_TeacherName,
       m.F_FullName AS F_MajorName,
       (SELECT COUNT(1) FROM school_course_chapter x WHERE x.F_CourseId = a.F_Id AND ISNULL(x.F_DeleteMark,0)=0) AS ChapterCount,
       (SELECT COUNT(1) FROM school_course_lesson y WHERE y.F_CourseId = a.F_Id AND ISNULL(y.F_DeleteMark,0)=0) AS LessonCount
FROM school_course a
LEFT JOIN school_course_category c ON a.F_CategoryId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize m ON a.F_MajorId = m.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (onlyOnline)
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = 1");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                sb.Append(" AND (a.F_Name LIKE '%" + Q(keyword) + "%' OR c.F_Name LIKE '%" + Q(keyword) + "%' OR u.F_RealName LIKE '%" + Q(keyword) + "%')");
            }
            if (!string.IsNullOrEmpty(categoryId))
            {
                sb.Append(" AND a.F_CategoryId = '" + Q(categoryId) + "'");
            }
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + (status == "1" ? "1" : "0"));
            }
            return sb.ToString();
        }

        private static string Q(string s)
        {
            return (s ?? "").Replace("'", "''");
        }

        public async Task<CourseEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 学生端：已上架课程卡片
        /// </summary>
        public async Task<List<CourseCardModel>> GetStudyCards(string keyword, string categoryId)
        {
            var sql = BuildQuerySql(keyword, categoryId, "1", true);
            var list = await repository.Db.SqlQueryable<CourseExtend>(sql).ToListAsync();
            var userId = currentuser.UserId;
            var result = list.Select(a => new CourseCardModel
            {
                F_Id = a.F_Id,
                F_Name = a.F_Name,
                F_Cover = a.F_Cover,
                F_Intro = a.F_Intro,
                F_TeacherName = a.F_TeacherName,
                F_CategoryName = a.F_CategoryName,
                LessonCount = a.LessonCount,
                ProgressPercent = 0
            }).ToList();
            // 计算每个课程整体进度
            foreach (var item in result)
            {
                item.ProgressPercent = await GetCourseProgressPercent(item.F_Id, userId);
            }
            return result;
        }

        /// <summary>
        /// 学生端：课程学习详情（章节+课时+资料+进度）
        /// </summary>
        public async Task<CourseStudyInfo> GetStudyInfo(string courseId)
        {
            var course = await repository.FindEntity(courseId);
            if (course == null || course.F_DeleteMark == true || (course.F_Status ?? 0) != 1)
            {
                throw new Exception("课程不存在或未上架");
            }
            var userId = currentuser.UserId;
            var chapters = await repository.Db.Queryable<CourseChapterEntity>()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode).ToListAsync();
            var lessons = await repository.Db.Queryable<CourseLessonEntity>()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode).ToListAsync();
            var wares = await repository.Db.Queryable<CourseWareEntity>()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToListAsync();
            var progresses = await repository.Db.Queryable<CourseProgressEntity>()
                .Where(a => a.F_UserId == userId)
                .ToListAsync();
            var progressMap = progresses.ToDictionary(a => a.F_LessonId, a => a);

            var info = new CourseStudyInfo { Course = course, Wares = wares };
            int totalPercent = 0;
            int lessonTotal = lessons.Count;
            foreach (var ch in chapters)
            {
                var node = new CourseStudyChapter { F_Id = ch.F_Id, F_Name = ch.F_Name, F_SortCode = ch.F_SortCode };
                foreach (var ls in lessons.Where(a => a.F_ChapterId == ch.F_Id).OrderBy(a => a.F_SortCode))
                {
                    var item = new CourseStudyLesson
                    {
                        F_Id = ls.F_Id,
                        F_ChapterId = ls.F_ChapterId,
                        F_Name = ls.F_Name,
                        F_VideoUrl = ls.F_VideoUrl,
                        F_VideoDuration = ls.F_VideoDuration,
                        F_SortCode = ls.F_SortCode
                    };
                    if (progressMap.TryGetValue(ls.F_Id, out var p))
                    {
                        item.Position = p.F_Position ?? 0;
                        var dur = p.F_Duration ?? ls.F_VideoDuration ?? 0;
                        item.ProgressPercent = dur > 0 ? Math.Min(100, (int)Math.Round(item.Position * 100.0 / dur)) : 0;
                    }
                    totalPercent += item.ProgressPercent;
                    node.Lessons.Add(item);
                }
                info.Chapters.Add(node);
            }
            info.ProgressPercent = lessonTotal > 0 ? (int)Math.Round(totalPercent * 1.0 / lessonTotal) : 0;
            return info;
        }

        /// <summary>
        /// 某课程当前用户整体进度百分比
        /// </summary>
        private async Task<int> GetCourseProgressPercent(string courseId, string userId)
        {
            var lessons = await repository.Db.Queryable<CourseLessonEntity>()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .Select(a => new { a.F_Id, a.F_VideoDuration })
                .ToListAsync();
            if (lessons.Count == 0) return 0;
            var lessonIds = lessons.Select(a => a.F_Id).ToList();
            var progresses = await repository.Db.Queryable<CourseProgressEntity>()
                .Where(a => lessonIds.Contains(a.F_LessonId) && a.F_UserId == userId)
                .ToListAsync();
            var map = progresses.ToDictionary(a => a.F_LessonId, a => a);
            int sum = 0;
            foreach (var ls in lessons)
            {
                if (map.TryGetValue(ls.F_Id, out var p))
                {
                    var dur = p.F_Duration ?? ls.F_VideoDuration ?? 0;
                    sum += dur > 0 ? Math.Min(100, (int)Math.Round((p.F_Position ?? 0) * 100.0 / dur)) : 0;
                }
            }
            return (int)Math.Round(sum * 1.0 / lessons.Count);
        }

        #endregion

        #region 提交

        public async Task SubmitForm(CourseEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                if (entity.F_Status == null) entity.F_Status = 0;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            if (await repository.Db.Queryable<CourseChapterEntity>().AnyAsync(a => ids.Contains(a.F_CourseId) && a.F_DeleteMark == false))
            {
                throw new Exception("课程下存在章节，请先删除章节");
            }
            await repository.Delete(a => ids.Contains(a.F_Id));
        }

        /// <summary>
        /// 上架/下架
        /// </summary>
        public async Task SetStatus(string keyValue, int status)
        {
            var entity = await repository.FindEntity(keyValue);
            if (entity == null) throw new Exception("课程不存在");
            entity.F_Status = status;
            entity.F_LastModifyTime = DateTime.Now;
            entity.F_LastModifyUserId = currentuser.UserId;
            await repository.Update(entity);
        }

        /// <summary>
        /// 教师列表（授课教师下拉）
        /// </summary>
        public async Task<List<UserEntity>> GetTeacherList()
        {
            var teachers = await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .ToListAsync();
            var roles = await repository.Db.Queryable<RoleEntity>().ToListAsync();
            // 过滤出角色名包含"老师/教师/班主任/校长/管理员"的用户
            var teacherIds = teachers.Where(t =>
            {
                if (string.IsNullOrEmpty(t.F_RoleId)) return false;
                var rids = t.F_RoleId.Split(',');
                return roles.Where(r => rids.Contains(r.F_Id)).Any(r => r.F_FullName.Contains("老师") || r.F_FullName.Contains("教师") || r.F_FullName.Contains("班主任") || r.F_FullName.Contains("校长") || r.F_FullName.Contains("管理员"));
            }).Select(t => t.F_Id).ToList();
            return teachers.Where(a => teacherIds.Contains(a.F_Id)).OrderBy(a => a.F_Account).ToList();
        }

        #endregion
    }
}
