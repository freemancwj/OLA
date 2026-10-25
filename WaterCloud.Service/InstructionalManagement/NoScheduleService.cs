using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 不排课时间服务（全校/教室/教师）
    /// </summary>
    public class NoScheduleService : BaseService<NoScheduleEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public NoScheduleService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<NoScheduleExtend>> GetLookList(SoulPage<NoScheduleExtend> pagination, int? type)
        {
            pagination.field = "F_CreatorTime";
            pagination.order = "desc";
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (type != null)
            {
                query = query.Where(a => a.F_Type == type);
            }
            var list = await query.OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToListAsync();
            //内存补名称，避免跨表排序规则冲突
            var classroomMap = (await repository.Db.Queryable<ClassroomEntity>()
                .Where(a => a.F_DeleteMark == false).ToListAsync())
                .GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_Name);
            var teacherMap = (await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false).ToListAsync())
                .GroupBy(a => a.F_Id).ToDictionary(a => a.Key, a => a.First().F_RealName);
            var result = list.Select(a => new NoScheduleExtend
            {
                F_Id = a.F_Id,
                F_Type = a.F_Type,
                F_TargetId = a.F_TargetId,
                F_WeekDay = a.F_WeekDay,
                F_Section = a.F_Section,
                F_Reason = a.F_Reason,
                F_CreatorTime = a.F_CreatorTime,
                F_TargetName = a.F_Type == 1 ? "全校"
                    : (a.F_Type == 2 ? (a.F_TargetId != null && classroomMap.ContainsKey(a.F_TargetId) ? classroomMap[a.F_TargetId] : "")
                    : (a.F_TargetId != null && teacherMap.ContainsKey(a.F_TargetId) ? teacherMap[a.F_TargetId] : ""))
            }).ToList();
            pagination.count = result.Count;
            return result.Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows).ToList();
        }

        public async Task<NoScheduleEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 教室下拉（含教学楼名）
        /// </summary>
        public async Task<List<ClassroomEntity>> GetClassroomList()
        {
            return await repository.Db.Queryable<ClassroomEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_Name)
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetTeacherList(string keyword)
        {
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && (a.F_RoleId == null || !a.F_RoleId.Contains(StudentRoleId)));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_RealName.Contains(kw) || a.F_Account.Contains(kw));
            }
            return await query.OrderBy(a => a.F_Account).Take(200).ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(NoScheduleEntity entity, string keyValue)
        {
            if (entity == null || entity.F_Type == null)
            {
                throw new Exception("请选择类型");
            }
            if (entity.F_Type != 1 && string.IsNullOrWhiteSpace(entity.F_TargetId))
            {
                throw new Exception(entity.F_Type == 2 ? "请选择教室" : "请选择教师");
            }
            if (entity.F_WeekDay == null || entity.F_Section == null)
            {
                throw new Exception("请选择星期和节次");
            }
            if (entity.F_Type == 1)
            {
                entity.F_TargetId = null;
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_Type == entity.F_Type && a.F_WeekDay == entity.F_WeekDay && a.F_Section == entity.F_Section
                    && a.F_TargetId == entity.F_TargetId && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该时间段已存在相同的不排课设置");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                var data = await repository.FindEntity(keyValue);
                if (data == null || data.F_DeleteMark == true)
                {
                    throw new Exception("记录不存在");
                }
                data.F_Type = entity.F_Type;
                data.F_TargetId = entity.F_TargetId;
                data.F_WeekDay = entity.F_WeekDay;
                data.F_Section = entity.F_Section;
                data.F_Reason = entity.F_Reason;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new NoScheduleEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
