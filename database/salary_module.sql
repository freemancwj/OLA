-- =============================================================
-- 薪酬管理模块 初始化脚本（幂等，可重复执行）
-- 内容：sal_* 12 张表建表；薪酬管理一级菜单及各子菜单、按钮；角色授权
-- 注意：所有字符串列显式 COLLATE Chinese_PRC_CI_AS，避免与主系统老表联表冲突
-- =============================================================

-- -------------------------------------------------------------
-- 1. 教职工薪酬档案（一人一档，扩展 hr_employee）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_profile')
BEGIN
	CREATE TABLE [dbo].[sal_profile] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,        -- 关联 sys_user
		[F_JobNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,     -- 工号
		[F_EntryDate] datetime NULL,                                    -- 入职时间
		[F_TeachingYears] int NULL DEFAULT 0,                           -- 教龄（年）
		[F_PostType] int NULL DEFAULT 1,                                -- 岗位属性 1专任教师 2行政 3后勤 4校医 5临聘代课
		[F_TitleLevel] int NULL DEFAULT 2,                              -- 职称 1二级 2一级 3高级 4正高级
		[F_IsHeadTeacher] tinyint NULL DEFAULT 0,                       -- 是否班主任
		[F_IsDeputyHead] tinyint NULL DEFAULT 0,                        -- 是否副班主任
		[F_IsGroupLeader] tinyint NULL DEFAULT 0,                       -- 是否教研组长
		[F_IsLessonLeader] tinyint NULL DEFAULT 0,                      -- 是否备课组长
		[F_IsGraduationTeacher] tinyint NULL DEFAULT 0,                 -- 是否毕业班任课
		[F_ClassId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,        -- 班主任所在班级（sys_organize）
		[F_SocialBase] decimal(18,2) NULL DEFAULT 0,                    -- 社保公积金缴纳基数
		[F_TaxDeduction] decimal(18,2) NULL DEFAULT 0,                  -- 个税专项附加扣除/月
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteTime] datetime NULL,
		[F_DeleteUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_profile] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_profile_F_UserId] ON [dbo].[sal_profile] ([F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 2. 薪酬规则参数（单行配置，所有单价可调）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_rule')
BEGIN
	CREATE TABLE [dbo].[sal_rule] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		-- 课时费职称单价（元/节）
		[F_RateTitle1] decimal(18,2) NULL DEFAULT 50,   -- 二级
		[F_RateTitle2] decimal(18,2) NULL DEFAULT 60,   -- 一级
		[F_RateTitle3] decimal(18,2) NULL DEFAULT 80,   -- 高级
		[F_RateTitle4] decimal(18,2) NULL DEFAULT 100,  -- 正高级
		[F_StandardWeeklyPeriods] int NULL DEFAULT 12,  -- 周标准课时数
		[F_OvertimeRate] decimal(5,2) NULL DEFAULT 1.5, -- 超课时倍率
		[F_MorningReadRate] decimal(18,2) NULL DEFAULT 20,   -- 早读补贴/次
		[F_EveningStudyRate] decimal(18,2) NULL DEFAULT 30,  -- 晚自习值班/次
		[F_SubstituteRate] decimal(18,2) NULL DEFAULT 50,    -- 代课费/节
		-- 班主任津贴分档
		[F_ClassTier1Max] int NULL DEFAULT 40,          -- 一档人数上限
		[F_ClassTier1Amount] decimal(18,2) NULL DEFAULT 500,
		[F_ClassTier2Max] int NULL DEFAULT 50,
		[F_ClassTier2Amount] decimal(18,2) NULL DEFAULT 600,
		[F_ClassTier3Amount] decimal(18,2) NULL DEFAULT 700,  -- 超过二档上限
		[F_GraduationBonusRate] decimal(5,2) NULL DEFAULT 0.3,-- 毕业班班主任加发比例
		[F_DeputyRate] decimal(5,2) NULL DEFAULT 0.3,         -- 副班主任津贴比例
		-- 班主任考核发放比例
		[F_AssessExcellent] decimal(5,2) NULL DEFAULT 1.0,    -- 优秀
		[F_AssessGood] decimal(5,2) NULL DEFAULT 0.9,         -- 合格
		[F_AssessPoor] decimal(5,2) NULL DEFAULT 0.6,         -- 不合格
		-- 课后服务费单价（元/节）
		[F_AfterSchoolHomework] decimal(18,2) NULL DEFAULT 40,   -- 第一时段作业辅导
		[F_AfterSchoolClub] decimal(18,2) NULL DEFAULT 60,       -- 第二时段社团活动
		[F_AfterSchoolCare] decimal(18,2) NULL DEFAULT 30,       -- 延时托管值班
		-- 其他专项津贴
		[F_GroupLeaderAllowance] decimal(18,2) NULL DEFAULT 200, -- 教研组长/月
		[F_LessonLeaderAllowance] decimal(18,2) NULL DEFAULT 100,-- 备课组长/月
		[F_InvigilateRate] decimal(18,2) NULL DEFAULT 100,       -- 监考费/场次
		[F_DutyRate] decimal(18,2) NULL DEFAULT 50,              -- 节假日值班/次
		-- 考勤扣款规则（元/次或元/天）
		[F_LateDeduct] decimal(18,2) NULL DEFAULT 20,     -- 迟到早退/次
		[F_SickLeaveDeduct] decimal(18,2) NULL DEFAULT 50,-- 病假/天
		[F_PersonalLeaveDeduct] decimal(18,2) NULL DEFAULT 100, -- 事假/天
		[F_AbsentDeduct] decimal(18,2) NULL DEFAULT 200,  -- 旷工/天
		-- 社保公积金个人部分比例（占基数）
		[F_SocialRate] decimal(5,4) NULL DEFAULT 0.105,   -- 养老8%+医疗2%+失业0.5%
		[F_HousingFundRate] decimal(5,4) NULL DEFAULT 0.12, -- 公积金12%
		[F_UnionFeeRate] decimal(5,4) NULL DEFAULT 0.005, -- 工会会费0.5%
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_rule] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 3. 课时记录（教务按月录入）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_period_record')
BEGIN
	CREATE TABLE [dbo].[sal_period_record] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,      -- 薪资月份 yyyy-MM
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_NormalPeriods] decimal(10,1) NULL DEFAULT 0,            -- 正常课时数
		[F_OvertimePeriods] decimal(10,1) NULL DEFAULT 0,          -- 超课时数
		[F_SubstitutePeriods] decimal(10,1) NULL DEFAULT 0,        -- 代课节数
		[F_MorningReadCount] int NULL DEFAULT 0,                   -- 早读次数
		[F_EveningStudyCount] int NULL DEFAULT 0,                  -- 晚自习次数
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_period_record] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_period_F_Month_UserId] ON [dbo].[sal_period_record] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 4. 班主任考核记录（德育处按月录入）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_headteacher_assess')
BEGIN
	CREATE TABLE [dbo].[sal_headteacher_assess] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ClassId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,   -- 班级
		[F_ClassSize] int NULL DEFAULT 0,                          -- 班级人数
		[F_IsGraduation] tinyint NULL DEFAULT 0,                   -- 是否毕业班
		[F_AssessLevel] int NULL DEFAULT 2,                        -- 考核 1优秀 2合格 3不合格
		[F_Score] decimal(10,1) NULL,                              -- 班级量化评分
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_headteacher_assess] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_assess_F_Month_UserId] ON [dbo].[sal_headteacher_assess] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 5. 课后服务记录（课后服务管理员按月录入）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_afterschool_record')
BEGIN
	CREATE TABLE [dbo].[sal_afterschool_record] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ServiceType] int NULL DEFAULT 1,    -- 1作业辅导 2社团指导 3延时托管
		[F_Periods] decimal(10,1) NULL DEFAULT 0,  -- 节数/次数
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_afterschool_record] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_afterschool_F_Month_UserId] ON [dbo].[sal_afterschool_record] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 6. 监考记录
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_invigilate_record')
BEGIN
	CREATE TABLE [dbo].[sal_invigilate_record] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ExamName] nvarchar(100) COLLATE Chinese_PRC_CI_AS NULL, -- 考试名称（期中/期末/中考/高考）
		[F_Sessions] int NULL DEFAULT 0,                            -- 监考场次
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_invigilate_record] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_invigilate_F_Month_UserId] ON [dbo].[sal_invigilate_record] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 7. 考勤汇总（按月，从请假/打卡归集）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_attendance_summary')
BEGIN
	CREATE TABLE [dbo].[sal_attendance_summary] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_SickLeaveDays] decimal(10,1) NULL DEFAULT 0,     -- 病假天数
		[F_PersonalLeaveDays] decimal(10,1) NULL DEFAULT 0, -- 事假天数
		[F_AbsentDays] decimal(10,1) NULL DEFAULT 0,        -- 旷工天数
		[F_LateCount] int NULL DEFAULT 0,                   -- 迟到次数
		[F_EarlyCount] int NULL DEFAULT 0,                  -- 早退次数
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_attendance_summary] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_attendance_F_Month_UserId] ON [dbo].[sal_attendance_summary] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 8. 薪酬手动调整项（人事微调，留痕）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_adjustment')
BEGIN
	CREATE TABLE [dbo].[sal_adjustment] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ItemName] nvarchar(100) COLLATE Chinese_PRC_CI_AS NULL,  -- 调整项名称（如临时奖金/补发课时）
		[F_Amount] decimal(18,2) NULL DEFAULT 0,                    -- 金额（正为加，负为扣）
		[F_Reason] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_adjustment] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_adjustment_F_Month_UserId] ON [dbo].[sal_adjustment] ([F_Month] ASC, [F_UserId] ASC)
