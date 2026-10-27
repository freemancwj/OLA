using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.SalaryManagement
{
    /// <summary>
    /// 采集类数据的通用 CRUD 基类（联查 sys_user 展示账号/姓名，按月+关键词分页）
    /// </summary>
    public abstract class SalCollectService<TEntity, TExtend> : BaseService<TEntity>, IDenpendency
        where TEntity : class, new()
        where TExtend : class, new()
    {
        public SalCollectService(ISqlSugarClient context) : base(context) { }

        protected abstract Expression<Func<TEntity, UserEntity, TExtend>> BuildSelect();
        protected abstract Expression<Func<TEntity, object>>[] OrderExpr();
        protected abstract string GetUserId(TEntity e);
        protected abstract void SetNew(TEntity e, string id, string userId);
        protected abstract string EntityName { get; }

        public virtual async Task<List<TExtend>> GetList(SoulPage<TExtend> pagination, string month, string keyword, Dictionary<string, string> extra = null)
        {
            var queryable = repository.Db.Queryable<TEntity, UserEntity>((e, u) => new object[] { JoinType.Left, EF(e) == u.F_Id })
                .Where((e, u) => DelMark(e) == false && u.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(month)) queryable = queryable.Where((e, u) => MonthOf(e) == month);
            if (!string.IsNullOrEmpty(keyword)) queryable = queryable.Where((e, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword));
            queryable = ApplyExtra(queryable, extra);
            var orders = OrderExpr();
            foreach (var o in orders) queryable = queryable.OrderBy(o, OrderByType.Desc);
            var query = queryable.Select(BuildSelect());
            pagination.count = await query.CountAsync();
            return await query.ToPageListAsync(pagination.page, pagination.rows);
        }
        protected virtual ISugarQueryable<TEntity, UserEntity> ApplyExtra(ISugarQueryable<TEntity, UserEntity> q, Dictionary<string, string> extra) => q;
        // 反射取字段，避免每个子类重复写 lambda
        private static string EF(TEntity e) => (string)typeof(TEntity).GetProperty("F_UserId").GetValue(e);
        private static bool? DelMark(TEntity e) => (bool?)typeof(TEntity).GetProperty("F_DeleteMark").GetValue(e);
        private static string MonthOf(TEntity e) => (string)typeof(TEntity).GetProperty("F_Month").GetValue(e);

        public async Task<TEntity> GetForm(string keyValue)
            => await repository.Db.Queryable<TEntity>().Where(a => EF(a) != null && (string)typeof(TEntity).GetProperty("F_Id").GetValue(a) == keyValue).FirstAsync();

        public async Task SubmitForm(TEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                SetNew(entity, Utils.GuId(), currentuser.UserId);
                await repository.Insert(entity);
            }
            else
            {
                typeof(TEntity).GetProperty("F_LastModifyTime").SetValue(entity, DateTime.Now);
                typeof(TEntity).GetProperty("F_LastModifyUserId").SetValue(entity, currentuser.UserId);
                var idProp = typeof(TEntity).GetProperty("F_Id");
                await repository.Db.Updateable(entity).Where(a => (string)idProp.GetValue(a) == keyValue).ExecuteCommandAsync();
            }
        }
        public async Task DeleteForm(string keyValue)
        {
            var e = await GetForm(keyValue);
            if (e != null) await repository.Delete(a => (string)typeof(TEntity).GetProperty("F_Id").GetValue(a) == keyValue);
        }
    }

    /// <summary>课时记录服务</summary>
    public class SalPeriodRecordService : SalCollectService<SalPeriodRecordEntity, SalPeriodRecordExtend>
    {
        public SalPeriodRecordService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "课时记录";
        protected override string GetUserId(SalPeriodRecordEntity e) => e.F_UserId;
        protected override void SetNew(SalPeriodRecordEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalPeriodRecordEntity, UserEntity, SalPeriodRecordExtend>> BuildSelect() => (e, u) => new SalPeriodRecordExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_NormalPeriods = e.F_NormalPeriods, F_OvertimePeriods = e.F_OvertimePeriods,
            F_SubstitutePeriods = e.F_SubstitutePeriods, F_MorningReadCount = e.F_MorningReadCount, F_EveningStudyCount = e.F_EveningStudyCount,
            F_Remark = e.F_Remark, F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName
        };
        protected override Expression<Func<SalPeriodRecordEntity, object>>[] OrderExpr() => new Expression<Func<SalPeriodRecordEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>班主任考核服务</summary>
    public class SalHeadTeacherAssessService : SalCollectService<SalHeadTeacherAssessEntity, SalHeadTeacherAssessExtend>
    {
        public SalHeadTeacherAssessService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "班主任考核";
        protected override string GetUserId(SalHeadTeacherAssessEntity e) => e.F_UserId;
        protected override void SetNew(SalHeadTeacherAssessEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalHeadTeacherAssessEntity, UserEntity, SalHeadTeacherAssessExtend>> BuildSelect() => (e, u) => new SalHeadTeacherAssessExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_ClassId = e.F_ClassId, F_ClassSize = e.F_ClassSize,
            F_IsGraduation = e.F_IsGraduation, F_AssessLevel = e.F_AssessLevel, F_Score = e.F_Score, F_Remark = e.F_Remark,
            F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName, F_ClassName = e.F_ClassId
        };
        protected override Expression<Func<SalHeadTeacherAssessEntity, object>>[] OrderExpr() => new Expression<Func<SalHeadTeacherAssessEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>课后服务记录服务</summary>
    public class SalAfterSchoolRecordService : SalCollectService<SalAfterSchoolRecordEntity, SalAfterSchoolRecordExtend>
    {
        public SalAfterSchoolRecordService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "课后服务记录";
        protected override string GetUserId(SalAfterSchoolRecordEntity e) => e.F_UserId;
        protected override void SetNew(SalAfterSchoolRecordEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalAfterSchoolRecordEntity, UserEntity, SalAfterSchoolRecordExtend>> BuildSelect() => (e, u) => new SalAfterSchoolRecordExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_ServiceType = e.F_ServiceType, F_Periods = e.F_Periods,
            F_Remark = e.F_Remark, F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName
        };
        protected override Expression<Func<SalAfterSchoolRecordEntity, object>>[] OrderExpr() => new Expression<Func<SalAfterSchoolRecordEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>监考记录服务</summary>
    public class SalInvigilateRecordService : SalCollectService<SalInvigilateRecordEntity, SalInvigilateRecordExtend>
    {
        public SalInvigilateRecordService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "监考记录";
        protected override string GetUserId(SalInvigilateRecordEntity e) => e.F_UserId;
        protected override void SetNew(SalInvigilateRecordEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalInvigilateRecordEntity, UserEntity, SalInvigilateRecordExtend>> BuildSelect() => (e, u) => new SalInvigilateRecordExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_ExamName = e.F_ExamName, F_Sessions = e.F_Sessions,
            F_Remark = e.F_Remark, F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName
        };
        protected override Expression<Func<SalInvigilateRecordEntity, object>>[] OrderExpr() => new Expression<Func<SalInvigilateRecordEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>考勤汇总服务</summary>
    public class SalAttendanceSummaryService : SalCollectService<SalAttendanceSummaryEntity, SalAttendanceSummaryExtend>
    {
        public SalAttendanceSummaryService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "考勤汇总";
        protected override string GetUserId(SalAttendanceSummaryEntity e) => e.F_UserId;
        protected override void SetNew(SalAttendanceSummaryEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalAttendanceSummaryEntity, UserEntity, SalAttendanceSummaryExtend>> BuildSelect() => (e, u) => new SalAttendanceSummaryExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_SickLeaveDays = e.F_SickLeaveDays, F_PersonalLeaveDays = e.F_PersonalLeaveDays,
            F_AbsentDays = e.F_AbsentDays, F_LateCount = e.F_LateCount, F_EarlyCount = e.F_EarlyCount, F_Remark = e.F_Remark,
            F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName
        };
        protected override Expression<Func<SalAttendanceSummaryEntity, object>>[] OrderExpr() => new Expression<Func<SalAttendanceSummaryEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>薪酬调整服务</summary>
    public class SalAdjustmentService : SalCollectService<SalAdjustmentEntity, SalAdjustmentExtend>
    {
        public SalAdjustmentService(ISqlSugarClient c) : base(c) { }
        protected override string EntityName => "薪酬调整";
        protected override string GetUserId(SalAdjustmentEntity e) => e.F_UserId;
        protected override void SetNew(SalAdjustmentEntity e, string id, string uid) { e.F_Id = id; e.F_CreatorTime = DateTime.Now; e.F_CreatorUserId = uid; e.F_DeleteMark = false; }
        protected override Expression<Func<SalAdjustmentEntity, UserEntity, SalAdjustmentExtend>> BuildSelect() => (e, u) => new SalAdjustmentExtend
        {
            F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_ItemName = e.F_ItemName, F_Amount = e.F_Amount,
            F_Reason = e.F_Reason, F_CreatorTime = e.F_CreatorTime, F_Account = u.F_Account, F_RealName = u.F_RealName
        };
        protected override Expression<Func<SalAdjustmentEntity, object>>[] OrderExpr() => new Expression<Func<SalAdjustmentEntity, object>>[] { e => e.F_Month, e => e.F_CreatorTime };
    }

    /// <summary>薪酬申诉服务</summary>
    public class SalAppealService : BaseService<SalAppealEntity>, IDenpendency
    {
        public SalAppealService(ISqlSugarClient c) : base(c) { }

        public async Task<List<SalAppealExtend>> GetList(SoulPage<SalAppealExtend> pagination, string month, string keyword, string status, bool onlyMine = false)
        {
            var queryable = repository.Db.Queryable<SalAppealEntity, UserEntity>((e, u) => new object[] { JoinType.Left, e.F_UserId == u.F_Id })
                .Where((e, u) => e.F_DeleteMark == false);
            if (onlyMine) { var uid = currentuser.UserId; queryable = queryable.Where((e, u) => e.F_UserId == uid); }
            if (!string.IsNullOrEmpty(month)) queryable = queryable.Where((e, u) => e.F_Month == month);
            if (!string.IsNullOrEmpty(keyword)) queryable = queryable.Where((e, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword));
            if (!string.IsNullOrEmpty(status)) { var s = int.Parse(status); queryable = queryable.Where((e, u) => e.F_Status == s); }
            queryable = queryable.OrderBy((e, u) => e.F_CreatorTime, OrderByType.Desc);
            var query = queryable.Select((e, u) => new SalAppealExtend
            {
                F_Id = e.F_Id, F_PayslipId = e.F_PayslipId, F_Month = e.F_Month, F_UserId = e.F_UserId, F_Content = e.F_Content,
                F_Status = e.F_Status, F_Reply = e.F_Reply, F_ReplyTime = e.F_ReplyTime, F_CreatorTime = e.F_CreatorTime,
                F_Account = u.F_Account, F_RealName = u.F_RealName
            });
            pagination.count = await query.CountAsync();
            return await query.ToPageListAsync(pagination.page, pagination.rows);
        }

        /// <summary>老师提交申诉（只能对自己的工资单）</summary>
        public async Task SubmitAppeal(string payslipId, string content)
        {
            var slip = await repository.Db.Queryable<SalPayslipEntity>().Where(a => a.F_Id == payslipId && a.F_DeleteMark == false).FirstAsync();
            if (slip == null) throw new Exception("工资单不存在");
            if (slip.F_UserId != currentuser.UserId) throw new Exception("只能对本人工资单申诉");
            var entity = new SalAppealEntity
            {
                F_Id = Utils.GuId(), F_PayslipId = payslipId, F_Month = slip.F_Month, F_UserId = currentuser.UserId,
                F_Content = content, F_Status = 0, F_DeleteMark = false, F_CreatorTime = DateTime.Now
            };
            await repository.Insert(entity);
        }

        /// <summary>人事回复处理申诉</summary>
        public async Task Reply(string keyValue, string reply)
        {
            var e = await repository.Db.Queryable<SalAppealEntity>().Where(a => a.F_Id == keyValue).FirstAsync();
            if (e == null) throw new Exception("申诉记录不存在");
            e.F_Reply = reply; e.F_Status = 1; e.F_ReplyUserId = currentuser.UserId; e.F_ReplyTime = DateTime.Now; e.F_LastModifyTime = DateTime.Now;
            await repository.Db.Updateable(e).UpdateColumns(a => new { a.F_Reply, a.F_Status, a.F_ReplyUserId, a.F_ReplyTime, a.F_LastModifyTime }).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
        }
    }
}
