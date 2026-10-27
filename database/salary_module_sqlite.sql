-- 薪酬管理模块 SQLite 版（幂等）
CREATE TABLE IF NOT EXISTS sal_profile (
	F_Id TEXT PRIMARY KEY, F_UserId TEXT, F_JobNumber TEXT, F_EntryDate TEXT, F_TeachingYears INTEGER DEFAULT 0,
	F_PostType INTEGER DEFAULT 1, F_TitleLevel INTEGER DEFAULT 2, F_IsHeadTeacher INTEGER DEFAULT 0, F_IsDeputyHead INTEGER DEFAULT 0,
	F_IsGroupLeader INTEGER DEFAULT 0, F_IsLessonLeader INTEGER DEFAULT 0, F_IsGraduationTeacher INTEGER DEFAULT 0,
	F_ClassId TEXT, F_SocialBase NUMERIC DEFAULT 0, F_TaxDeduction NUMERIC DEFAULT 0, F_Remark TEXT,
	F_EnabledMark INTEGER DEFAULT 1, F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT,
	F_LastModifyTime TEXT, F_LastModifyUserId TEXT, F_DeleteTime TEXT, F_DeleteUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_profile_F_UserId ON sal_profile (F_UserId);

CREATE TABLE IF NOT EXISTS sal_rule (
	F_Id TEXT PRIMARY KEY, F_RateTitle1 NUMERIC DEFAULT 50, F_RateTitle2 NUMERIC DEFAULT 60, F_RateTitle3 NUMERIC DEFAULT 80, F_RateTitle4 NUMERIC DEFAULT 100,
	F_StandardWeeklyPeriods INTEGER DEFAULT 12, F_OvertimeRate NUMERIC DEFAULT 1.5, F_MorningReadRate NUMERIC DEFAULT 20, F_EveningStudyRate NUMERIC DEFAULT 30,
	F_SubstituteRate NUMERIC DEFAULT 50, F_ClassTier1Max INTEGER DEFAULT 40, F_ClassTier1Amount NUMERIC DEFAULT 500,
	F_ClassTier2Max INTEGER DEFAULT 50, F_ClassTier2Amount NUMERIC DEFAULT 600, F_ClassTier3Amount NUMERIC DEFAULT 700,
	F_GraduationBonusRate NUMERIC DEFAULT 0.3, F_DeputyRate NUMERIC DEFAULT 0.3, F_AssessExcellent NUMERIC DEFAULT 1.0,
	F_AssessGood NUMERIC DEFAULT 0.9, F_AssessPoor NUMERIC DEFAULT 0.6, F_AfterSchoolHomework NUMERIC DEFAULT 40,
	F_AfterSchoolClub NUMERIC DEFAULT 60, F_AfterSchoolCare NUMERIC DEFAULT 30, F_GroupLeaderAllowance NUMERIC DEFAULT 200,
	F_LessonLeaderAllowance NUMERIC DEFAULT 100, F_InvigilateRate NUMERIC DEFAULT 100, F_DutyRate NUMERIC DEFAULT 50,
	F_LateDeduct NUMERIC DEFAULT 20, F_SickLeaveDeduct NUMERIC DEFAULT 50, F_PersonalLeaveDeduct NUMERIC DEFAULT 100,
	F_AbsentDeduct NUMERIC DEFAULT 200, F_SocialRate NUMERIC DEFAULT 0.105, F_HousingFundRate NUMERIC DEFAULT 0.12,
	F_UnionFeeRate NUMERIC DEFAULT 0.005, F_EnabledMark INTEGER DEFAULT 1, F_CreatorTime TEXT, F_CreatorUserId TEXT,
	F_LastModifyTime TEXT, F_LastModifyUserId TEXT);

CREATE TABLE IF NOT EXISTS sal_period_record (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_NormalPeriods NUMERIC DEFAULT 0, F_OvertimePeriods NUMERIC DEFAULT 0,
	F_SubstitutePeriods NUMERIC DEFAULT 0, F_MorningReadCount INTEGER DEFAULT 0, F_EveningStudyCount INTEGER DEFAULT 0,
	F_Remark TEXT, F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_period_F_Month_UserId ON sal_period_record (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_headteacher_assess (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_ClassId TEXT, F_ClassSize INTEGER DEFAULT 0,
	F_IsGraduation INTEGER DEFAULT 0, F_AssessLevel INTEGER DEFAULT 2, F_Score NUMERIC, F_Remark TEXT,
	F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_assess_F_Month_UserId ON sal_headteacher_assess (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_afterschool_record (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_ServiceType INTEGER DEFAULT 1, F_Periods NUMERIC DEFAULT 0,
	F_Remark TEXT, F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_afterschool_F_Month_UserId ON sal_afterschool_record (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_invigilate_record (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_ExamName TEXT, F_Sessions INTEGER DEFAULT 0,
	F_Remark TEXT, F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_invigilate_F_Month_UserId ON sal_invigilate_record (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_attendance_summary (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_SickLeaveDays NUMERIC DEFAULT 0, F_PersonalLeaveDays NUMERIC DEFAULT 0,
	F_AbsentDays NUMERIC DEFAULT 0, F_LateCount INTEGER DEFAULT 0, F_EarlyCount INTEGER DEFAULT 0, F_Remark TEXT,
	F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_attendance_F_Month_UserId ON sal_attendance_summary (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_adjustment (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_ItemName TEXT, F_Amount NUMERIC DEFAULT 0, F_Reason TEXT,
	F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_adjustment_F_Month_UserId ON sal_adjustment (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_payslip (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_FixedTotal NUMERIC DEFAULT 0, F_PeriodTotal NUMERIC DEFAULT 0,
	F_HeadTeacherTotal NUMERIC DEFAULT 0, F_AfterSchoolTotal NUMERIC DEFAULT 0, F_SpecialTotal NUMERIC DEFAULT 0,
	F_AdjustmentTotal NUMERIC DEFAULT 0, F_AttendanceDeduct NUMERIC DEFAULT 0, F_SocialPersonal NUMERIC DEFAULT 0,
	F_UnionFee NUMERIC DEFAULT 0, F_Tax NUMERIC DEFAULT 0, F_GrossPay NUMERIC DEFAULT 0, F_NetPay NUMERIC DEFAULT 0,
	F_Status INTEGER DEFAULT 0, F_SubmitTime TEXT, F_SubmitUserId TEXT, F_ApproveRemark TEXT, F_LockTime TEXT,
	F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_CreatorUserId TEXT, F_LastModifyTime TEXT, F_LastModifyUserId TEXT);
CREATE UNIQUE INDEX IF NOT EXISTS IX_sal_payslip_F_Month_UserId ON sal_payslip (F_Month, F_UserId);

CREATE TABLE IF NOT EXISTS sal_payslip_detail (
	F_Id TEXT PRIMARY KEY, F_PayslipId TEXT, F_Month TEXT, F_UserId TEXT, F_Category TEXT, F_ItemName TEXT,
	F_Quantity NUMERIC, F_UnitPrice NUMERIC, F_Amount NUMERIC DEFAULT 0, F_SortCode INTEGER DEFAULT 0, F_CreatorTime TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_detail_F_PayslipId ON sal_payslip_detail (F_PayslipId);

CREATE TABLE IF NOT EXISTS sal_appeal (
	F_Id TEXT PRIMARY KEY, F_PayslipId TEXT, F_Month TEXT, F_UserId TEXT, F_Content TEXT, F_Status INTEGER DEFAULT 0,
	F_Reply TEXT, F_ReplyUserId TEXT, F_ReplyTime TEXT, F_DeleteMark INTEGER DEFAULT 0, F_CreatorTime TEXT, F_LastModifyTime TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_appeal_F_Month ON sal_appeal (F_Month);

CREATE TABLE IF NOT EXISTS sal_audit_log (
	F_Id TEXT PRIMARY KEY, F_Month TEXT, F_UserId TEXT, F_Action TEXT, F_Detail TEXT, F_OperatorId TEXT, F_CreatorTime TEXT);
CREATE INDEX IF NOT EXISTS IX_sal_audit_F_Month ON sal_audit_log (F_Month);

-- 菜单
INSERT OR IGNORE INTO sys_module (F_Id,F_ParentId,F_Layers,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_Target,F_IsMenu,F_IsExpand,F_IsFields,F_IsPublic,F_AllowEdit,F_AllowDelete,F_SortCode,F_DeleteMark,F_EnabledMark,F_Description,F_CreatorTime)
SELECT 'c0a70001-0000-4000-8000-000000000001','87dc2de3-ccbc-4dab-bb90-89fc68cbde4f',2,'SalaryManagement','薪酬管理','fa fa-money',NULL,'expand',1,1,0,0,0,0,6,0,1,'教职工薪酬管理',datetime('now','localtime')
WHERE NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id='c0a70001-0000-4000-8000-000000000001');

INSERT OR IGNORE INTO sys_module (F_Id,F_ParentId,F_Layers,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_Target,F_IsMenu,F_IsExpand,F_IsFields,F_IsPublic,F_AllowEdit,F_AllowDelete,F_SortCode,F_DeleteMark,F_EnabledMark,F_Description,F_CreatorTime)
SELECT s.Id,'c0a70001-0000-4000-8000-000000000001',3,s.EnCode,s.Name,s.Icon,s.Url,'iframe',1,0,0,0,0,0,s.Sort,0,1,s.Name,datetime('now','localtime')
FROM (SELECT 'c0a70002-0000-4000-8000-000000000001' Id,'SalProfile' EnCode,'薪酬档案' Name,'fa fa-id-card' Icon,'/SalaryManagement/SalProfile/Index' Url,1 Sort
UNION ALL SELECT 'c0a70003-0000-4000-8000-000000000001','SalRule','薪酬规则配置','fa fa-cogs','/SalaryManagement/SalRule/Index',2
UNION ALL SELECT 'c0a70004-0000-4000-8000-000000000001','SalPeriodRecord','课时记录','fa fa-book','/SalaryManagement/SalPeriodRecord/Index',3
UNION ALL SELECT 'c0a70005-0000-4000-8000-000000000001','SalHeadTeacherAssess','班主任考核','fa fa-star','/SalaryManagement/SalHeadTeacherAssess/Index',4
UNION ALL SELECT 'c0a70006-0000-4000-8000-000000000001','SalAfterSchoolRecord','课后服务记录','fa fa-clock-o','/SalaryManagement/SalAfterSchoolRecord/Index',5
UNION ALL SELECT 'c0a70007-0000-4000-8000-000000000001','SalInvigilateRecord','监考记录','fa fa-edit','/SalaryManagement/SalInvigilateRecord/Index',6
UNION ALL SELECT 'c0a70008-0000-4000-8000-000000000001','SalAttendanceSummary','考勤汇总','fa fa-calendar-check-o','/SalaryManagement/SalAttendanceSummary/Index',7
UNION ALL SELECT 'c0a70009-0000-4000-8000-000000000001','SalAdjustment','薪酬调整','fa fa-sliders','/SalaryManagement/SalAdjustment/Index',8
UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001','SalPayslip','工资单管理','fa fa-file-text','/SalaryManagement/SalPayslip/Index',9
UNION ALL SELECT 'c0a7000b-0000-4000-8000-000000000001','SalAppeal','薪酬申诉','fa fa-comment','/SalaryManagement/SalAppeal/Index',10
UNION ALL SELECT 'c0a7000c-0000-4000-8000-000000000001','SalReport','薪酬报表','fa fa-bar-chart','/SalaryManagement/SalReport/Index',11
UNION ALL SELECT 'c0a7000d-0000-4000-8000-000000000001','SalMyPayslip','我的工资条','fa fa-money','/SalaryManagement/SalMyPayslip/Index',12) s
WHERE NOT EXISTS (SELECT 1 FROM sys_module x WHERE x.F_Id = s.Id);

-- 默认规则
INSERT INTO sal_rule (F_Id, F_CreatorTime)
SELECT lower(hex(randomblob(16))), datetime('now','localtime') WHERE NOT EXISTS (SELECT 1 FROM sal_rule);

-- 审计列补齐（重复执行时工具会跳过 duplicate column 错误，实现幂等）
ALTER TABLE sal_period_record ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_period_record ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_headteacher_assess ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_headteacher_assess ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_afterschool_record ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_afterschool_record ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_invigilate_record ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_invigilate_record ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_attendance_summary ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_attendance_summary ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_adjustment ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_adjustment ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_payslip ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_payslip ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_appeal ADD COLUMN F_DeleteTime TEXT;
ALTER TABLE sal_appeal ADD COLUMN F_DeleteUserId TEXT;
ALTER TABLE sal_appeal ADD COLUMN F_CreatorUserId TEXT;
ALTER TABLE sal_appeal ADD COLUMN F_LastModifyUserId TEXT;
ALTER TABLE sal_payslip_detail ADD COLUMN F_CreatorUserId TEXT;
ALTER TABLE sal_audit_log ADD COLUMN F_CreatorUserId TEXT;

-- 按钮（增/改/删 → 7个采集/档案菜单；算薪/提交/审批/归档/明细/驳回 → 工资单；回复 → 申诉；导出 → 报表）
INSERT INTO sys_modulebutton (F_Id,F_ModuleId,F_ParentId,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_SortCode,F_EnabledMark,F_DeleteMark,F_CreatorTime)
SELECT lower(hex(randomblob(16))), s.ModuleId, NULL, s.EnCode, s.Name, NULL, NULL, s.Sort, 1, 0, datetime('now','localtime')
FROM (
	SELECT 'c0a70002-0000-4000-8000-000000000001' ModuleId, 'NF-add' EnCode, '新增' Name, 1 Sort
	UNION ALL SELECT 'c0a70002-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70002-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70004-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70004-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70004-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70005-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70005-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70005-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70006-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70006-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70006-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70007-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70007-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70007-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70008-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70008-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70008-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a70009-0000-4000-8000-000000000001', 'NF-add', '新增', 1
	UNION ALL SELECT 'c0a70009-0000-4000-8000-000000000001', 'NF-edit', '修改', 2
	UNION ALL SELECT 'c0a70009-0000-4000-8000-000000000001', 'NF-delete', '删除', 3
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-calc', '自动算薪', 1
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-submit', '提交审批', 2
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-approve', '审批', 3
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-lock', '归档锁定', 4
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-details', '明细', 5
	UNION ALL SELECT 'c0a7000a-0000-4000-8000-000000000001', 'NF-reject', '驳回', 6
	UNION ALL SELECT 'c0a7000b-0000-4000-8000-000000000001', 'NF-reply', '回复处理', 1
	UNION ALL SELECT 'c0a7000c-0000-4000-8000-000000000001', 'NF-export', '导出', 1
) s
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = s.ModuleId AND x.F_EnCode = s.EnCode AND IFNULL(x.F_DeleteMark,0)=0);

-- 角色授权：管理角色（薪酬管理员/财务/校长/教务/德育/课后服务等）
-- 一级菜单 + 管理子菜单（不含我的工资条）
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))), 1, m.F_Id, 1, r.F_Id, 0, datetime('now','localtime')
FROM (SELECT F_Id FROM sys_role WHERE IFNULL(F_DeleteMark,0)=0 AND F_EnCode IN ('R_01','R_02','G_01','R_05','G_02','R_07','R_08','G_03','R_06')) r
CROSS JOIN (SELECT F_Id FROM sys_module WHERE (F_Id='c0a70001-0000-4000-8000-000000000001' OR (F_ParentId='c0a70001-0000-4000-8000-000000000001' AND F_EnCode <> 'SalMyPayslip')) AND IFNULL(F_DeleteMark,0)=0) m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemId=m.F_Id AND x.F_ObjectId=r.F_Id);

-- 管理角色 → 薪酬模块全部按钮
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))), 2, b.F_Id, 1, r.F_Id, 0, datetime('now','localtime')
FROM (SELECT F_Id FROM sys_role WHERE IFNULL(F_DeleteMark,0)=0 AND F_EnCode IN ('R_01','R_02','G_01','R_05','G_02','R_07','R_08','G_03','R_06')) r
CROSS JOIN (SELECT mb.F_Id FROM sys_modulebutton mb JOIN sys_module m ON mb.F_ModuleId=m.F_Id WHERE m.F_ParentId='c0a70001-0000-4000-8000-000000000001' AND IFNULL(mb.F_DeleteMark,0)=0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemId=b.F_Id AND x.F_ObjectId=r.F_Id);

-- 我的工资条：所有有效角色
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))), 1, 'c0a7000d-0000-4000-8000-000000000001', 1, r.F_Id, 0, datetime('now','localtime')
FROM (SELECT F_Id FROM sys_role WHERE IFNULL(F_DeleteMark,0)=0) r
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemId='c0a7000d-0000-4000-8000-000000000001' AND x.F_ObjectId=r.F_Id);
