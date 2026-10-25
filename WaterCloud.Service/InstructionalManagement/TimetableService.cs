using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemManage;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 课表管理服务类（授课计划、自动排课、课表查询）
    /// </summary>
    public class TimetableService : BaseService<TimetablePlanEntity>, IDenpendency
    {
        //科目字典（与成绩管理一致）
        private const string CourseItemId = "08dd9f77-fcba-446c-8a54-fb2b95717c33";

        public TimetableService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 排课参数（无配置时默认5天/8节/同科2节）
        /// </summary>
        public async Task<ScheduleParamEntity> GetScheduleParam()
        {
            var data = await repository.Db.Queryable<ScheduleParamEntity>()
                .Where(a => a.F_DeleteMark == false).FirstAsync();
            if (data == null)
            {
                data = new ScheduleParamEntity { F_DaysPerWeek = 5, F_SectionsPerDay = 8, F_MaxSameCourseDaily = 2, F_MaxConsecutive = 2 };
            }
            return data;
        }

        /// <summary>
        /// 教室列表
        /// </summary>
        public async Task<List<ClassroomEntity>> GetClassroomList()
        {
            return await repository.Db.Queryable<ClassroomEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        #region 基础数据
        /// <summary>
        /// 班级列表
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_EnCode)
                .ToListAsync();
        }

        /// <summary>
        /// 科目列表（字典）
        /// </summary>
        public async Task<List<ItemsDetailEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_ItemId == CourseItemId && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        /// <summary>
        /// 教师列表（角色名含教师/老师/班主任/校长/管理员的用户）
        /// </summary>
        public async Task<List<UserEntity>> GetTeacherList()
        {
            var roleIds = await repository.Db.Queryable<RoleEntity>()
                .Where(a => a.F_EnabledMark == true && a.F_DeleteMark == false
                    && (a.F_FullName.Contains("老师") || a.F_FullName.Contains("教师") || a.F_FullName.Contains("班主任")
                        || a.F_FullName.Contains("校长") || a.F_FullName.Contains("管理员")))
                .Select(a => a.F_Id)
                .ToListAsync();
            var users = await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Account)
                .ToListAsync();
            //在内存中过滤，避免跨表排序规则冲突
            return users.Where(a => !string.IsNullOrEmpty(a.F_RoleId) && roleIds.Any(r => a.F_RoleId.Contains(r))).ToList();
        }

        /// <summary>
        /// 学生列表（可按班级过滤）
        /// </summary>
        public async Task<List<UserEntity>> GetStudentList(string classId)
        {
            var studentRoleId = await repository.Db.Queryable<RoleEntity>()
                .Where(a => a.F_FullName == "学生" && a.F_EnabledMark == true)
                .Select(a => a.F_Id)
                .FirstAsync();
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true);
            if (!string.IsNullOrEmpty(studentRoleId))
            {
                query = query.Where(a => a.F_RoleId != null && a.F_RoleId.Contains(studentRoleId));
            }
            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(a => a.F_OrganizeId != null && a.F_OrganizeId.Contains(classId));
            }
            return await query.OrderBy(a => a.F_Account).ToListAsync();
        }
        #endregion

        #region 授课计划
        /// <summary>
        /// 授课计划列表（含名称）
        /// </summary>
        public async Task<List<TimetablePlanExtend>> GetPlanList(string semester, string classId)
        {
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == null || a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(semester))
            {
                query = query.Where(a => a.F_Semester == semester);
            }
            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(a => a.F_ClassId == classId);
            }
            var list = await query.OrderBy(a => a.F_Semester).ToListAsync();
            var classMap = (await GetClassList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_FullName);
            var courseMap = (await GetCourseList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_ItemName);
            var teacherMap = (await GetTeacherList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_RealName);
            var result = new List<TimetablePlanExtend>();
            foreach (var item in list)
            {
                result.Add(new TimetablePlanExtend
                {
                    F_Id = item.F_Id,
                    F_Semester = item.F_Semester,
                    F_ClassId = item.F_ClassId,
                    F_CourseId = item.F_CourseId,
                    F_TeacherId = item.F_TeacherId,
                    F_WeeklyHours = item.F_WeeklyHours,
                    F_EnabledMark = item.F_EnabledMark,
                    F_CreatorTime = item.F_CreatorTime,
                    F_ClassName = item.F_ClassId != null && classMap.ContainsKey(item.F_ClassId) ? classMap[item.F_ClassId] : "",
                    F_CourseName = item.F_CourseId != null && courseMap.ContainsKey(item.F_CourseId) ? courseMap[item.F_CourseId] : "",
                    F_TeacherName = item.F_TeacherId != null && teacherMap.ContainsKey(item.F_TeacherId) ? teacherMap[item.F_TeacherId] : ""
                });
            }
            return result;
        }

        public async Task<TimetablePlanEntity> GetPlanForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 授课计划中已有的学期列表（自动排课学期下拉）
        /// </summary>
        public async Task<List<string>> GetSemesterList()
        {
            var list = await repository.IQueryable()
                .Where(a => a.F_DeleteMark == null || a.F_DeleteMark == false)
                .Where(a => a.F_Semester != null && a.F_Semester != "")
                .Select(a => a.F_Semester)
                .ToListAsync();
            return list.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().OrderByDescending(a => a).ToList();
        }

        public async Task SubmitPlanForm(TimetablePlanEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(entity.F_TeacherId))
            {
                entity.F_TeacherId = null;
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                //同一学期同班同科目只允许一条计划
                var exist = await repository.FindEntity(a => a.F_Semester == entity.F_Semester
                    && a.F_ClassId == entity.F_ClassId && a.F_CourseId == entity.F_CourseId && a.F_DeleteMark == false);
                if (exist != null)
                {
                    throw new Exception("该班级在此学期已存在相同科目的授课计划");
                }
                entity.F_EnabledMark = true;
                entity.F_DeleteMark = false;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeletePlanForm(string keyValue)
        {
            await repository.Delete(a => a.F_Id == keyValue);
        }
        #endregion

        #region 自动排课
        /// <summary>
        /// 自动生成课表：按授课计划把课时贪心分配到课表格子
        /// 规则：同班同时间不冲突、同教师同时间不冲突、同科目每天最多N节、跳过不排课时间
        /// </summary>
        public async Task<string> AutoGenerate(string semester, List<string> classIds)
        {
            if (string.IsNullOrWhiteSpace(semester))
            {
                throw new Exception("学期不能为空");
            }
            if (classIds == null || classIds.Count == 0)
            {
                throw new Exception("请选择要排课的班级");
            }
            //排课参数
            var param = await GetScheduleParam();
            int daysPerWeek = param.F_DaysPerWeek ?? 5;
            int sectionsPerDay = param.F_SectionsPerDay ?? 8;
            int maxSameCourseDaily = param.F_MaxSameCourseDaily ?? 2;
            //不排课时间（全校/教师）
            var noSchedules = await repository.Db.Queryable<NoScheduleEntity>()
                .Where(a => a.F_DeleteMark == false && (a.F_EnabledMark == null || a.F_EnabledMark == true))
                .ToListAsync();
            var globalNoSched = noSchedules.Where(a => a.F_Type == 1)
                .Select(a => a.F_WeekDay + "_" + a.F_Section).ToList();
            var teacherNoSched = noSchedules.Where(a => a.F_Type == 3 && !string.IsNullOrEmpty(a.F_TargetId))
                .Select(a => a.F_TargetId + "_" + a.F_WeekDay + "_" + a.F_Section).ToList();

            var plans = await repository.IQueryable()
                .Where(a => a.F_Semester == semester && a.F_DeleteMark == false && classIds.Contains(a.F_ClassId))
                .ToListAsync();
            if (plans.Count == 0)
            {
                throw new Exception("所选班级在该学期没有授课计划，请先维护授课计划");
            }
            //先清除这些班级该学期已有课表
            await repository.Db.Deleteable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && classIds.Contains(a.F_ClassId))
                .ExecuteCommandAsync();

            //其他班级已有课表中的教师占用（用于教师冲突检测）
            var otherEntries = await repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && a.F_DeleteMark == false && !classIds.Contains(a.F_ClassId))
                .ToListAsync();
            var teacherBusy = new HashSet<string>();
            foreach (var e in otherEntries)
            {
                if (!string.IsNullOrEmpty(e.F_TeacherId))
                {
                    teacherBusy.Add(e.F_TeacherId + "_" + e.F_WeekDay + "_" + e.F_Section);
                }
            }

            var newEntries = new List<TimetableEntity>();
            var rand = new Random();
            var failed = new List<string>();
            foreach (var classId in classIds)
            {
                var classPlans = plans.Where(a => a.F_ClassId == classId).ToList();
                //该班已占用格子
                var classBusy = new HashSet<string>();
                //同科目每天已排节数
                var courseDaily = new Dictionary<string, int>();
                //展开为课次列表（优先排课时多的科目）
                var sessions = new List<TimetablePlanEntity>();
                foreach (var p in classPlans.OrderByDescending(a => a.F_WeeklyHours))
                {
                    for (int i = 0; i < (p.F_WeeklyHours ?? 0); i++)
                    {
                        sessions.Add(p);
                    }
                }
                int totalSlots = daysPerWeek * sectionsPerDay;
                if (sessions.Count > totalSlots)
                {
                    throw new Exception("班级课次总数超过每周可用格子数（" + totalSlots + "），请调整授课计划");
                }
                foreach (var p in sessions)
                {
                    bool placed = false;
                    //随机起点尝试所有格子，保证分散
                    int startDay = rand.Next(daysPerWeek) + 1;
                    int startSec = rand.Next(sectionsPerDay) + 1;
                    for (int tryCount = 0; tryCount < totalSlots && !placed; tryCount++)
                    {
                        int day = ((startDay - 1 + tryCount / sectionsPerDay) % daysPerWeek) + 1;
                        int sec = ((startSec - 1 + tryCount) % sectionsPerDay) + 1;
                        string cellKey = day + "_" + sec;
                        if (classBusy.Contains(cellKey))
                        {
                            continue;
                        }
                        //全校不排课
                        if (globalNoSched.Contains(cellKey))
                        {
                            continue;
                        }
                        //同科目每天最多N节
                        string dailyKey = p.F_CourseId + "_" + day;
                        if (courseDaily.ContainsKey(dailyKey) && courseDaily[dailyKey] >= maxSameCourseDaily)
                        {
                            continue;
                        }
                        //教师冲突
                        if (!string.IsNullOrEmpty(p.F_TeacherId) && teacherBusy.Contains(p.F_TeacherId + "_" + day + "_" + sec))
                        {
                            continue;
                        }
                        //教师不排课时间
                        if (!string.IsNullOrEmpty(p.F_TeacherId) && teacherNoSched.Contains(p.F_TeacherId + "_" + day + "_" + sec))
                        {
                            continue;
                        }
                        var entry = new TimetableEntity
                        {
                            F_Semester = semester,
                            F_ClassId = classId,
                            F_WeekDay = day,
                            F_Section = sec,
                            F_CourseId = p.F_CourseId,
                            F_TeacherId = p.F_TeacherId,
                            F_EnabledMark = true,
                            F_DeleteMark = false
                        };
                        entry.Create();
                        newEntries.Add(entry);
                        classBusy.Add(cellKey);
                        if (!string.IsNullOrEmpty(p.F_TeacherId))
                        {
                            teacherBusy.Add(p.F_TeacherId + "_" + day + "_" + sec);
                        }
                        courseDaily[dailyKey] = (courseDaily.ContainsKey(dailyKey) ? courseDaily[dailyKey] : 0) + 1;
                        placed = true;
                    }
                    if (!placed)
                    {
                        failed.Add(p.F_CourseId);
                    }
                }
            }
            if (newEntries.Count > 0)
            {
                await repository.Db.Insertable(newEntries).ExecuteCommandAsync();
            }
            if (failed.Count > 0)
            {
                return "排课完成，但有 " + failed.Count + " 节课因冲突未能安排，请手工调整";
            }
            return "排课成功，共安排 " + newEntries.Count + " 节课";
        }

        /// <summary>
        /// 清空课表
        /// </summary>
        public async Task ClearTimetable(string semester, string classId)
        {
            await repository.Db.Deleteable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && a.F_ClassId == classId)
                .ExecuteCommandAsync();
        }
        #endregion

        #region 课表查询
        /// <summary>
        /// 课表矩阵数据（按班级或教师）
        /// </summary>
        public async Task<List<TimetableCellModel>> GetTimetable(string semester, string classId = "", string teacherId = "", string studentId = "")
        {
            //学生视角：取学生所在班级
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = await repository.Db.Queryable<UserEntity>().InSingleAsync(studentId);
                if (student == null || string.IsNullOrEmpty(student.F_OrganizeId))
                {
                    return new List<TimetableCellModel>();
                }
                var classMap0 = (await GetClassList()).Select(a => a.F_Id).ToList();
                classId = student.F_OrganizeId.Split(',').Select(a => a.Trim()).FirstOrDefault(a => classMap0.Contains(a)) ?? "";
                if (string.IsNullOrEmpty(classId))
                {
                    return new List<TimetableCellModel>();
                }
            }
            var query = repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(a => a.F_ClassId == classId);
            }
            if (!string.IsNullOrEmpty(teacherId))
            {
                query = query.Where(a => a.F_TeacherId == teacherId);
            }
            var list = await query.OrderBy(a => a.F_WeekDay).OrderBy(a => a.F_Section).ToListAsync();
            var classMap = (await GetClassList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_FullName);
            var courseMap = (await GetCourseList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_ItemName);
            var teacherMap = (await GetTeacherList()).GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_RealName);
            var result = new List<TimetableCellModel>();
            foreach (var e in list)
            {
                result.Add(new TimetableCellModel
                {
                    F_Id = e.F_Id,
                    F_WeekDay = e.F_WeekDay ?? 0,
                    F_Section = e.F_Section ?? 0,
                    F_CourseId = e.F_CourseId,
                    F_CourseName = e.F_CourseId != null && courseMap.ContainsKey(e.F_CourseId) ? courseMap[e.F_CourseId] : "",
                    F_TeacherId = e.F_TeacherId,
                    F_TeacherName = e.F_TeacherId != null && teacherMap.ContainsKey(e.F_TeacherId) ? teacherMap[e.F_TeacherId] : "",
                    F_ClassId = e.F_ClassId,
                    F_ClassName = e.F_ClassId != null && classMap.ContainsKey(e.F_ClassId) ? classMap[e.F_ClassId] : "",
                    F_Classroom = e.F_Classroom,
                    F_ClassroomId = e.F_ClassroomId
                });
            }
            return result;
        }

        /// <summary>
        /// 保存单个格子（新增或覆盖），含教师/教室冲突及不排课校验
        /// </summary>
        public async Task SaveCell(TimetableEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.F_Semester) || string.IsNullOrEmpty(entity.F_ClassId)
                || entity.F_WeekDay == null || entity.F_Section == null || string.IsNullOrEmpty(entity.F_CourseId))
            {
                throw new Exception("学期、班级、星期、节次、科目均不能为空");
            }
            if (string.IsNullOrEmpty(entity.F_TeacherId))
            {
                entity.F_TeacherId = null;
            }
            if (string.IsNullOrEmpty(entity.F_ClassroomId))
            {
                entity.F_ClassroomId = null;
            }
            //选择教室后同步教室名称
            if (!string.IsNullOrEmpty(entity.F_ClassroomId))
            {
                var room = await repository.Db.Queryable<ClassroomEntity>()
                    .Where(a => a.F_Id == entity.F_ClassroomId).FirstAsync();
                entity.F_Classroom = room == null ? entity.F_Classroom : room.F_Name;
            }
            var exist = await repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Semester == entity.F_Semester && a.F_ClassId == entity.F_ClassId
                    && a.F_WeekDay == entity.F_WeekDay && a.F_Section == entity.F_Section && a.F_DeleteMark == false)
                .FirstAsync();
            var excludeId = exist == null ? "" : exist.F_Id;
            //教师冲突：同教师同时间已被其他班级占用
            if (!string.IsNullOrEmpty(entity.F_TeacherId))
            {
                var conflict = await repository.Db.Queryable<TimetableEntity>()
                    .Where(a => a.F_Semester == entity.F_Semester && a.F_WeekDay == entity.F_WeekDay
                        && a.F_Section == entity.F_Section && a.F_TeacherId == entity.F_TeacherId
                        && a.F_ClassId != entity.F_ClassId && a.F_DeleteMark == false)
                    .FirstAsync();
                if (conflict != null)
                {
                    throw new Exception("该教师在此时间已有其他班级课程，保存失败");
                }
            }
            //教室冲突：同教室同时间已被占用
            if (!string.IsNullOrEmpty(entity.F_ClassroomId))
            {
                var roomBusy = await repository.Db.Queryable<TimetableEntity>()
                    .Where(a => a.F_Semester == entity.F_Semester && a.F_WeekDay == entity.F_WeekDay
                        && a.F_Section == entity.F_Section && a.F_ClassroomId == entity.F_ClassroomId
                        && a.F_DeleteMark == false && a.F_Id != excludeId)
                    .AnyAsync();
                if (roomBusy)
                {
                    throw new Exception("该教室在此时间已被占用，保存失败");
                }
            }
            //不排课校验（全校/教师/教室）
            var noSchedules = await repository.Db.Queryable<NoScheduleEntity>()
                .Where(a => a.F_WeekDay == entity.F_WeekDay && a.F_Section == entity.F_Section
                    && a.F_DeleteMark == false && (a.F_EnabledMark == null || a.F_EnabledMark == true))
                .ToListAsync();
            foreach (var ns in noSchedules)
            {
                if (ns.F_Type == 1)
                {
                    throw new Exception("该时间为全校不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
                if (ns.F_Type == 3 && !string.IsNullOrEmpty(entity.F_TeacherId) && ns.F_TargetId == entity.F_TeacherId)
                {
                    throw new Exception("该时间为该教师的不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
                if (ns.F_Type == 2 && !string.IsNullOrEmpty(entity.F_ClassroomId) && ns.F_TargetId == entity.F_ClassroomId)
                {
                    throw new Exception("该时间为该教室的不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
            }
            if (exist != null)
            {
                entity.Modify(exist.F_Id);
                await repository.Db.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
            }
            else
            {
                entity.F_EnabledMark = true;
                entity.F_DeleteMark = false;
                entity.Create();
                await repository.Db.Insertable(entity).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 清除单个格子
        /// </summary>
        public async Task ClearCell(string keyValue)
        {
            await repository.Db.Deleteable<TimetableEntity>().Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
        }
        #endregion
    }
}
