-- =============================================================
-- 课表管理模块初始化脚本（教学管理）
-- 内容：2 张 school_timetable* 表 + 2 个菜单 + 角色授权
-- 执行方式：在 SSMS 中连接 watercloudnetdb 数据库后整体执行
-- 可重复执行（幂等）
-- =============================================================
USE [watercloudnetdb]
GO

-- -------------------------------------------------------------
-- 1. 授课计划表 school_timetable_plan
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_timetable_plan]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_timetable_plan] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_Semester] nvarchar(50) NOT NULL,
		[F_ClassId] nvarchar(50) NOT NULL,
		[F_CourseId] nvarchar(50) NOT NULL,
		[F_TeacherId] nvarchar(50) NULL,
		[F_WeeklyHours] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_TIMETABLE_PLAN] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 课表明细表 school_timetable
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_timetable]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_timetable] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_Semester] nvarchar(50) NOT NULL,
		[F_ClassId] nvarchar(50) NOT NULL,
		[F_WeekDay] int NULL,
		[F_Section] int NULL,
		[F_CourseId] nvarchar(50) NULL,
		[F_TeacherId] nvarchar(50) NULL,
		[F_Classroom] nvarchar(100) NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_TIMETABLE] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_TIMETABLE_Q] ON [dbo].[school_timetable] ([F_Semester] ASC, [F_ClassId] ASC, [F_TeacherId] ASC)
END
GO

-- =============================================================
-- 3. 菜单（挂在"教学管理"目录下，找不到则挂到根节点）
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a20001-0000-4000-8000-000000000001')
BEGIN
	DECLARE @ttParent1 nvarchar(50), @ttLayer1 int
	SELECT TOP 1 @ttParent1 = F_Id, @ttLayer1 = ISNULL(F_Layers, 1) FROM sys_module WHERE F_FullName = N'教学管理' AND ISNULL(F_DeleteMark, 0) = 0 ORDER BY F_Layers
	IF @ttParent1 IS NULL BEGIN SET @ttParent1 = '0' SET @ttLayer1 = 0 END
	INSERT INTO [dbo].[sys_module] ([F_Id], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Icon], [F_UrlAddress], [F_Target], [F_IsMenu], [F_IsExpand], [F_IsFields], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_Description], [F_CreatorTime])
	VALUES (N'c0a20001-0000-4000-8000-000000000001', @ttParent1, @ttLayer1 + 1, N'TimetableManage', N'课表管理', N'fa fa-calendar', N'/InstructionalManagement/Timetable/Manage', N'iframe', 1, 0, 0, 0, 0, 0, 4, 0, 1, N'授课计划维护与自动排课', GETDATE())
END
GO

IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a20001-0000-4000-8000-000000000002')
BEGIN
	DECLARE @ttParent2 nvarchar(50), @ttLayer2 int
	SELECT TOP 1 @ttParent2 = F_Id, @ttLayer2 = ISNULL(F_Layers, 1) FROM sys_module WHERE F_FullName = N'教学管理' AND ISNULL(F_DeleteMark, 0) = 0 ORDER BY F_Layers
	IF @ttParent2 IS NULL BEGIN SET @ttParent2 = '0' SET @ttLayer2 = 0 END
	INSERT INTO [dbo].[sys_module] ([F_Id], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Icon], [F_UrlAddress], [F_Target], [F_IsMenu], [F_IsExpand], [F_IsFields], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_Description], [F_CreatorTime])
	VALUES (N'c0a20001-0000-4000-8000-000000000002', @ttParent2, @ttLayer2 + 1, N'TimetableQuery', N'课表查询', N'fa fa-table', N'/InstructionalManagement/Timetable/Index', N'iframe', 1, 0, 0, 0, 0, 0, 5, 0, 1, N'按班级/教师/学生查询课表', GETDATE())
END
GO

-- =============================================================
-- 4. 角色授权（默认授权给管理员类角色，其他角色请在"系统管理-角色权限"中配置）
-- =============================================================
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_role r
CROSS JOIN sys_module m
WHERE ISNULL(r.F_DeleteMark, 0) = 0
  AND (r.F_EnCode IN (N'admin', N'administrators') OR r.F_FullName LIKE N'%管理员%')
  AND m.F_Id IN (N'c0a20001-0000-4000-8000-000000000001', N'c0a20001-0000-4000-8000-000000000002')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'课表管理模块初始化完成'
GO
