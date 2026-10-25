-- =============================================================
-- 大学教务扩展·批次2：培养方案 + 教学任务 + 排课调课 初始化脚本（幂等，可重复执行）
-- 内容：培养方案/教学任务/节次时间/不排课时间/排课参数/调课申请
--      + school_timetable 增加教室Id + 菜单按钮授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 培养方案
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_program')
BEGIN
	CREATE TABLE [dbo].[school_program](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_MajorId] [nvarchar](50) NULL,
		[F_Semester] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_Credit] [decimal](18, 2) NULL,
		[F_Period] [int] NULL,
		[F_Nature] [nvarchar](20) NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_program] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 教学任务
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_teaching_task')
BEGIN
	CREATE TABLE [dbo].[school_teaching_task](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Semester] [nvarchar](50) NULL,
		[F_TeacherId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_WeekPeriod] [int] NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_teaching_task] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 3. 节次起止时间
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_period_time')
BEGIN
	CREATE TABLE [dbo].[school_period_time](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Section] [int] NULL,
		[F_StartTime] [nvarchar](10) NULL,
		[F_EndTime] [nvarchar](10) NULL,
		[F_SortCode] [int] NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_period_time] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 4. 不排课时间
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_no_schedule')
BEGIN
	CREATE TABLE [dbo].[school_no_schedule](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Type] [int] NULL,
		[F_TargetId] [nvarchar](50) NULL,
		[F_WeekDay] [int] NULL,
		[F_Section] [int] NULL,
		[F_Reason] [nvarchar](200) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_no_schedule] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 5. 排课参数（单条）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_schedule_param')
BEGIN
	CREATE TABLE [dbo].[school_schedule_param](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_DaysPerWeek] [int] NULL,
		[F_SectionsPerDay] [int] NULL,
		[F_MaxSameCourseDaily] [int] NULL,
		[F_MaxConsecutive] [int] NULL,
		[F_Description] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_schedule_param] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 6. 调课申请
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_schedule_change')
BEGIN
	CREATE TABLE [dbo].[school_schedule_change](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_TimetableId] [nvarchar](50) NULL,
		[F_Semester] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_TeacherId] [nvarchar](50) NULL,
		[F_OldWeekDay] [int] NULL,
		[F_OldSection] [int] NULL,
		[F_NewWeekDay] [int] NULL,
		[F_NewSection] [int] NULL,
		[F_NewClassroomId] [nvarchar](50) NULL,
		[F_Reason] [nvarchar](500) NULL,
		[F_ApplicantId] [nvarchar](50) NULL,
		[F_Status] [int] NULL,
		[F_AuditUserId] [nvarchar](50) NULL,
		[F_AuditTime] [datetime2](7) NULL,
		[F_AuditRemark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_schedule_change] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_schedule_change_timetable' AND object_id = OBJECT_ID('school_schedule_change'))
	CREATE NONCLUSTERED INDEX [IX_school_schedule_change_timetable] ON [dbo].[school_schedule_change] ([F_TimetableId] ASC)
GO

-- -------------------------------------------------------------
-- 7. 课表增加教室Id
-- -------------------------------------------------------------
IF COL_LENGTH('school_timetable', 'F_ClassroomId') IS NULL
	ALTER TABLE [dbo].[school_timetable] ADD [F_ClassroomId] [nvarchar](50) NULL
GO

-- -------------------------------------------------------------
-- 8. 菜单（挂在教学管理 08dafb93-99da-47e5-883f-8ff1acc7a050 下）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52001-0000-4000-8000-000000000001' OR F_EnCode = N'ProgramManage')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52001-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ProgramManage', N'培养方案', N'fa fa-book', N'/InstructionalManagement/Program/Index', N'iframe', 1, 0, 0, 0, 0, 0, 32, 0, 1, N'专业学期课程设置与学分学时', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52002-0000-4000-8000-000000000001' OR F_EnCode = N'TeachingTask')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52002-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'TeachingTask', N'教学任务', N'fa fa-tasks', N'/InstructionalManagement/TeachingTask/Index', N'iframe', 1, 0, 0, 0, 0, 0, 33, 0, 1, N'教师授课任务分配', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52003-0000-4000-8000-000000000001' OR F_EnCode = N'PeriodTime')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52003-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'PeriodTime', N'节次时间', N'fa fa-clock-o', N'/InstructionalManagement/PeriodTime/Index', N'iframe', 1, 0, 0, 0, 0, 0, 34, 0, 1, N'节次起止时间维护', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52004-0000-4000-8000-000000000001' OR F_EnCode = N'ScheduleParam')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52004-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ScheduleParam', N'排课参数', N'fa fa-cog', N'/InstructionalManagement/ScheduleParam/Index', N'iframe', 1, 0, 0, 0, 0, 0, 35, 0, 1, N'自动排课参数设置', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52005-0000-4000-8000-000000000001' OR F_EnCode = N'NoSchedule')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52005-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'NoSchedule', N'不排课时间', N'fa fa-ban', N'/InstructionalManagement/NoSchedule/Index', N'iframe', 1, 0, 0, 0, 0, 0, 36, 0, 1, N'全校/教室/教师不排课时间', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a52006-0000-4000-8000-000000000001' OR F_EnCode = N'ScheduleChange')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a52006-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ScheduleChange', N'调课申请', N'fa fa-exchange', N'/InstructionalManagement/ScheduleChange/Index', N'iframe', 1, 0, 0, 0, 0, 0, 37, 0, 1, N'调课申请与审批', GETDATE())
GO

