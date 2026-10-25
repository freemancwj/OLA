-- =============================================================
-- 大学教务扩展·批次1：基础数据 + 考务核心 初始化脚本（幂等，可重复执行）
-- 内容：教学楼/教室/考试时间段/成绩类型/考务安排/补考/清考/成绩修改申请/免缓缺登记
--      + school_course 扩展大学字段 + 菜单按钮授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 教学楼
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_building')
BEGIN
	CREATE TABLE [dbo].[school_building](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Name] [nvarchar](100) NULL,
		[F_Location] [nvarchar](200) NULL,
		[F_Description] [nvarchar](max) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_building] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 教室
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_classroom')
BEGIN
	CREATE TABLE [dbo].[school_classroom](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_BuildingId] [nvarchar](50) NULL,
		[F_Name] [nvarchar](100) NULL,
		[F_Capacity] [int] NULL,
		[F_Type] [nvarchar](50) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_classroom] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_classroom_building' AND object_id = OBJECT_ID('school_classroom'))
	CREATE NONCLUSTERED INDEX [IX_school_classroom_building] ON [dbo].[school_classroom] ([F_BuildingId] ASC)
GO

-- -------------------------------------------------------------
-- 3. 考试时间段
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_exam_period')
BEGIN
	CREATE TABLE [dbo].[school_exam_period](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Name] [nvarchar](100) NULL,
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
		CONSTRAINT [PK_school_exam_period] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 4. 成绩类型（是否计入统计）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_score_type')
BEGIN
	CREATE TABLE [dbo].[school_score_type](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Name] [nvarchar](100) NULL,
		[F_IncludeStat] [tinyint] NULL,
		[F_SortCode] [int] NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_score_type] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- 预置常用成绩类型
