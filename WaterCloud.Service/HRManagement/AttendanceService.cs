using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.HRManagement
{
    /// <summary>
    /// 签到打卡服务（规则设置 + 员工打卡 + 记录查询）
    /// </summary>
    public class AttendanceService : BaseService<AttendanceRuleEntity>, IDenpendency
    {
        public AttendanceService(ISqlSugarClient context) : base(context)
        {
        }

        #region 打卡规则

        /// <summary>
        /// 获取当前打卡规则（单行配置，没有则返回默认）
        /// </summary>
        public async Task<AttendanceRuleEntity> GetRule()
        {
            var rule = await repository.Db.Queryable<AttendanceRuleEntity>()
                .OrderBy(a => a.F_CreatorTime)
                .FirstAsync();
            if (rule == null)
            {
                rule = new AttendanceRuleEntity
                {
                    F_StartTime = "08:30",
                    F_EndTime = "17:30",
                    F_LateGrace = 10,
                    F_EarlyGrace = 10,
                    F_EnabledMark = true
                };
            }
            return rule;
        }

        /// <summary>
        /// 保存打卡规则
        /// </summary>
        public async Task SaveRule(AttendanceRuleEntity entity)
        {
            var exist = await repository.Db.Queryable<AttendanceRuleEntity>().FirstAsync();
            if (exist == null)
            {
                entity.F_Id = Utils.GuId();
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                await repository.Insert(entity);
            }
            else
            {
                entity.F_Id = exist.F_Id;
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(entity).Where(a => a.F_Id == exist.F_Id).ExecuteCommandAsync();
            }
        }

        #endregion

        #region 员工打卡

        /// <summary>
        /// 获取当前用户今日打卡状态
        /// </summary>
        public async Task<AttendanceRecordEntity> GetTodayRecord()
        {
            var today = DateTime.Now.Date;
            return await repository.Db.Queryable<AttendanceRecordEntity>()
                .Where(a => a.F_UserId == currentuser.UserId && a.F_Date == today)
                .FirstAsync();
        }

        /// <summary>
        /// 上班打卡
        /// </summary>
        public async Task<string> ClockIn()
        {
            var rule = await GetRule();
            if (rule.F_EnabledMark != true)
            {
                throw new Exception("打卡功能未启用");
            }
            var now = DateTime.Now;
            var today = now.Date;
            var record = await GetTodayRecord();
            if (record != null && record.F_ClockInTime != null)
            {
                throw new Exception("今日已打过上班卡，打卡时间：" + record.F_ClockInTime.Value.ToString("HH:mm"));
            }
            var status = 0; // 0正常
            var remark = "";
            if (!string.IsNullOrEmpty(rule.F_StartTime) && TimeSpan.TryParse(rule.F_StartTime, out var startTs))
            {
                var lateLimit = startTs.Add(TimeSpan.FromMinutes(rule.F_LateGrace ?? 0));
                if (now.TimeOfDay > lateLimit)
                {
                    status = 1; // 迟到
                    remark = "迟到" + (int)(now.TimeOfDay - startTs).TotalMinutes + "分钟";
                }
            }
            if (record == null)
            {
                record = new AttendanceRecordEntity
                {
                    F_Id = Utils.GuId(),
                    F_UserId = currentuser.UserId,
                    F_Date = today,
                    F_ClockInTime = now,
                    F_ClockInStatus = status,
                    F_Remark = remark,
                    F_CreatorTime = now
                };
                await repository.Db.Insertable(record).ExecuteCommandAsync();
            }
            else
            {
                record.F_ClockInTime = now;
                record.F_ClockInStatus = status;
                record.F_Remark = remark;
                await repository.Db.Updateable(record).ExecuteCommandAsync();
            }
            return status == 0 ? "上班打卡成功" : "上班打卡成功（" + remark + "）";
        }

        /// <summary>
        /// 下班打卡
        /// </summary>
        public async Task<string> ClockOut()
        {
            var rule = await GetRule();
            if (rule.F_EnabledMark != true)
            {
                throw new Exception("打卡功能未启用");
            }
            var now = DateTime.Now;
            var today = now.Date;
            var record = await GetTodayRecord();
            if (record == null || record.F_ClockInTime == null)
            {
                throw new Exception("请先打上班卡");
            }
            if (record.F_ClockOutTime != null)
            {
                throw new Exception("今日已打过下班卡，打卡时间：" + record.F_ClockOutTime.Value.ToString("HH:mm"));
            }
            var status = 0; // 0正常
            var remark = "";
            if (!string.IsNullOrEmpty(rule.F_EndTime) && TimeSpan.TryParse(rule.F_EndTime, out var endTs))
            {
                var earlyLimit = endTs.Subtract(TimeSpan.FromMinutes(rule.F_EarlyGrace ?? 0));
                if (now.TimeOfDay < earlyLimit)
                {
                    status = 1; // 早退
                    remark = "早退" + (int)(endTs - now.TimeOfDay).TotalMinutes + "分钟";
                }
            }
            record.F_ClockOutTime = now;
            record.F_ClockOutStatus = status;
            record.F_Remark = string.IsNullOrEmpty(record.F_Remark) ? remark : record.F_Remark + "；" + remark;
            await repository.Db.Updateable(record).ExecuteCommandAsync();
            return status == 0 ? "下班打卡成功" : "下班打卡成功（" + remark + "）";
        }

        /// <summary>
        /// 当前用户打卡记录列表
        /// </summary>
        public async Task<List<AttendanceRecordEntity>> GetMyRecords(SoulPage<AttendanceRecordEntity> pagination, string month)
        {
            var queryable = repository.Db.Queryable<AttendanceRecordEntity>()
                .Where(a => a.F_UserId == currentuser.UserId);
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out var firstDay))
            {
                var lastDay = firstDay.AddMonths(1).AddDays(-1);
                queryable = queryable.Where(a => a.F_Date >= firstDay && a.F_Date <= lastDay);
            }
            queryable = queryable.OrderBy(a => a.F_Date, OrderByType.Desc);
            pagination.count = await queryable.CountAsync();
            return await queryable.ToPageListAsync(pagination.page, pagination.rows);
        }

        /// <summary>
        /// 打卡记录列表（管理员视角，联查用户）
        /// </summary>
        public async Task<List<AttendanceRecordExtend>> GetRecordList(SoulPage<AttendanceRecordExtend> pagination, string keyword, string date)
        {
            var queryable = repository.Db.Queryable<AttendanceRecordEntity, UserEntity>((r, u) => new object[]
            {
                JoinType.Left, r.F_UserId == u.F_Id
            });
            if (!string.IsNullOrEmpty(keyword))
            {
                queryable = queryable.Where((r, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var d))
            {
                queryable = queryable.Where((r, u) => r.F_Date == d.Date);
            }
            queryable = queryable.OrderBy((r, u) => r.F_Date, OrderByType.Desc);
            var query = queryable.Select((r, u) => new AttendanceRecordExtend
            {
                F_Id = r.F_Id,
                F_UserId = r.F_UserId,
                F_Date = r.F_Date,
                F_ClockInTime = r.F_ClockInTime,
                F_ClockInStatus = r.F_ClockInStatus,
                F_ClockOutTime = r.F_ClockOutTime,
                F_ClockOutStatus = r.F_ClockOutStatus,
                F_Remark = r.F_Remark,
                F_CreatorTime = r.F_CreatorTime,
                F_Account = u.F_Account,
                F_RealName = u.F_RealName,
                F_OrganizeName = u.F_OrganizeId
            });
            pagination.count = await query.CountAsync();
            var list = await query.ToPageListAsync(pagination.page, pagination.rows);
            // 补充部门名称
            if (list.Count > 0)
            {
                var orgIds = list.Where(a => !string.IsNullOrEmpty(a.F_OrganizeName))
                    .SelectMany(a => a.F_OrganizeName.Split(',')).Distinct().ToList();
                if (orgIds.Count() > 0)
                {
                    var orgs = await repository.Db.Queryable<OrganizeEntity>()
                        .Where(a => orgIds.Contains(a.F_Id)).Select(a => new { a.F_Id, a.F_FullName }).ToListAsync();
                    var dict = orgs.ToDictionary(a => a.F_Id, a => a.F_FullName);
                    foreach (var item in list)
                    {
                        if (!string.IsNullOrEmpty(item.F_OrganizeName))
                        {
                            var parts = item.F_OrganizeName.Split(',').Select(a => a.Trim()).Where(a => a.Length > 0).ToList();
                            item.F_OrganizeName = string.Join(",", parts.Select(a => dict.ContainsKey(a) ? dict[a] : a));
                        }
                    }
                }
            }
            return list;
        }

        #endregion
    }
}