END
GO

-- -------------------------------------------------------------
-- 9. 工资单（每月每人一条，带审批状态）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_payslip')
BEGIN
	CREATE TABLE [dbo].[sal_payslip] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		-- 汇总金额
		[F_FixedTotal] decimal(18,2) NULL DEFAULT 0,        -- 固定部分合计
		[F_PeriodTotal] decimal(18,2) NULL DEFAULT 0,       -- 课时费合计
		[F_HeadTeacherTotal] decimal(18,2) NULL DEFAULT 0,  -- 班主任津贴
		[F_AfterSchoolTotal] decimal(18,2) NULL DEFAULT 0,  -- 课后服务费
		[F_SpecialTotal] decimal(18,2) NULL DEFAULT 0,      -- 专项津贴合计
		[F_AdjustmentTotal] decimal(18,2) NULL DEFAULT 0,   -- 手动调整合计
		[F_AttendanceDeduct] decimal(18,2) NULL DEFAULT 0,  -- 考勤扣款
		[F_SocialPersonal] decimal(18,2) NULL DEFAULT 0,    -- 社保公积金个人部分
		[F_UnionFee] decimal(18,2) NULL DEFAULT 0,          -- 工会会费
		[F_Tax] decimal(18,2) NULL DEFAULT 0,               -- 个人所得税
		[F_GrossPay] decimal(18,2) NULL DEFAULT 0,          -- 应发工资
		[F_NetPay] decimal(18,2) NULL DEFAULT 0,            -- 实发工资
		-- 审批状态 0草稿 1人事提交 2教务已核 3财务已核 4校长已批(归档锁定) 5驳回
		[F_Status] int NULL DEFAULT 0,
		[F_SubmitTime] datetime NULL,
		[F_SubmitUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ApproveRemark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LockTime] datetime NULL,                          -- 归档锁定时间
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sal_payslip] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE UNIQUE INDEX [IX_sal_payslip_F_Month_UserId] ON [dbo].[sal_payslip] ([F_Month] ASC, [F_UserId] ASC) WHERE [F_DeleteMark] = 0
END
GO