IF NOT EXISTS (SELECT 1 FROM school_score_type WHERE F_Name = N'平时成绩' AND ISNULL(F_DeleteMark,0) = 0)
	INSERT INTO [dbo].[school_score_type] ([F_Id],[F_Name],[F_IncludeStat],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (N'c0a59901-0000-4000-8000-0000000000a1', N'平时成绩', 1, 1, 1, 0, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM school_score_type WHERE F_Name = N'期中考试' AND ISNULL(F_DeleteMark,0) = 0)
	INSERT INTO [dbo].[school_score_type] ([F_Id],[F_Name],[F_IncludeStat],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (N'c0a59901-0000-4000-8000-0000000000a2', N'期中考试', 1, 2, 1, 0, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM school_score_type WHERE F_Name = N'期末考试' AND ISNULL(F_DeleteMark,0) = 0)
	INSERT INTO [dbo].[school_score_type] ([F_Id],[F_Name],[F_IncludeStat],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (N'c0a59901-0000-4000-8000-0000000000a3', N'期末考试', 1, 3, 1, 0, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM school_score_type WHERE F_Name = N'补考' AND ISNULL(F_DeleteMark,0) = 0)
	INSERT INTO [dbo].[school_score_type] ([F_Id],[F_Name],[F_IncludeStat],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (N'c0a59901-0000-4000-8000-0000000000a4', N'补考', 0, 4, 1, 0, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM school_score_type WHERE F_Name = N'清考' AND ISNULL(F_DeleteMark,0) = 0)
	INSERT INTO [dbo].[school_score_type] ([F_Id],[F_Name],[F_IncludeStat],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (N'c0a59901-0000-4000-8000-0000000000a5', N'清考', 0, 5, 1, 0, GETDATE())
GO

-- -------------------------------------------------------------
-- 5. 考务安排
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_exam_arrange')
BEGIN
	CREATE TABLE [dbo].[school_exam_arrange](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_ExamName] [nvarchar](200) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_ClassroomId] [nvarchar](50) NULL,
		[F_ExamDate] [datetime2](7) NULL,
		[F_PeriodId] [nvarchar](50) NULL,
		[F_MainInvigilatorId] [nvarchar](50) NULL,
		[F_SubInvigilatorId] [nvarchar](50) NULL,
		[F_Status] [tinyint] NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_exam_arrange] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_exam_arrange_room' AND object_id = OBJECT_ID('school_exam_arrange'))
	CREATE NONCLUSTERED INDEX [IX_school_exam_arrange_room] ON [dbo].[school_exam_arrange] ([F_ClassroomId] ASC, [F_ExamDate] ASC, [F_PeriodId] ASC)
GO

-- -------------------------------------------------------------
-- 6. 补考管理
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_makeup_exam')
BEGIN
	CREATE TABLE [dbo].[school_makeup_exam](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_StudentId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_ScoreId] [nvarchar](50) NULL,
		[F_ExamDate] [datetime2](7) NULL,
		[F_PeriodId] [nvarchar](50) NULL,
		[F_ClassroomId] [nvarchar](50) NULL,
		[F_MainInvigilatorId] [nvarchar](50) NULL,
		[F_SubInvigilatorId] [nvarchar](50) NULL,
		[F_Status] [tinyint] NULL,
		[F_NewScore] [decimal](18, 2) NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_makeup_exam] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_makeup_exam_sc' AND object_id = OBJECT_ID('school_makeup_exam'))
	CREATE NONCLUSTERED INDEX [IX_school_makeup_exam_sc] ON [dbo].[school_makeup_exam] ([F_StudentId] ASC, [F_CourseId] ASC)
GO

-- -------------------------------------------------------------
-- 7. 清考管理
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_clear_exam')
BEGIN
	CREATE TABLE [dbo].[school_clear_exam](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_StudentId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_ScoreId] [nvarchar](50) NULL,
		[F_ExamDate] [datetime2](7) NULL,
		[F_PeriodId] [nvarchar](50) NULL,
		[F_ClassroomId] [nvarchar](50) NULL,
		[F_MainInvigilatorId] [nvarchar](50) NULL,
		[F_SubInvigilatorId] [nvarchar](50) NULL,
		[F_Status] [tinyint] NULL,
		[F_NewScore] [decimal](18, 2) NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_clear_exam] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_clear_exam_sc' AND object_id = OBJECT_ID('school_clear_exam'))
	CREATE NONCLUSTERED INDEX [IX_school_clear_exam_sc] ON [dbo].[school_clear_exam] ([F_StudentId] ASC, [F_CourseId] ASC)
GO

-- -------------------------------------------------------------
-- 8. 成绩修改申请
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_score_change')
BEGIN
	CREATE TABLE [dbo].[school_score_change](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_ScoreId] [nvarchar](50) NULL,
		[F_StudentId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_OldScore] [decimal](18, 2) NULL,
		[F_NewScore] [decimal](18, 2) NULL,
		[F_Reason] [nvarchar](500) NULL,
		[F_Status] [tinyint] NULL,
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
		CONSTRAINT [PK_school_score_change] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_score_change_score' AND object_id = OBJECT_ID('school_score_change'))
	CREATE NONCLUSTERED INDEX [IX_school_score_change_score] ON [dbo].[school_score_change] ([F_ScoreId] ASC)
GO

-- -------------------------------------------------------------
-- 9. 免缓缺登记
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_score_exemption')
BEGIN
	CREATE TABLE [dbo].[school_score_exemption](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_StudentId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_Type] [nvarchar](20) NULL,
		[F_Reason] [nvarchar](500) NULL,
		[F_Status] [tinyint] NULL,
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
		CONSTRAINT [PK_school_score_exemption] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_score_exemption_sc' AND object_id = OBJECT_ID('school_score_exemption'))
	CREATE NONCLUSTERED INDEX [IX_school_score_exemption_sc] ON [dbo].[school_score_exemption] ([F_StudentId] ASC, [F_CourseId] ASC)
GO

-- -------------------------------------------------------------
-- 10. school_course 扩展大学字段
-- -------------------------------------------------------------
IF COL_LENGTH('school_course', 'F_MajorId') IS NULL
	ALTER TABLE [dbo].[school_course] ADD [F_MajorId] [nvarchar](50) NULL
GO
IF COL_LENGTH('school_course', 'F_Credit') IS NULL
	ALTER TABLE [dbo].[school_course] ADD [F_Credit] [decimal](18, 2) NULL
GO
IF COL_LENGTH('school_course', 'F_Period') IS NULL
	ALTER TABLE [dbo].[school_course] ADD [F_Period] [int] NULL
GO
IF COL_LENGTH('school_course', 'F_Nature') IS NULL
	ALTER TABLE [dbo].[school_course] ADD [F_Nature] [nvarchar](50) NULL
GO
IF COL_LENGTH('school_course', 'F_ExamScore') IS NULL
	ALTER TABLE [dbo].[school_course] ADD [F_ExamScore] [decimal](18, 2) NULL
GO

-- -------------------------------------------------------------
-- 11. 菜单（挂在教学管理 08dafb93-99da-47e5-883f-8ff1acc7a050 下）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51001-0000-4000-8000-000000000001' OR F_EnCode = N'MajorManage')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51001-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'MajorManage', N'专业管理', N'fa fa-graduation-cap', N'/InstructionalManagement/Major/Index', N'iframe', 1, 0, 0, 0, 0, 0, 21, 0, 1, N'大学专业维护（基于组织机构）', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51002-0000-4000-8000-000000000001' OR F_EnCode = N'BuildingManage')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51002-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'BuildingManage', N'教学楼管理', N'fa fa-building', N'/InstructionalManagement/Building/Index', N'iframe', 1, 0, 0, 0, 0, 0, 22, 0, 1, N'教学楼维护', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51003-0000-4000-8000-000000000001' OR F_EnCode = N'ClassroomManage')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51003-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ClassroomManage', N'教室管理', N'fa fa-home', N'/InstructionalManagement/Classroom/Index', N'iframe', 1, 0, 0, 0, 0, 0, 23, 0, 1, N'教室维护（容量/类型）', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51004-0000-4000-8000-000000000001' OR F_EnCode = N'ExamPeriod')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51004-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ExamPeriod', N'考试时间段', N'fa fa-clock-o', N'/InstructionalManagement/ExamPeriod/Index', N'iframe', 1, 0, 0, 0, 0, 0, 24, 0, 1, N'考试时间段维护', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51005-0000-4000-8000-000000000001' OR F_EnCode = N'ExamArrange')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51005-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ExamArrange', N'考务安排', N'fa fa-calendar', N'/InstructionalManagement/ExamArrange/Index', N'iframe', 1, 0, 0, 0, 0, 0, 25, 0, 1, N'考试安排与监考分配', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51006-0000-4000-8000-000000000001' OR F_EnCode = N'Invigilator')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51006-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'Invigilator', N'监考管理', N'fa fa-eye', N'/InstructionalManagement/ExamArrange/Invigilator', N'iframe', 1, 0, 0, 0, 0, 0, 26, 0, 1, N'监考任务查询（教师看本人）', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51007-0000-4000-8000-000000000001' OR F_EnCode = N'MakeupExam')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51007-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'MakeupExam', N'补考管理', N'fa fa-repeat', N'/InstructionalManagement/MakeupExam/Index', N'iframe', 1, 0, 0, 0, 0, 0, 27, 0, 1, N'补考名单生成/安排/录分', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51008-0000-4000-8000-000000000001' OR F_EnCode = N'ClearExam')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51008-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ClearExam', N'清考管理', N'fa fa-refresh', N'/InstructionalManagement/ClearExam/Index', N'iframe', 1, 0, 0, 0, 0, 0, 28, 0, 1, N'清考名单生成/安排/录分', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51009-0000-4000-8000-000000000001' OR F_EnCode = N'ScoreTypeManage')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51009-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ScoreTypeManage', N'成绩类型', N'fa fa-tags', N'/InstructionalManagement/ScoreType/Index', N'iframe', 1, 0, 0, 0, 0, 0, 29, 0, 1, N'成绩类型与统计口径维护', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51010-0000-4000-8000-000000000001' OR F_EnCode = N'ScoreChange')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51010-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ScoreChange', N'成绩修改申请', N'fa fa-edit', N'/InstructionalManagement/ScoreChange/Index', N'iframe', 1, 0, 0, 0, 0, 0, 30, 0, 1, N'成绩修改申请与审批', GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a51011-0000-4000-8000-000000000001' OR F_EnCode = N'ScoreExemption')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a51011-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'ScoreExemption', N'免缓缺登记', N'fa fa-minus-circle', N'/InstructionalManagement/ScoreExemption/Index', N'iframe', 1, 0, 0, 0, 0, 0, 31, 0, 1, N'免考/缓考/缺考登记与审批', GETDATE())
GO

