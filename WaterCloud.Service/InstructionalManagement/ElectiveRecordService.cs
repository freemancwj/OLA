using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 选课记录服务（管理端名单 + 学生端选课/退课）
    /// </summary>
    public class ElectiveRecordService : BaseService<ElectiveRecordEntity>, IDenpendency
    {
        public ElectiveRecordService(ISqlSugarClient context) : base(context)
        {
        }

        #region 管理端名单

        public async Task<List<ElectiveRecordExtend>> GetLookList(SoulPage<ElectiveRecordExtend> pagination, string roundId, string electiveCourseId, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_StudentName", "F_Account", "F_CourseName", "F_SelectTime", "F_Status" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_SelectTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, u.F_RealName AS F_StudentName, u.F_Account, o.F_FullName AS F_ClassName,
       c.F_Name AS F_CourseName, tu.F_RealName AS F_TeacherName, r.F_Name AS F_RoundName
FROM school_elective_record a
LEFT JOIN sys_user u ON a.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON u.F_OrganizeId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_elective_course ec ON a.F_ElectiveCourseId = ec.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON ec.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user tu ON ec.F_TeacherId = tu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_elective_round r ON a.F_RoundId = r.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0 AND a.F_Status = 1");
            if (!string.IsNullOrEmpty(roundId))
            {
                sb.Append(" AND a.F_RoundId = '" + Q(roundId) + "'");
            }
            if (!string.IsNullOrEmpty(electiveCourseId))
            {
                sb.Append(" AND a.F_ElectiveCourseId = '" + Q(electiveCourseId) + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.SqlQueryable<ElectiveRecordExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 管理端移除学生选课（退课处理）
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ElectiveRecordEntity
            {
                F_Status = 0,
                F_DropTime = DateTime.Now,
                F_DeleteMark = false,
                F_LastModifyTime = DateTime.Now,
                F_LastModifyUserId = currentuser.UserId
            });
        }

        #endregion

        #region 学生端

        /// <summary>
        /// 当前开放批次
        /// </summary>
        public async Task<ElectiveRoundEntity> GetOpenRound()
        {
            var now = DateTime.Now;
            return await repository.Db.Queryable<ElectiveRoundEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && a.F_StartTime <= now && a.F_EndTime >= now)
                .OrderBy(a => a.F_StartTime, OrderByType.Desc)
                .FirstAsync();
        }

        /// <summary>
        /// 可选课程列表（含已选人数与本人是否已选）
        /// </summary>
        public async Task<List<ElectiveCourseExtend>> GetAvailableList(string roundId)
        {
            if (string.IsNullOrEmpty(roundId))
            {
                return new List<ElectiveCourseExtend>();
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, c.F_Name AS F_CourseName, u.F_RealName AS F_TeacherName, cr.F_Name AS F_ClassroomName,
       (SELECT COUNT(1) FROM school_elective_record x WHERE x.F_ElectiveCourseId = a.F_Id COLLATE Chinese_PRC_CI_AS
           AND x.F_Status = 1 AND ISNULL(x.F_DeleteMark,0) = 0) AS F_SelectedCount
FROM school_elective_course a
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_ClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0 AND ISNULL(a.F_EnabledMark,0) = 1 AND a.F_RoundId = '" + Q(roundId) + "'");
            var list = await repository.SqlQueryable<ElectiveCourseExtend>(sb.ToString())
                .OrderBy(a => a.F_CourseName).ToListAsync();
            //本人已选
            var myIds = (await repository.IQueryable()
                .Where(a => a.F_RoundId == roundId && a.F_StudentId == currentuser.UserId
                    && a.F_Status == 1 && a.F_DeleteMark == false)
                .Select(a => a.F_ElectiveCourseId).ToListAsync());
            foreach (var item in list)
            {
                item.F_IsSelected = myIds.Contains(item.F_Id);
            }
            return list;
        }

        /// <summary>
        /// 我的选课（某批次）
        /// </summary>
        public async Task<List<ElectiveCourseExtend>> GetMyList(string roundId)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, c.F_Name AS F_CourseName, u.F_RealName AS F_TeacherName, cr.F_Name AS F_ClassroomName,
       (SELECT COUNT(1) FROM school_elective_record x WHERE x.F_ElectiveCourseId = a.F_Id COLLATE Chinese_PRC_CI_AS
           AND x.F_Status = 1 AND ISNULL(x.F_DeleteMark,0) = 0) AS F_SelectedCount
FROM school_elective_course a
INNER JOIN school_elective_record rec ON rec.F_ElectiveCourseId = a.F_Id COLLATE Chinese_PRC_CI_AS
    AND rec.F_Status = 1 AND ISNULL(rec.F_DeleteMark,0) = 0 AND rec.F_StudentId = '" + Q(currentuser.UserId) + @"'
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_ClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrEmpty(roundId))
            {
                sb.Append(" AND a.F_RoundId = '" + Q(roundId) + "'");
            }
            var list = await repository.SqlQueryable<ElectiveCourseExtend>(sb.ToString())
                .OrderBy(a => a.F_CourseName).ToListAsync();
            foreach (var item in list)
            {
                item.F_IsSelected = true;
            }
            return list;
        }

        /// <summary>
        /// 选课
        /// </summary>
        public async Task SelectCourse(string electiveCourseId)
        {
            var round = await GetOpenRound();
            if (round == null)
            {
                throw new Exception("当前不在选课时间内");
            }
            var course = await repository.Db.Queryable<ElectiveCourseEntity>()
                .Where(a => a.F_Id == electiveCourseId && a.F_RoundId == round.F_Id
                    && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .FirstAsync();
            if (course == null)
            {
                throw new Exception("该课程不在开放选课范围内");
            }
            //重复选课校验
            var dup = await repository.IQueryable()
                .Where(a => a.F_ElectiveCourseId == electiveCourseId && a.F_StudentId == currentuser.UserId
                    && a.F_Status == 1 && a.F_DeleteMark == false)
                .AnyAsync();
            if (dup)
            {
                throw new Exception("您已选过该课程");
            }
            //同课程其他班次校验
            var sameCourse = await repository.Db.Queryable<ElectiveCourseEntity>()
                .Where(a => a.F_RoundId == round.F_Id && a.F_CourseId == course.F_CourseId && a.F_DeleteMark == false)
                .Select(a => a.F_Id).ToListAsync();
            var dupCourse = await repository.IQueryable()
                .Where(a => sameCourse.Contains(a.F_ElectiveCourseId) && a.F_StudentId == currentuser.UserId
                    && a.F_Status == 1 && a.F_DeleteMark == false)
                .AnyAsync();
            if (dupCourse)
            {
                throw new Exception("您已选过该课程的其他班次");
            }
            //容量校验
            var selected = await repository.IQueryable()
                .Where(a => a.F_ElectiveCourseId == electiveCourseId && a.F_Status == 1 && a.F_DeleteMark == false)
                .CountAsync();
            if (selected >= (course.F_Capacity ?? 0))
            {
                throw new Exception("该课程名额已满");
            }
            //时间冲突校验（与本人已选课程比星期+节次）
            if (course.F_WeekDay != null && course.F_Section != null)
            {
                var myRecords = await repository.IQueryable()
                    .Where(a => a.F_RoundId == round.F_Id && a.F_StudentId == currentuser.UserId
                        && a.F_Status == 1 && a.F_DeleteMark == false)
                    .Select(a => a.F_ElectiveCourseId).ToListAsync();
                if (myRecords.Count > 0)
                {
                    var timeClash = await repository.Db.Queryable<ElectiveCourseEntity>()
                        .Where(a => myRecords.Contains(a.F_Id) && a.F_WeekDay == course.F_WeekDay
                            && a.F_Section == course.F_Section && a.F_DeleteMark == false)
                        .AnyAsync();
                    if (timeClash)
                    {
                        throw new Exception("该课程与您已选课程上课时间冲突");
                    }
                }
            }
            var data = new ElectiveRecordEntity
            {
                F_RoundId = round.F_Id,
                F_ElectiveCourseId = electiveCourseId,
                F_StudentId = currentuser.UserId,
                F_Status = 1,
                F_SelectTime = DateTime.Now,
                F_DeleteMark = false,
                F_EnabledMark = true
            };
            data.Create();
            await repository.Insert(data);
        }

        /// <summary>
        /// 退课
        /// </summary>
        public async Task DropCourse(string electiveCourseId)
        {
            var round = await GetOpenRound();
            if (round == null)
            {
                throw new Exception("当前不在选课时间内，无法退课");
            }
            var record = await repository.IQueryable()
                .Where(a => a.F_ElectiveCourseId == electiveCourseId && a.F_StudentId == currentuser.UserId
                    && a.F_Status == 1 && a.F_DeleteMark == false)
                .FirstAsync();
            if (record == null)
            {
                throw new Exception("未找到选课记录");
            }
            record.F_Status = 0;
            record.F_DropTime = DateTime.Now;
            record.Modify(record.F_Id);
            await repository.Update(record);
        }

        #endregion

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