-- -------------------------------------------------------------
-- 10. 工资单明细（每一笔钱的来源）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_payslip_detail')
BEGIN
	CREATE TABLE [dbo].[sal_payslip_detail] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_PayslipId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,  -- 关联 sal_payslip
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Category] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,   -- 分类：固定/课时/班主任/课后服务/专项/调整/考勤扣款/社保/个税
		[F_ItemName] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,  -- 明细名称（如 课时费(20节×80元)）
		[F_Quantity] decimal(18,2) NULL,                            -- 数量
		[F_UnitPrice] decimal(18,2) NULL,                           -- 单价
		[F_Amount] decimal(18,2) NULL DEFAULT 0,                    -- 金额（正为收入，负为扣款）
		[F_SortCode] int NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		CONSTRAINT [PK_sal_payslip_detail] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_detail_F_PayslipId] ON [dbo].[sal_payslip_detail] ([F_PayslipId] ASC)
END
GO

-- -------------------------------------------------------------
-- 11. 薪酬申诉
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_appeal')
BEGIN
	CREATE TABLE [dbo].[sal_appeal] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_PayslipId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,      -- 申诉人
		[F_Content] nvarchar(1000) COLLATE Chinese_PRC_CI_AS NULL,   -- 申诉内容
		[F_Status] int NULL DEFAULT 0,                               -- 0待处理 1已处理
		[F_Reply] nvarchar(1000) COLLATE Chinese_PRC_CI_AS NULL,     -- 人事回复
		[F_ReplyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ReplyTime] datetime NULL,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_LastModifyTime] datetime NULL,
		CONSTRAINT [PK_sal_appeal] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_appeal_F_Month] ON [dbo].[sal_appeal] ([F_Month] ASC)