-- -------------------------------------------------------------
-- 12. 按钮（新增/修改/删除；补考清考加生成名单；申请类加审批）
-- -------------------------------------------------------------
-- 通用三按钮宏：Major/Building/Classroom/ExamPeriod/ScoreType
DECLARE @i INT = 1
DECLARE @menus TABLE (mid NVARCHAR(50), url NVARCHAR(200))
INSERT INTO @menus (mid, url) VALUES
(N'c0a51001-0000-4000-8000-000000000001', N'/InstructionalManagement/Major'),
(N'c0a51002-0000-4000-8000-000000000001', N'/InstructionalManagement/Building'),
(N'c0a51003-0000-4000-8000-000000000001', N'/InstructionalManagement/Classroom'),
(N'c0a51004-0000-4000-8000-000000000001', N'/InstructionalManagement/ExamPeriod'),
(N'c0a51009-0000-4000-8000-000000000001', N'/InstructionalManagement/ScoreType'),
(N'c0a51005-0000-4000-8000-000000000001', N'/InstructionalManagement/ExamArrange')
WHILE @i <= 6
BEGIN
	DECLARE @mid NVARCHAR(50), @url NVARCHAR(200)
	SELECT @mid = mid, @url = url FROM (SELECT mid, url, ROW_NUMBER() OVER (ORDER BY mid) rn FROM @menus) t WHERE rn = @i
	-- 取菜单Id第5段前缀生成按钮Id
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

