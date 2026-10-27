using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.SalaryManagement
{
    /// <summary>
    /// 工资单服务：自动算薪引擎、审批流转、归档锁定、审计日志
    /// </summary>
    public class SalPayslipService : BaseService<SalPayslipEntity>, IDenpendency
    {
        public SalPayslipService(ISqlSugarClient context) : base(context) { }

        #region 列表与查询

        public async Task<List<SalPayslipExtend>> GetList(SoulPage<SalPayslipExtend> pagination, string month, string keyword, string status)
        {
            var queryable = repository.Db.Queryable<SalPayslipEntity, UserEntity>((e, u) => new object[] { JoinType.Left, e.F_UserId == u.F_Id })
                .Where((e, u) => e.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(month)) queryable = queryable.Where((e, u) => e.F_Month == month);
            if (!string.IsNullOrEmpty(keyword)) queryable = queryable.Where((e, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword));
            if (!string.IsNullOrEmpty(status)) { var s = int.Parse(status); queryable = queryable.Where((e, u) => e.F_Status == s); }
            queryable = queryable.OrderBy((e, u) => e.F_Month, OrderByType.Desc).OrderBy((e, u) => e.F_CreatorTime, OrderByType.Desc);
            var query = queryable.Select((e, u) => new SalPayslipExtend
            {
                F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_FixedTotal = e.F_FixedTotal, F_PeriodTotal = e.F_PeriodTotal,
                F_HeadTeacherTotal = e.F_HeadTeacherTotal, F_AfterSchoolTotal = e.F_AfterSchoolTotal, F_SpecialTotal = e.F_SpecialTotal,
                F_AdjustmentTotal = e.F_AdjustmentTotal, F_AttendanceDeduct = e.F_AttendanceDeduct, F_SocialPersonal = e.F_SocialPersonal,
                F_UnionFee = e.F_UnionFee, F_Tax = e.F_Tax, F_GrossPay = e.F_GrossPay, F_NetPay = e.F_NetPay, F_Status = e.F_Status,
                F_LockTime = e.F_LockTime, F_CreatorTime = e.F_CreatorTime,
                F_Account = u.F_Account, F_RealName = u.F_RealName, F_OrganizeName = u.F_OrganizeId
            });
            pagination.count = await query.CountAsync();
            return await query.ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<SalPayslipEntity> GetForm(string keyValue) => await repository.Db.Queryable<SalPayslipEntity>().Where(a => a.F_Id == keyValue && a.F_DeleteMark == false).FirstAsync();

        public async Task<List<SalPayslipDetailEntity>> GetDetails(string payslipId) =>
            await repository.Db.Queryable<SalPayslipDetailEntity>().Where(a => a.F_PayslipId == payslipId).OrderBy(a => a.F_SortCode).ToListAsync();

        #endregion

        #region 自动算薪引擎

        /// <summary>
        /// 对某月执行自动算薪：归集各部门数据 → 按规则计算 → 生成工资单+明细
        /// 已归档(状态4)的不重算；其余状态重算覆盖。
        /// </summary>
        public async Task<int> CalculateMonth(string month)
        {
            if (string.IsNullOrEmpty(month)) throw new Exception("请选择薪资月份");
            var rule = await repository.Db.Queryable<SalRuleEntity>().OrderBy(a => a.F_CreatorTime).FirstAsync();
            if (rule == null) throw new Exception("请先配置薪酬规则");

            var profiles = await repository.Db.Queryable<SalProfileEntity>().Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true).ToListAsync();
            if (profiles.Count == 0) throw new Exception("尚无薪酬档案，请先维护教职工档案");

            // 归集当月各部门数据
            var periods = await repository.Db.Queryable<SalPeriodRecordEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();
            var assess = await repository.Db.Queryable<SalHeadTeacherAssessEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();
            var afters = await repository.Db.Queryable<SalAfterSchoolRecordEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();
            var invigilates = await repository.Db.Queryable<SalInvigilateRecordEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();
            var attendances = await repository.Db.Queryable<SalAttendanceSummaryEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();
            var adjustments = await repository.Db.Queryable<SalAdjustmentEntity>().Where(a => a.F_Month == month && a.F_DeleteMark == false).ToListAsync();

            int count = 0;
            foreach (var p in profiles)
            {
                // 已归档锁定的不重算
                var existing = await repository.Db.Queryable<SalPayslipEntity>().Where(a => a.F_Month == month && a.F_UserId == p.F_UserId && a.F_DeleteMark == false).FirstAsync();
                if (existing != null && existing.F_Status == 4) continue;

                var details = new List<SalPayslipDetailEntity>();
                var pd = periods.FirstOrDefault(a => a.F_UserId == p.F_UserId);
                var ad = assess.FirstOrDefault(a => a.UserMatch(p.F_UserId));
                decimal fixedTotal = 0, periodTotal = 0, headTotal = 0, afterTotal = 0, specialTotal = 0, adjustTotal = 0;
                decimal attendDeduct = 0, socialPersonal = 0, unionFee = 0, tax = 0;

                // ---- 固定部分 ----
                // 这里简化为：岗位基本工资+薪级工资+教龄津贴+职称补贴 由档案社保基数推导基础工资项。
                // 实际学校可在调整项里录入固定工资，此处以档案 F_SocialBase 作为月固定工资基数。
                decimal baseSalary = p.F_SocialBase ?? 0;
                if (baseSalary > 0) { details.Add(MakeDetail(month, p.F_UserId, "固定", "岗位基本工资+薪级工资", 1, baseSalary, baseSalary, 1)); fixedTotal += baseSalary; }
                decimal teachingAllowance = (p.F_TeachingYears ?? 0) * 10m; // 教龄津贴：每年10元/月
                if (teachingAllowance > 0) { details.Add(MakeDetail(month, p.F_UserId, "固定", $"教龄津贴({p.F_TeachingYears}年×10元)", p.F_TeachingYears ?? 0, 10, teachingAllowance, 2)); fixedTotal += teachingAllowance; }

                // ---- 课时费 ----
                decimal titleRate = TitleRate(rule, p.F_TitleLevel);
                if (pd != null)
                {
                    decimal normalFee = (pd.F_NormalPeriods ?? 0) * titleRate;
                    if (normalFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课时", $"课时费({pd.F_NormalPeriods}节×{titleRate}元)", pd.F_NormalPeriods, titleRate, normalFee, 10)); periodTotal += normalFee; }
                    decimal overFee = (pd.F_OvertimePeriods ?? 0) * titleRate * (rule.F_OvertimeRate ?? 1.5m);
                    if (overFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课时", $"超课时费({pd.F_OvertimePeriods}节×{titleRate}×{rule.F_OvertimeRate}倍)", pd.F_OvertimePeriods, titleRate * (rule.F_OvertimeRate ?? 1.5m), overFee, 11)); periodTotal += overFee; }
                    decimal subFee = (pd.F_SubstitutePeriods ?? 0) * (rule.F_SubstituteRate ?? 0);
                    if (subFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课时", $"代课费({pd.F_SubstitutePeriods}节×{rule.F_SubstituteRate}元)", pd.F_SubstitutePeriods, rule.F_SubstituteRate, subFee, 12)); periodTotal += subFee; }
                    decimal morningFee = (pd.F_MorningReadCount ?? 0) * (rule.F_MorningReadRate ?? 0);
                    if (morningFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课时", $"早读补贴({pd.F_MorningReadCount}次×{rule.F_MorningReadRate}元)", pd.F_MorningReadCount, rule.F_MorningReadRate, morningFee, 13)); periodTotal += morningFee; }
                    decimal eveningFee = (pd.F_EveningStudyCount ?? 0) * (rule.F_EveningStudyRate ?? 0);
                    if (eveningFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课时", $"晚自习值班({pd.F_EveningStudyCount}次×{rule.F_EveningStudyRate}元)", pd.F_EveningStudyCount, rule.F_EveningStudyRate, eveningFee, 14)); periodTotal += eveningFee; }
                }

                // ---- 班主任津贴 ----
                if (ad != null)
                {
                    decimal tierAmount = ClassTierAmount(rule, ad.F_ClassSize ?? 0);
                    decimal assessRate = ad.F_AssessLevel == 1 ? (rule.F_AssessExcellent ?? 1) : ad.F_AssessLevel == 3 ? (rule.F_AssessPoor ?? 0.6m) : (rule.F_AssessGood ?? 0.9m);
                    decimal headAmount = tierAmount * assessRate;
                    string headName = $"班主任津贴(班级{ad.F_ClassSize}人档{tierAmount}元×考核{assessRate * 100}%)";
                    if (ad.F_IsGraduation == true)
                    {
                        decimal gradBonus = headAmount * (rule.F_GraduationBonusRate ?? 0.3m);
                        headAmount += gradBonus;
                        headName += $"+毕业班加成{gradBonus:0.##}元";
                    }
                    details.Add(MakeDetail(month, p.F_UserId, "班主任", headName, 1, tierAmount, headAmount, 20));
                    headTotal += headAmount;
                }
                // 副班主任津贴（按对应主班津贴比例）
                if (p.F_IsDeputyHead == true && ad == null)
                {
                    // 副班主任无考核记录时按一档基础×比例
                    decimal deputyBase = (rule.F_ClassTier1Amount ?? 0) * (rule.F_DeputyRate ?? 0.3m);
                    if (deputyBase > 0) { details.Add(MakeDetail(month, p.F_UserId, "班主任", $"副班主任津贴(按主班{rule.F_DeputyRate * 100}%)", 1, deputyBase, deputyBase, 21)); headTotal += deputyBase; }
                }

                // ---- 课后服务费 ----
                var myAfters = afters.Where(a => a.F_UserId == p.F_UserId).ToList();
                foreach (var af in myAfters)
                {
                    decimal rate = af.F_ServiceType == 2 ? (rule.F_AfterSchoolClub ?? 0) : af.F_ServiceType == 3 ? (rule.F_AfterSchoolCare ?? 0) : (rule.F_AfterSchoolHomework ?? 0);
                    string typeName = af.F_ServiceType == 2 ? "社团活动" : af.F_ServiceType == 3 ? "延时托管" : "作业辅导";
                    decimal fee = (af.F_Periods ?? 0) * rate;
                    if (fee > 0) { details.Add(MakeDetail(month, p.F_UserId, "课后服务", $"课后服务-{typeName}({af.F_Periods}节×{rate}元)", af.F_Periods, rate, fee, 30)); afterTotal += fee; }
                }

                // ---- 专项津贴 ----
                if (p.F_IsGroupLeader == true) { var v = rule.F_GroupLeaderAllowance ?? 0; if (v > 0) { details.Add(MakeDetail(month, p.F_UserId, "专项", "教研组长津贴", 1, v, v, 40)); specialTotal += v; } }
                if (p.F_IsLessonLeader == true) { var v = rule.F_LessonLeaderAllowance ?? 0; if (v > 0) { details.Add(MakeDetail(month, p.F_UserId, "专项", "备课组长津贴", 1, v, v, 41)); specialTotal += v; } }
                var myInv = invigilates.Where(a => a.F_UserId == p.F_UserId).ToList();
                foreach (var iv in myInv)
                {
                    decimal fee = (iv.F_Sessions ?? 0) * (rule.F_InvigilateRate ?? 0);
                    if (fee > 0) { details.Add(MakeDetail(month, p.F_UserId, "专项", $"监考费-{iv.F_ExamName}({iv.F_Sessions}场×{rule.F_InvigilateRate}元)", iv.F_Sessions, rule.F_InvigilateRate, fee, 42)); specialTotal += fee; }
                }

                // ---- 手动调整 ----
                var myAdj = adjustments.Where(a => a.F_UserId == p.F_UserId).ToList();
                foreach (var aj in myAdj)
                {
                    details.Add(MakeDetail(month, p.F_UserId, "调整", aj.F_ItemName + (string.IsNullOrEmpty(aj.F_Reason) ? "" : $"({aj.F_Reason})"), 1, aj.F_Amount, aj.F_Amount ?? 0, 50));
                    adjustTotal += aj.F_Amount ?? 0;
                }

                // ---- 考勤扣款 ----
                var att = attendances.FirstOrDefault(a => a.F_UserId == p.F_UserId);
                if (att != null)
                {
                    decimal d = 0;
                    decimal sick = (att.F_SickLeaveDays ?? 0) * (rule.F_SickLeaveDeduct ?? 0); if (sick > 0) { details.Add(MakeDetail(month, p.F_UserId, "考勤扣款", $"病假扣款({att.F_SickLeaveDays}天×{rule.F_SickLeaveDeduct}元)", att.F_SickLeaveDays, rule.F_SickLeaveDeduct, -sick, 60)); d += sick; }
                    decimal personal = (att.F_PersonalLeaveDays ?? 0) * (rule.F_PersonalLeaveDeduct ?? 0); if (personal > 0) { details.Add(MakeDetail(month, p.F_UserId, "考勤扣款", $"事假扣款({att.F_PersonalLeaveDays}天×{rule.F_PersonalLeaveDeduct}元)", att.F_PersonalLeaveDays, rule.F_PersonalLeaveDeduct, -personal, 61)); d += personal; }
                    decimal absent = (att.F_AbsentDays ?? 0) * (rule.F_AbsentDeduct ?? 0); if (absent > 0) { details.Add(MakeDetail(month, p.F_UserId, "考勤扣款", $"旷工扣款({att.F_AbsentDays}天×{rule.F_AbsentDeduct}元)", att.F_AbsentDays, rule.F_AbsentDeduct, -absent, 62)); d += absent; }
                    decimal late = ((att.F_LateCount ?? 0) + (att.F_EarlyCount ?? 0)) * (rule.F_LateDeduct ?? 0); if (late > 0) { details.Add(MakeDetail(month, p.F_UserId, "考勤扣款", $"迟到早退({(att.F_LateCount ?? 0) + (att.F_EarlyCount ?? 0)}次×{rule.F_LateDeduct}元)", (att.F_LateCount ?? 0) + (att.F_EarlyCount ?? 0), rule.F_LateDeduct, -late, 63)); d += late; }
                    attendDeduct = d;
                }

                // ---- 社保公积金/工会 ----
                decimal socialBase = p.F_SocialBase ?? 0;
                if (socialBase > 0)
                {
                    decimal social = Math.Round(socialBase * (rule.F_SocialRate ?? 0.105m), 2);
                    decimal fund = Math.Round(socialBase * (rule.F_HousingFundRate ?? 0.12m), 2);
                    socialPersonal = social + fund;
                    details.Add(MakeDetail(month, p.F_UserId, "社保", $"社保个人部分(基数{socialBase}×{rule.F_SocialRate * 100}%)", 1, social, -social, 70));
                    details.Add(MakeDetail(month, p.F_UserId, "社保", $"公积金个人部分(基数{socialBase}×{rule.F_HousingFundRate * 100}%)", 1, fund, -fund, 71));
                    unionFee = Math.Round(socialBase * (rule.F_UnionFeeRate ?? 0.005m), 2);
                    if (unionFee > 0) { details.Add(MakeDetail(month, p.F_UserId, "社保", $"工会会费(基数{socialBase}×{rule.F_UnionFeeRate * 100}%)", 1, unionFee, -unionFee, 72)); }
                }

                // ---- 应发/个税/实发 ----
                decimal gross = fixedTotal + periodTotal + headTotal + afterTotal + specialTotal + adjustTotal - attendDeduct;
                // 累计预扣法算个税
                tax = await CalcMonthlyTax(p, month, gross, socialPersonal + unionFee);
                if (tax > 0) { details.Add(MakeDetail(month, p.F_UserId, "个税", $"个人所得税(累计预扣)", 1, tax, -tax, 80)); }
                decimal net = gross - socialPersonal - unionFee - tax;

                // ---- 落库 ----
                string payslipId;
                if (existing == null)
                {
                    payslipId = Utils.GuId();
                    var slip = new SalPayslipEntity
                    {
                        F_Id = payslipId, F_Month = month, F_UserId = p.F_UserId,
                        F_FixedTotal = fixedTotal, F_PeriodTotal = periodTotal, F_HeadTeacherTotal = headTotal,
                        F_AfterSchoolTotal = afterTotal, F_SpecialTotal = specialTotal, F_AdjustmentTotal = adjustTotal,
                        F_AttendanceDeduct = attendDeduct, F_SocialPersonal = socialPersonal, F_UnionFee = unionFee,
                        F_Tax = tax, F_GrossPay = gross, F_NetPay = net, F_Status = 0,
                        F_DeleteMark = false, F_CreatorTime = DateTime.Now, F_CreatorUserId = currentuser.UserId
                    };
                    await repository.Insert(slip);
                }
                else
                {
                    payslipId = existing.F_Id;
                    existing.F_FixedTotal = fixedTotal; existing.F_PeriodTotal = periodTotal; existing.F_HeadTeacherTotal = headTotal;
                    existing.F_AfterSchoolTotal = afterTotal; existing.F_SpecialTotal = specialTotal; existing.F_AdjustmentTotal = adjustTotal;
                    existing.F_AttendanceDeduct = attendDeduct; existing.F_SocialPersonal = socialPersonal; existing.F_UnionFee = unionFee;
                    existing.F_Tax = tax; existing.F_GrossPay = gross; existing.F_NetPay = net;
                    existing.F_Status = 0; existing.F_LastModifyTime = DateTime.Now; existing.F_LastModifyUserId = currentuser.UserId;
                    await repository.Db.Updateable(existing).Where(a => a.F_Id == existing.F_Id).ExecuteCommandAsync();
                    await repository.Db.Deleteable<SalPayslipDetailEntity>().Where(a => a.F_PayslipId == payslipId).ExecuteCommandAsync();
                }
                foreach (var d in details) { d.F_Id = Utils.GuId(); d.F_PayslipId = payslipId; d.F_CreatorTime = DateTime.Now; }
                if (details.Count > 0) await repository.Db.Insertable(details).ExecuteCommandAsync();
                await WriteLog(month, p.F_UserId, "自动算薪", $"应发{gross}元 实发{net}元");
                count++;
            }
            return count;
        }

        private SalPayslipDetailEntity MakeDetail(string month, string userId, string category, string name, decimal? qty, decimal? price, decimal amount, int sort)
            => new SalPayslipDetailEntity { F_Month = month, F_UserId = userId, F_Category = category, F_ItemName = name, F_Quantity = qty, F_UnitPrice = price, F_Amount = amount, F_SortCode = sort };

        private decimal TitleRate(SalRuleEntity rule, int? titleLevel)
            => titleLevel == 4 ? (rule.F_RateTitle4 ?? 100) : titleLevel == 3 ? (rule.F_RateTitle3 ?? 80) : titleLevel == 1 ? (rule.F_RateTitle1 ?? 50) : (rule.F_RateTitle2 ?? 60);

        private decimal ClassTierAmount(SalRuleEntity rule, int classSize)
        {
            if (classSize <= (rule.F_ClassTier1Max ?? 40)) return rule.F_ClassTier1Amount ?? 500;
            if (classSize <= (rule.F_ClassTier2Max ?? 50)) return rule.F_ClassTier2Amount ?? 600;
            return rule.F_ClassTier3Amount ?? 700;
        }

        /// <summary>累计预扣法计算当月个税</summary>
        private async Task<decimal> CalcMonthlyTax(SalProfileEntity p, string month, decimal gross, decimal socialUnion)
        {
            // 解析年月，计算当年已累计月份
            if (!DateTime.TryParse(month + "-01", out var mDate)) return 0;
            int year = mDate.Year, mon = mDate.Month;
            // 累计应纳税所得额 = Σ(累计收入 - 累计免税5000×月数 - 累计专项扣除(社保工会) - 累计专项附加扣除)
            // 简化：用当年1月至本月各月已归档工资单的应发做累计，当前月用本次 gross
            var yearStart = year + "-01";
            var priorSlips = await repository.Db.Queryable<SalPayslipEntity>()
                .Where(a => a.F_UserId == p.F_UserId && a.F_DeleteMark == false && SqlFunc.Substring(a.F_Month, 0, 4) == year.ToString() && a.F_Month != month)
                .Select(a => new { a.F_Month, a.F_GrossPay, a.F_SocialPersonal, a.F_UnionFee }).ToListAsync();
            decimal cumIncome = gross, cumDeduct = socialUnion + (p.F_TaxDeduction ?? 0);
            foreach (var s in priorSlips)
            {
                cumIncome += s.F_GrossPay ?? 0;
                cumDeduct += (s.F_SocialPersonal ?? 0) + (s.F_UnionFee ?? 0) + (p.F_TaxDeduction ?? 0);
            }
            int months = priorSlips.Count + 1;
            decimal cumTaxFree = 5000m * months;
            decimal cumTaxable = cumIncome - cumTaxFree - cumDeduct;
            if (cumTaxable <= 0) return 0;
            // 累计预扣率表（年度）
            (decimal threshold, decimal rate, decimal quick)[] brackets = new[]
            {
                (36000m, 0.03m, 0m), (144000m, 0.10m, 2520m), (300000m, 0.20m, 16920m),
                (420000m, 0.25m, 31920m), (660000m, 0.30m, 52920m), (960000m, 0.35m, 85920m), (decimal.MaxValue, 0.45m, 181920m)
            };
            var b = brackets.First(x => cumTaxable <= x.threshold);
            decimal cumTax = cumTaxable * b.rate - b.quick;
            // 已预缴税额 = 之前月份已扣个税合计
            decimal paidTax = priorSlips.Count == 0 ? 0 : await repository.Db.Queryable<SalPayslipEntity>()
                .Where(a => a.F_UserId == p.F_UserId && a.F_DeleteMark == false && SqlFunc.Substring(a.F_Month, 0, 4) == year.ToString() && a.F_Month != month)
                .SumAsync(a => a.F_Tax) ?? 0;
            decimal tax = Math.Max(0, Math.Round(cumTax - paidTax, 2));
            return tax;
        }

        #endregion

        #region 审批流转

        /// <summary>人事提交审批（草稿/驳回 → 1）</summary>
        public async Task SubmitApprove(string keyValue)
        {
            var slip = await GetForm(keyValue); if (slip == null) throw new Exception("工资单不存在");
            if (slip.F_Status != 0 && slip.F_Status != 5) throw new Exception("当前状态不允许提交");
            slip.F_Status = 1; slip.F_SubmitTime = DateTime.Now; slip.F_SubmitUserId = currentuser.UserId; slip.F_LastModifyTime = DateTime.Now;
            await repository.Db.Updateable(slip).UpdateColumns(a => new { a.F_Status, a.F_SubmitTime, a.F_SubmitUserId, a.F_LastModifyTime }).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
            await WriteLog(slip.F_Month, slip.F_UserId, "人事提交", $"工资单提交进入审批");
        }

        /// <summary>逐级审批：1→2(教务)→3(财务)→4(校长,归档锁定)</summary>
        public async Task Approve(string keyValue, string remark)
        {
            var slip = await GetForm(keyValue); if (slip == null) throw new Exception("工资单不存在");
            int cur = slip.F_Status ?? 0;
            if (cur < 1 || cur > 3) throw new Exception("当前状态不允许审批");
            int next = cur + 1;
            slip.F_Status = next; slip.F_ApproveRemark = remark; slip.F_LastModifyTime = DateTime.Now;
            if (next == 4) slip.F_LockTime = DateTime.Now; // 校长审批后归档锁定
            await repository.Db.Updateable(slip).UpdateColumns(a => new { a.F_Status, a.F_ApproveRemark, a.F_LockTime, a.F_LastModifyTime }).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
            string action = next == 2 ? "教务核对" : next == 3 ? "财务核对" : "校长审批(归档)";
            await WriteLog(slip.F_Month, slip.F_UserId, action, remark ?? "");
        }

        /// <summary>驳回（回到草稿 0）</summary>
        public async Task Reject(string keyValue, string remark)
        {
            var slip = await GetForm(keyValue); if (slip == null) throw new Exception("工资单不存在");
            if (slip.F_Status == 4) throw new Exception("已归档锁定的工资单不能驳回");
            slip.F_Status = 0; slip.F_ApproveRemark = remark; slip.F_LastModifyTime = DateTime.Now;
            await repository.Db.Updateable(slip).UpdateColumns(a => new { a.F_Status, a.F_ApproveRemark, a.F_LastModifyTime }).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
            await WriteLog(slip.F_Month, slip.F_UserId, "驳回", remark ?? "");
        }

        #endregion

        #region 报表导出

        /// <summary>某月全部工资单（联查用户，报表用）</summary>
        public async Task<List<SalPayslipExtend>> GetAllByMonth(string month)
        {
            var query = repository.Db.Queryable<SalPayslipEntity, UserEntity>((e, u) => new object[] { JoinType.Left, e.F_UserId == u.F_Id })
                .Where((e, u) => e.F_DeleteMark == false && e.F_Month == month)
                .OrderBy((e, u) => u.F_Account)
                .Select((e, u) => new SalPayslipExtend
                {
                    F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_FixedTotal = e.F_FixedTotal, F_PeriodTotal = e.F_PeriodTotal,
                    F_HeadTeacherTotal = e.F_HeadTeacherTotal, F_AfterSchoolTotal = e.F_AfterSchoolTotal, F_SpecialTotal = e.F_SpecialTotal,
                    F_AdjustmentTotal = e.F_AdjustmentTotal, F_AttendanceDeduct = e.F_AttendanceDeduct, F_SocialPersonal = e.F_SocialPersonal,
                    F_UnionFee = e.F_UnionFee, F_Tax = e.F_Tax, F_GrossPay = e.F_GrossPay, F_NetPay = e.F_NetPay, F_Status = e.F_Status,
                    F_Account = u.F_Account, F_RealName = u.F_RealName
                });
            return await query.ToListAsync();
        }

        /// <summary>某月某类别的工资明细（联查用户姓名，报表用）</summary>
        public async Task<List<SalPayslipDetailExtend>> GetDetailsByMonthCategory(string month, string category)
        {
            var queryable = repository.Db.Queryable<SalPayslipDetailEntity, UserEntity>((d, u) => new object[] { JoinType.Left, d.F_UserId == u.F_Id })
                .Where((d, u) => d.F_Month == month);
            if (!string.IsNullOrEmpty(category)) queryable = queryable.Where((d, u) => d.F_Category == category);
            var query = queryable
                .OrderBy((d, u) => u.F_Account).OrderBy((d, u) => d.F_SortCode)
                .Select((d, u) => new SalPayslipDetailExtend
                {
                    F_Id = d.F_Id, F_PayslipId = d.F_PayslipId, F_Month = d.F_Month, F_UserId = d.F_UserId, F_Category = d.F_Category,
                    F_ItemName = d.F_ItemName, F_Quantity = d.F_Quantity, F_UnitPrice = d.F_UnitPrice, F_Amount = d.F_Amount, F_SortCode = d.F_SortCode,
                    F_Account = u.F_Account, F_RealName = u.F_RealName
                });
            return await query.ToListAsync();
        }

        #endregion

        #region 我的工资条 / 申诉

        public async Task<List<SalPayslipEntity>> GetMyPayslips(SoulPage<SalPayslipEntity> pagination)
        {
            var userId = currentuser.UserId;
            var q = repository.Db.Queryable<SalPayslipEntity>().Where(a => a.F_UserId == userId && a.F_DeleteMark == false).OrderBy(a => a.F_Month, OrderByType.Desc);
            pagination.count = await q.CountAsync();
            return await q.ToPageListAsync(pagination.page, pagination.rows);
        }

        #endregion

        #region 审计日志

        public async Task WriteLog(string month, string userId, string action, string detail)
        {
            var log = new SalAuditLogEntity { F_Id = Utils.GuId(), F_Month = month, F_UserId = userId, F_Action = action, F_Detail = detail, F_OperatorId = currentuser.UserId, F_CreatorTime = DateTime.Now };
            await repository.Db.Insertable(log).ExecuteCommandAsync();
        }

        public async Task<List<SalAuditLogExtend>> GetAuditLogs(SoulPage<SalAuditLogExtend> pagination, string month, string keyword)
        {
            var queryable = repository.Db.Queryable<SalAuditLogEntity, UserEntity>((e, u) => new object[] { JoinType.Left, e.F_UserId == u.F_Id })
                .Where((e, u) => true);
            if (!string.IsNullOrEmpty(month)) queryable = queryable.Where((e, u) => e.F_Month == month);
            if (!string.IsNullOrEmpty(keyword)) queryable = queryable.Where((e, u) => u.F_RealName.Contains(keyword) || e.F_Action.Contains(keyword));
            queryable = queryable.OrderBy((e, u) => e.F_CreatorTime, OrderByType.Desc);
            var query = queryable.Select((e, u) => new SalAuditLogExtend { F_Id = e.F_Id, F_Month = e.F_Month, F_UserId = e.F_UserId, F_Action = e.F_Action, F_Detail = e.F_Detail, F_OperatorId = e.F_OperatorId, F_CreatorTime = e.F_CreatorTime, F_RealName = u.F_RealName, F_OperatorName = e.F_OperatorId });
            pagination.count = await query.CountAsync();
            var list = await query.ToPageListAsync(pagination.page, pagination.rows);
            // 解析操作人姓名
            var opIds = list.Where(a => !string.IsNullOrEmpty(a.F_OperatorName)).Select(a => a.F_OperatorName).Distinct().ToList();
            if (opIds.Count > 0)
            {
                var users = await repository.Db.Queryable<UserEntity>().Where(a => opIds.Contains(a.F_Id)).Select(a => new { a.F_Id, a.F_RealName }).ToListAsync();
                var dict = users.ToDictionary(a => a.F_Id, a => a.F_RealName);
                foreach (var item in list) item.F_OperatorName = dict.ContainsKey(item.F_OperatorName ?? "") ? dict[item.F_OperatorName] : "";
            }
            return list;
        }

        #endregion
    }

    /// <summary>扩展：考核记录按用户匹配</summary>
    internal static class SalAssessExt
    {
        public static bool UserMatch(this SalHeadTeacherAssessEntity e, string userId) => e.F_UserId == userId;
    }
}