-- -------------------------------------------------------------
-- 9. 按钮（通用三按钮：培养方案/教学任务/节次时间/不排课时间）
-- -------------------------------------------------------------
DECLARE @i INT = 1
DECLARE @menus TABLE (mid NVARCHAR(50), url NVARCHAR(200))
INSERT INTO @menus (mid, url) VALUES
(N'c0a52001-0000-4000-8000-000000000001', N'/InstructionalManagement/Program'),
(N'c0a52002-0000-4000-8000-000000000001', N'/InstructionalManagement/TeachingTask'),
(N'c0a52003-0000-4000-8000-000000000001', N'/InstructionalManagement/PeriodTime'),
(N'c0a52005-0000-4000-8000-000000000001', N'/InstructionalManagement/NoSchedule')
WHILE @i <= 4
BEGIN
	DECLARE @mid NVARCHAR(50), @url NVARCHAR(200)
	SELECT @mid = mid, @url = url FROM (SELECT mid, url, ROW_NUMBER() OVER (ORDER BY mid) rn FROM @menus) t WHERE rn = @i
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = N'NF-add')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid, N'0', 1, N'NF-add', N'新增', 1, N'add', @url + N'/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = N'NF-edit')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid, N'0', 1, N'NF-edit', N'修改', 2, N'edit', @url + N'/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = N'NF-delete')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid, N'0', 1, N'NF-delete', N'删除', 2, N'delete', @url + N'/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
	SET @i = @i + 1
END
GO

-- 调课申请：申请/审批/删除
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a52006-0000-4000-8000-000000000001' AND F_EnCode = N'NF-add')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a52006-0000-4000-8000-000000000001', N'0', 1, N'NF-add', N'申请调课', 1, N'add', N'/InstructionalManagement/ScheduleChange/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a52006-0000-4000-8000-000000000001' AND F_EnCode = N'NF-audit')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a52006-0000-4000-8000-000000000001', N'0', 1, N'NF-audit', N'审批', 2, N'audit', N'/InstructionalManagement/ScheduleChange/AuditForm', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a52006-0000-4000-8000-000000000001' AND F_EnCode = N'NF-delete')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a52006-0000-4000-8000-000000000001', N'0', 1, N'NF-delete', N'删除', 2, N'delete', N'/InstructionalManagement/ScheduleChange/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- -------------------------------------------------------------
-- 10. 授权：教师类角色（R_01 系统管理员、R_02 普通管理员、R_04 老师、R_05 校长、R_06 班主任）
-- -------------------------------------------------------------
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_module m
CROSS JOIN sys_role r
WHERE m.F_Id IN (N'c0a52001-0000-4000-8000-000000000001', N'c0a52002-0000-4000-8000-000000000001', N'c0a52003-0000-4000-8000-000000000001',
	N'c0a52004-0000-4000-8000-000000000001', N'c0a52005-0000-4000-8000-000000000001', N'c0a52006-0000-4000-8000-000000000001')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_modulebutton b
CROSS JOIN sys_role r
WHERE b.F_ModuleId IN (N'c0a52001-0000-4000-8000-000000000001', N'c0a52002-0000-4000-8000-000000000001', N'c0a52003-0000-4000-8000-000000000001',
	N'c0a52005-0000-4000-8000-000000000001', N'c0a52006-0000-4000-8000-000000000001')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'大学教务扩展·批次2（培养方案+教学任务+排课调课）初始化完成'
GO
