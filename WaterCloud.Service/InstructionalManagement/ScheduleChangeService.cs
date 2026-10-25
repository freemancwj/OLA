using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemManage;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 调课申请服务（申请/审批，审批通过后回写课表）
    /// </summary>
    public class ScheduleChangeService : BaseService<ScheduleChangeEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public ScheduleChangeService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 调课申请列表（非管理员仅看本人申请）
        /// </summary>
        public async Task<List<ScheduleChangeExtend>> GetLookList(SoulPage<ScheduleChangeExtend> pagination, string keyword, int? status)
        {
            var allowedFields = new HashSet<string> { "F_Semester", "F_ClassName", "F_CourseName", "F_TeacherName", "F_Status", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, o.F_FullName AS F_ClassName, c.F_ItemName AS F_CourseName,
       tu.F_RealName AS F_TeacherName, au.F_RealName AS F_ApplicantName,
       cr.F_Name AS F_NewClassroomName, du.F_RealName AS F_AuditUserName
FROM school_schedule_change a
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user tu ON a.F_TeacherId = tu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user au ON a.F_ApplicantId = au.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_NewClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user du ON a.F_AuditUserId = du.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (currentuser == null || (!currentuser.IsSuperAdmin && !currentuser.IsAdmin))
            {
                sb.Append(" AND a.F_ApplicantId = '" + Q(currentuser == null ? "" : currentuser.UserId) + "'");
            }
            if (status != null)
            {
                sb.Append(" AND a.F_Status = " + status.Value);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR tu.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.SqlQueryable<ScheduleChangeExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<ScheduleChangeEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 申请详情（含名称，审批页展示用）
        /// </summary>
        public async Task<ScheduleChangeExtend> GetFormDetail(string keyValue)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*, o.F_FullName AS F_ClassName, c.F_ItemName AS F_CourseName,
       tu.F_RealName AS F_TeacherName, au.F_RealName AS F_ApplicantName,
       cr.F_Name AS F_NewClassroomName, du.F_RealName AS F_AuditUserName
FROM school_schedule_change a
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user tu ON a.F_TeacherId = tu.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user au ON a.F_ApplicantId = au.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_classroom cr ON a.F_NewClassroomId = cr.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user du ON a.F_AuditUserId = du.F_Id COLLATE Chinese_PRC_CI_AS
WHERE a.F_Id = '" + Q(keyValue) + "'");
            return await repository.SqlQueryable<ScheduleChangeExtend>(sb.ToString()).FirstAsync();
        }

        /// <summary>
        /// 可申请调课的课表格子（管理员全部，教师仅本人授课）
        /// </summary>
        public async Task<List<TimetableCellModel>> GetMyTimetableList(string semester)
        {
            if (string.IsNullOrWhiteSpace(semester))
            {
                return new List<TimetableCellModel>();
            }
            var query = repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && a.F_DeleteMark == false);
            if (currentuser == null || (!currentuser.IsSuperAdmin && !currentuser.IsAdmin))
            {
                query = query.Where(a => a.F_TeacherId == currentuser.UserId);
            }
            var list = await query.OrderBy(a => a.F_WeekDay).OrderBy(a => a.F_Section).ToListAsync();
            var classMap = (await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false).ToListAsync())
                .GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_FullName);
            var courseMap = (await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_DeleteMark == false).ToListAsync())
                .GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_ItemName);
            var teacherMap = (await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false).ToListAsync())
                .GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_RealName);
            return list.Select(e => new TimetableCellModel
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
            }).ToList();
        }

        /// <summary>
        /// 教室下拉
        /// </summary>
        public async Task<List<ClassroomEntity>> GetClassroomList()
        {
            return await repository.Db.Queryable<ClassroomEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 提交调课申请
        /// </summary>
        public async Task SubmitForm(ScheduleChangeEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_TimetableId))
            {
                throw new Exception("请选择要调整的课程");
            }
            if (entity.F_NewWeekDay == null || entity.F_NewSection == null)
            {
                throw new Exception("请选择新的上课时间");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Reason))
            {
                throw new Exception("请填写调课原因");
            }
            var cell = await repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Id == entity.F_TimetableId && a.F_DeleteMark == false)
                .FirstAsync();
            if (cell == null)
            {
                throw new Exception("原课表记录不存在或已被删除");
            }
            //非管理员只能调整自己的课
            if (currentuser != null && !currentuser.IsSuperAdmin && !currentuser.IsAdmin && cell.F_TeacherId != currentuser.UserId)
            {
                throw new Exception("只能对本人授课的课程申请调课");
            }
            //同一条课表存在待审批申请时不允许重复提交
            var pending = await repository.IQueryable()
                .Where(a => a.F_TimetableId == entity.F_TimetableId && a.F_Status == 0 && a.F_DeleteMark == false)
                .AnyAsync();
            if (pending)
            {
                throw new Exception("该课程已有待审批的调课申请，请勿重复提交");
            }
            //新时间冲突校验（排除自身格子）
            await CheckConflict(cell.F_Semester, cell.F_ClassId, cell.F_TeacherId, entity.F_NewClassroomId,
                entity.F_NewWeekDay.Value, entity.F_NewSection.Value, cell.F_Id);
            var data = new ScheduleChangeEntity
            {
                F_TimetableId = cell.F_Id,
                F_Semester = cell.F_Semester,
                F_ClassId = cell.F_ClassId,
                F_CourseId = cell.F_CourseId,
                F_TeacherId = cell.F_TeacherId,
                F_OldWeekDay = cell.F_WeekDay,
                F_OldSection = cell.F_Section,
                F_NewWeekDay = entity.F_NewWeekDay,
                F_NewSection = entity.F_NewSection,
                F_NewClassroomId = string.IsNullOrEmpty(entity.F_NewClassroomId) ? cell.F_ClassroomId : entity.F_NewClassroomId,
                F_Reason = entity.F_Reason,
                F_ApplicantId = currentuser.UserId,
                F_Status = 0,
                F_DeleteMark = false,
                F_EnabledMark = true
            };
            data.Create();
            await repository.Insert(data);
        }

        /// <summary>
        /// 审批（status 1通过 2拒绝），通过后回写课表
        /// </summary>
        public async Task Audit(string keyValue, int status, string remark)
        {
            var data = await repository.FindEntity(keyValue);
            if (data == null || data.F_DeleteMark == true)
            {
                throw new Exception("申请不存在");
            }
            if (data.F_Status != 0)
            {
                throw new Exception("该申请已审批，请勿重复操作");
            }
            if (status != 1 && status != 2)
            {
                throw new Exception("审批状态不正确");
            }
            if (status == 1)
            {
                //通过后回写课表，先做冲突校验
                var cell = await repository.Db.Queryable<TimetableEntity>()
                    .Where(a => a.F_Id == data.F_TimetableId && a.F_DeleteMark == false)
                    .FirstAsync();
                if (cell == null)
                {
                    throw new Exception("原课表记录已被删除，无法调课");
                }
                await CheckConflict(data.F_Semester, data.F_ClassId, data.F_TeacherId, data.F_NewClassroomId,
                    data.F_NewWeekDay.Value, data.F_NewSection.Value, data.F_TimetableId);
                cell.F_WeekDay = data.F_NewWeekDay;
                cell.F_Section = data.F_NewSection;
                if (!string.IsNullOrEmpty(data.F_NewClassroomId))
                {
                    cell.F_ClassroomId = data.F_NewClassroomId;
                    var room = await repository.Db.Queryable<ClassroomEntity>().Where(a => a.F_Id == data.F_NewClassroomId).FirstAsync();
                    cell.F_Classroom = room == null ? cell.F_Classroom : room.F_Name;
                }
                cell.F_LastModifyTime = DateTime.Now;
                cell.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(cell).ExecuteCommandAsync();
            }
            data.F_Status = status;
            data.F_AuditUserId = currentuser.UserId;
            data.F_AuditTime = DateTime.Now;
            data.F_AuditRemark = remark;
            data.Modify(keyValue);
            await repository.Update(data);
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = await repository.IQueryable().Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false).ToListAsync();
            if (list.Any(a => a.F_Status == 1))
            {
                throw new Exception("已通过的申请不允许删除");
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ScheduleChangeEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        #region 冲突校验

        /// <summary>
        /// 校验指定时间格子的冲突：班级/教师/教室占用 + 不排课时间
        /// </summary>
        private async Task CheckConflict(string semester, string classId, string teacherId, string classroomId, int weekDay, int section, string excludeId)
        {
            //班级占用
            var classBusy = await repository.Db.Queryable<TimetableEntity>()
                .Where(a => a.F_Semester == semester && a.F_ClassId == classId && a.F_WeekDay == weekDay
                    && a.F_Section == section && a.F_DeleteMark == false && a.F_Id != excludeId)
                .AnyAsync();
            if (classBusy)
            {
                throw new Exception("该班级在新时间已有课程安排");
            }
            //教师占用
            if (!string.IsNullOrEmpty(teacherId))
            {
                var teacherBusy = await repository.Db.Queryable<TimetableEntity>()
                    .Where(a => a.F_Semester == semester && a.F_TeacherId == teacherId && a.F_WeekDay == weekDay
                        && a.F_Section == section && a.F_DeleteMark == false && a.F_Id != excludeId)
                    .AnyAsync();
                if (teacherBusy)
                {
                    throw new Exception("该教师在新时间已有其他课程");
                }
            }
            //教室占用
            if (!string.IsNullOrEmpty(classroomId))
            {
                var roomBusy = await repository.Db.Queryable<TimetableEntity>()
                    .Where(a => a.F_Semester == semester && a.F_ClassroomId == classroomId && a.F_WeekDay == weekDay
                        && a.F_Section == section && a.F_DeleteMark == false && a.F_Id != excludeId)
                    .AnyAsync();
                if (roomBusy)
                {
                    throw new Exception("该教室在新时间已被占用");
                }
            }
            //不排课时间
            var noSchedules = await repository.Db.Queryable<NoScheduleEntity>()
                .Where(a => a.F_WeekDay == weekDay && a.F_Section == section && a.F_DeleteMark == false
                    && (a.F_EnabledMark == null || a.F_EnabledMark == true))
                .ToListAsync();
            foreach (var ns in noSchedules)
            {
                if (ns.F_Type == 1)
                {
                    throw new Exception("新时间为全校不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
                if (ns.F_Type == 3 && !string.IsNullOrEmpty(teacherId) && ns.F_TargetId == teacherId)
                {
                    throw new Exception("新时间为该教师的不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
                if (ns.F_Type == 2 && !string.IsNullOrEmpty(classroomId) && ns.F_TargetId == classroomId)
                {
                    throw new Exception("新时间为该教室的不排课时间" + (string.IsNullOrEmpty(ns.F_Reason) ? "" : "（" + ns.F_Reason + "）"));
                }
            }
        }

        #endregion

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