-- 补考/清考：生成名单 + 修改 + 删除
DECLARE @j INT = 1
DECLARE @exammenus TABLE (mid NVARCHAR(50), url NVARCHAR(200))
INSERT INTO @exammenus (mid, url) VALUES
(N'c0a51007-0000-4000-8000-000000000001', N'/InstructionalManagement/MakeupExam'),
(N'c0a51008-0000-4000-8000-000000000001', N'/InstructionalManagement/ClearExam')
WHILE @j <= 2
BEGIN
	DECLARE @mid2 NVARCHAR(50), @url2 NVARCHAR(200)
	SELECT @mid2 = mid, @url2 = url FROM (SELECT mid, url, ROW_NUMBER() OVER (ORDER BY mid) rn FROM @exammenus) t WHERE rn = @j
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid2 AND F_EnCode = N'NF-generate')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid2, N'0', 1, N'NF-generate', N'生成名单', 1, N'add', @url2 + N'/Generate', 0, 0, 0, 0, 1, 0, 1, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid2 AND F_EnCode = N'NF-edit')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid2, N'0', 1, N'NF-edit', N'安排录分', 2, N'edit', @url2 + N'/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid2 AND F_EnCode = N'NF-delete')
		INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
		VALUES (LOWER(NEWID()), @mid2, N'0', 1, N'NF-delete', N'删除', 2, N'delete', @url2 + N'/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
	SET @j = @j + 1
END
GO

-- 成绩修改申请：新增/审批/删除
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51010-0000-4000-8000-000000000001' AND F_EnCode = N'NF-add')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51010-0000-4000-8000-000000000001', N'0', 1, N'NF-add', N'新增申请', 1, N'add', N'/InstructionalManagement/ScoreChange/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51010-0000-4000-8000-000000000001' AND F_EnCode = N'NF-audit')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51010-0000-4000-8000-000000000001', N'0', 1, N'NF-audit', N'审批', 2, N'audit', N'/InstructionalManagement/ScoreChange/AuditForm', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51010-0000-4000-8000-000000000001' AND F_EnCode = N'NF-delete')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51010-0000-4000-8000-000000000001', N'0', 1, N'NF-delete', N'删除', 2, N'delete', N'/InstructionalManagement/ScoreChange/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- 免缓缺登记：新增/修改/审批/删除
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51011-0000-4000-8000-000000000001' AND F_EnCode = N'NF-add')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51011-0000-4000-8000-000000000001', N'0', 1, N'NF-add', N'新增登记', 1, N'add', N'/InstructionalManagement/ScoreExemption/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51011-0000-4000-8000-000000000001' AND F_EnCode = N'NF-edit')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51011-0000-4000-8000-000000000001', N'0', 1, N'NF-edit', N'修改', 2, N'edit', N'/InstructionalManagement/ScoreExemption/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51011-0000-4000-8000-000000000001' AND F_EnCode = N'NF-audit')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51011-0000-4000-8000-000000000001', N'0', 1, N'NF-audit', N'审批', 2, N'audit', N'/InstructionalManagement/ScoreExemption/AuditForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a51011-0000-4000-8000-000000000001' AND F_EnCode = N'NF-delete')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), N'c0a51011-0000-4000-8000-000000000001', N'0', 1, N'NF-delete', N'删除', 2, N'delete', N'/InstructionalManagement/ScoreExemption/DeleteForm', 0, 0, 0, 0, 4, 0, 1, GETDATE())
GO

-- -------------------------------------------------------------
-- 13. 授权：教师类角色（R_01 系统管理员、R_02 普通管理员、R_04 老师、R_05 校长、R_06 班主任）
-- -------------------------------------------------------------
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_module m
CROSS JOIN sys_role r
WHERE m.F_Id IN (N'c0a51001-0000-4000-8000-000000000001', N'c0a51002-0000-4000-8000-000000000001', N'c0a51003-0000-4000-8000-000000000001',
	N'c0a51004-0000-4000-8000-000000000001', N'c0a51005-0000-4000-8000-000000000001', N'c0a51006-0000-4000-8000-000000000001',
	N'c0a51007-0000-4000-8000-000000000001', N'c0a51008-0000-4000-8000-000000000001', N'c0a51009-0000-4000-8000-000000000001',
	N'c0a51010-0000-4000-8000-000000000001', N'c0a51011-0000-4000-8000-000000000001')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_modulebutton b
CROSS JOIN sys_role r
WHERE b.F_ModuleId IN (N'c0a51001-0000-4000-8000-000000000001', N'c0a51002-0000-4000-8000-000000000001', N'c0a51003-0000-4000-8000-000000000001',
	N'c0a51004-0000-4000-8000-000000000001', N'c0a51005-0000-4000-8000-000000000001',
	N'c0a51007-0000-4000-8000-000000000001', N'c0a51008-0000-4000-8000-000000000001', N'c0a51009-0000-4000-8000-000000000001',
	N'c0a51010-0000-4000-8000-000000000001', N'c0a51011-0000-4000-8000-000000000001')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'大学教务扩展·批次1（基础数据+考务核心）初始化完成'
GO