END
GO

-- -------------------------------------------------------------
-- 12. 薪酬操作审计日志
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sal_audit_log')
BEGIN
	CREATE TABLE [dbo].[sal_audit_log] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Month] nvarchar(7) COLLATE Chinese_PRC_CI_AS NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,      -- 涉及的老师
		[F_Action] nvarchar(100) COLLATE Chinese_PRC_CI_AS NULL,     -- 操作（算薪/调整/提交/审批/归档/驳回）
		[F_Detail] nvarchar(2000) COLLATE Chinese_PRC_CI_AS NULL,    -- 明细
		[F_OperatorId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,  -- 操作人
		[F_CreatorTime] datetime NULL,
		CONSTRAINT [PK_sal_audit_log] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_sal_audit_F_Month] ON [dbo].[sal_audit_log] ([F_Month] ASC)
END
GO

-- =============================================================
-- 菜单与授权
-- =============================================================

-- 一级菜单：薪酬管理（SortCode=6）
DECLARE @salMenuId nvarchar(50) = 'c0a70001-0000-4000-8000-000000000001'
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @salMenuId)
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (@salMenuId, '87dc2de3-ccbc-4dab-bb90-89fc68cbde4f', 2, N'SalaryManagement', N'薪酬管理', N'fa fa-money', NULL, N'expand', 1, 1, 0, 0, 0, 0, 6, 0, 1, N'教职工薪酬管理', GETDATE())
END
GO

-- 子菜单
DECLARE @salMenuId nvarchar(50) = 'c0a70001-0000-4000-8000-000000000001'
DECLARE @sub TABLE (Id nvarchar(50) COLLATE Chinese_PRC_CI_AS, EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, Name nvarchar(50) COLLATE Chinese_PRC_CI_AS, Icon nvarchar(50) COLLATE Chinese_PRC_CI_AS, Url nvarchar(200) COLLATE Chinese_PRC_CI_AS, Sort int)
INSERT INTO @sub VALUES
 ('c0a70002-0000-4000-8000-000000000001', N'SalProfile', N'薪酬档案', N'fa fa-id-card', N'/SalaryManagement/SalProfile/Index', 1),
 ('c0a70003-0000-4000-8000-000000000001', N'SalRule', N'薪酬规则配置', N'fa fa-cogs', N'/SalaryManagement/SalRule/Index', 2),
 ('c0a70004-0000-4000-8000-000000000001', N'SalPeriodRecord', N'课时记录', N'fa fa-book', N'/SalaryManagement/SalPeriodRecord/Index', 3),
 ('c0a70005-0000-4000-8000-000000000001', N'SalHeadTeacherAssess', N'班主任考核', N'fa fa-star', N'/SalaryManagement/SalHeadTeacherAssess/Index', 4),
 ('c0a70006-0000-4000-8000-000000000001', N'SalAfterSchoolRecord', N'课后服务记录', N'fa fa-clock-o', N'/SalaryManagement/SalAfterSchoolRecord/Index', 5),
 ('c0a70007-0000-4000-8000-000000000001', N'SalInvigilateRecord', N'监考记录', N'fa fa-edit', N'/SalaryManagement/SalInvigilateRecord/Index', 6),
 ('c0a70008-0000-4000-8000-000000000001', N'SalAttendanceSummary', N'考勤汇总', N'fa fa-calendar-check-o', N'/SalaryManagement/SalAttendanceSummary/Index', 7),
 ('c0a70009-0000-4000-8000-000000000001', N'SalAdjustment', N'薪酬调整', N'fa fa-sliders', N'/SalaryManagement/SalAdjustment/Index', 8),
 ('c0a7000a-0000-4000-8000-000000000001', N'SalPayslip', N'工资单管理', N'fa fa-file-text', N'/SalaryManagement/SalPayslip/Index', 9),
 ('c0a7000b-0000-4000-8000-000000000001', N'SalAppeal', N'薪酬申诉', N'fa fa-comment', N'/SalaryManagement/SalAppeal/Index', 10),
 ('c0a7000c-0000-4000-8000-000000000001', N'SalReport', N'薪酬报表', N'fa fa-bar-chart', N'/SalaryManagement/SalReport/Index', 11),
 ('c0a7000d-0000-4000-8000-000000000001', N'SalMyPayslip', N'我的工资条', N'fa fa-money', N'/SalaryManagement/SalMyPayslip/Index', 12)

INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
SELECT s.Id, @salMenuId, 3, s.EnCode, s.Name, s.Icon, s.Url, N'iframe', 1, 0, 0, 0, 0, 0, s.Sort, 0, 1, s.Name, GETDATE()
FROM @sub s
WHERE NOT EXISTS (SELECT 1 FROM sys_module x WHERE x.F_Id = s.Id)
GO

-- 各管理菜单的按钮（增/改/删）
DECLARE @btnModules TABLE (ModuleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @btnModules VALUES
 ('c0a70002-0000-4000-8000-000000000001'), -- SalProfile
 ('c0a70004-0000-4000-8000-000000000001'), -- SalPeriodRecord
 ('c0a70005-0000-4000-8000-000000000001'), -- SalHeadTeacherAssess
 ('c0a70006-0000-4000-8000-000000000001'), -- SalAfterSchoolRecord
 ('c0a70007-0000-4000-8000-000000000001'), -- SalInvigilateRecord
 ('c0a70008-0000-4000-8000-000000000001'), -- SalAttendanceSummary
 ('c0a70009-0000-4000-8000-000000000001')  -- SalAdjustment

DECLARE @btns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns VALUES (N'NF-add', N'新增', 1), (N'NF-edit', N'修改', 2), (N'NF-delete', N'删除', 3)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), m.ModuleId, NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @btnModules m CROSS JOIN @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = m.ModuleId AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO

-- 工资单管理按钮：算薪/提交/审批/归档/明细/驳回
DECLARE @payslipBtns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @payslipBtns VALUES
 (N'NF-calc', N'自动算薪', 1), (N'NF-submit', N'提交审批', 2), (N'NF-approve', N'审批', 3), (N'NF-lock', N'归档锁定', 4),
 (N'NF-details', N'明细', 5), (N'NF-reject', N'驳回', 6)
INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), 'c0a7000a-0000-4000-8000-000000000001', NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @payslipBtns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = 'c0a7000a-0000-4000-8000-000000000001' AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO

-- 薪酬报表按钮：导出
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = 'c0a7000c-0000-4000-8000-000000000001' AND F_EnCode = N'NF-export' AND ISNULL(F_DeleteMark,0)=0)
BEGIN
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0a7000c-0000-4000-8000-000000000001', NULL, N'NF-export', N'导出', 1, NULL, NULL, 1, 0, GETDATE())
END
GO

-- 薪酬申诉按钮：处理回复
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = 'c0a7000b-0000-4000-8000-000000000001' AND F_EnCode = N'NF-reply' AND ISNULL(F_DeleteMark,0)=0)
BEGIN
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0a7000b-0000-4000-8000-000000000001', NULL, N'NF-reply', N'回复处理', 1, NULL, NULL, 1, 0, GETDATE())
END
GO

-- =============================================================
-- 角色授权
-- 薪酬管理员/财务/校长/教务/德育/课后服务 → 管理菜单
-- 所有角色 → 我的工资条
-- =============================================================
DECLARE @manageRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @manageRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0 AND F_EnCode IN (N'R_01', N'R_02', N'G_01', N'R_05', N'G_02', N'R_07', N'R_08', N'G_03', N'R_06')

-- 一级菜单
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a70001-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a70001-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

-- 管理子菜单（除我的工资条外，全部授权给管理角色）
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
CROSS JOIN (SELECT F_Id FROM sys_module WHERE F_ParentId = 'c0a70001-0000-4000-8000-000000000001' AND F_EnCode <> N'SalMyPayslip' AND ISNULL(F_DeleteMark,0)=0) m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.F_Id AND ra.F_ObjectId = s.RoleId)

-- 所有管理菜单的按钮授权给管理角色
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
CROSS JOIN (SELECT mb.F_Id FROM sys_modulebutton mb INNER JOIN sys_module m ON mb.F_ModuleId = m.F_Id WHERE m.F_ParentId = 'c0a70001-0000-4000-8000-000000000001' AND ISNULL(mb.F_DeleteMark,0)=0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)
GO

-- 我的工资条：所有有效角色
DECLARE @allRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @allRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a7000d-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @allRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a7000d-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)
GO

-- 默认薪酬规则（无记录时插入一条默认配置）
IF NOT EXISTS (SELECT 1 FROM sal_rule)
BEGIN
	INSERT INTO [dbo].[sal_rule] ([F_Id],[F_CreatorTime]) VALUES (LOWER(NEWID()), GETDATE())
END
GO

-- -------------------------------------------------------------
-- 13. 审计列补齐（幂等，兼容旧库升级）
-- -------------------------------------------------------------
IF COL_LENGTH('sal_period_record','F_DeleteTime') IS NULL ALTER TABLE sal_period_record ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_period_record','F_DeleteUserId') IS NULL ALTER TABLE sal_period_record ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_headteacher_assess','F_DeleteTime') IS NULL ALTER TABLE sal_headteacher_assess ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_headteacher_assess','F_DeleteUserId') IS NULL ALTER TABLE sal_headteacher_assess ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_afterschool_record','F_DeleteTime') IS NULL ALTER TABLE sal_afterschool_record ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_afterschool_record','F_DeleteUserId') IS NULL ALTER TABLE sal_afterschool_record ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_invigilate_record','F_DeleteTime') IS NULL ALTER TABLE sal_invigilate_record ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_invigilate_record','F_DeleteUserId') IS NULL ALTER TABLE sal_invigilate_record ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_attendance_summary','F_DeleteTime') IS NULL ALTER TABLE sal_attendance_summary ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_attendance_summary','F_DeleteUserId') IS NULL ALTER TABLE sal_attendance_summary ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_adjustment','F_DeleteTime') IS NULL ALTER TABLE sal_adjustment ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_adjustment','F_DeleteUserId') IS NULL ALTER TABLE sal_adjustment ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_payslip','F_DeleteTime') IS NULL ALTER TABLE sal_payslip ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_payslip','F_DeleteUserId') IS NULL ALTER TABLE sal_payslip ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_appeal','F_DeleteTime') IS NULL ALTER TABLE sal_appeal ADD F_DeleteTime datetime NULL;
IF COL_LENGTH('sal_appeal','F_DeleteUserId') IS NULL ALTER TABLE sal_appeal ADD F_DeleteUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_appeal','F_CreatorUserId') IS NULL ALTER TABLE sal_appeal ADD F_CreatorUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_appeal','F_LastModifyUserId') IS NULL ALTER TABLE sal_appeal ADD F_LastModifyUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_payslip_detail','F_CreatorUserId') IS NULL ALTER TABLE sal_payslip_detail ADD F_CreatorUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
IF COL_LENGTH('sal_audit_log','F_CreatorUserId') IS NULL ALTER TABLE sal_audit_log ADD F_CreatorUserId nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL;
GO
