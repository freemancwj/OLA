-- =============================================================
-- 课程管理模块初始化脚本（教学管理）
-- 内容：6 张 school_course_* 业务表 + 3 个菜单 + 功能按钮 + 角色授权
-- 执行方式：在 SSMS 中连接 watercloudnetdb 数据库后整体执行
-- 可重复执行（幂等）
-- =============================================================
USE [watercloudnetdb]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- -------------------------------------------------------------
-- 1. 课程分类表 school_course_category
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course_category]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course_category] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_ParentId] nvarchar(50) NULL,
		[F_Name] nvarchar(100) NOT NULL,
		[F_SortCode] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE_CATEGORY] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 课程表 school_course
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_CategoryId] nvarchar(50) NULL,
		[F_Name] nvarchar(200) NOT NULL,
		[F_Cover] nvarchar(500) NULL,
		[F_TeacherId] nvarchar(50) NULL,
		[F_Intro] nvarchar(max) NULL,
		[F_Status] tinyint NULL,
		[F_SortCode] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 3. 课程章节表 school_course_chapter
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course_chapter]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course_chapter] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_CourseId] nvarchar(50) NOT NULL,
		[F_Name] nvarchar(200) NOT NULL,
		[F_SortCode] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE_CHAPTER] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 4. 课时表 school_course_lesson
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course_lesson]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course_lesson] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_CourseId] nvarchar(50) NOT NULL,
		[F_ChapterId] nvarchar(50) NOT NULL,
		[F_Name] nvarchar(200) NOT NULL,
		[F_VideoUrl] nvarchar(500) NULL,
		[F_VideoDuration] int NULL,
		[F_VideoSize] bigint NULL,
		[F_SortCode] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE_LESSON] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 5. 课程资料表 school_course_ware
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course_ware]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course_ware] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_CourseId] nvarchar(50) NOT NULL,
		[F_Name] nvarchar(200) NOT NULL,
		[F_FileUrl] nvarchar(500) NOT NULL,
		[F_FileSize] bigint NULL,
		[F_FileExt] nvarchar(20) NULL,
		[F_DownCount] int NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE_WARE] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 6. 课时学习进度表 school_course_progress
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_course_progress]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_course_progress] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_LessonId] nvarchar(50) NOT NULL,
		[F_UserId] nvarchar(50) NOT NULL,
		[F_Position] int NULL,
		[F_Duration] int NULL,
		[F_LastTime] datetime2 NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_COURSE_PROGRESS] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_COURSE_PROGRESS_LU] ON [dbo].[school_course_progress] ([F_LessonId] ASC, [F_UserId] ASC)
END
GO

-- =============================================================
-- 7. 菜单（挂在"教学管理"目录下，找不到则挂到根节点）
-- =============================================================
DECLARE @parentId nvarchar(50), @parentLayer int
SELECT TOP 1 @parentId = F_Id, @parentLayer = ISNULL(F_Layers, 1)
FROM sys_module
WHERE F_FullName = N'教学管理' AND ISNULL(F_DeleteMark, 0) = 0
ORDER BY F_Layers

IF @parentId IS NULL
BEGIN
	SET @parentId = '0'
	SET @parentLayer = 0
END

-- 课程分类
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a10001-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Icon], [F_UrlAddress], [F_Target], [F_IsMenu], [F_IsExpand], [F_IsFields], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_Description], [F_CreatorTime])
	VALUES (N'c0a10001-0000-4000-8000-000000000001', @parentId, @parentLayer + 1, N'CourseCategory', N'课程分类', N'fa fa-tags', N'/InstructionalManagement/CourseCategory/Index', N'iframe', 1, 0, 0, 0, 0, 0, 1, 0, 1, N'课程分类管理', GETDATE())
END
GO

-- 课程管理
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a10001-0000-4000-8000-000000000002')
BEGIN
	DECLARE @parentId2 nvarchar(50), @parentLayer2 int
	SELECT TOP 1 @parentId2 = F_Id, @parentLayer2 = ISNULL(F_Layers, 1) FROM sys_module WHERE F_FullName = N'教学管理' AND ISNULL(F_DeleteMark, 0) = 0 ORDER BY F_Layers
	IF @parentId2 IS NULL BEGIN SET @parentId2 = '0' SET @parentLayer2 = 0 END
	INSERT INTO [dbo].[sys_module] ([F_Id], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Icon], [F_UrlAddress], [F_Target], [F_IsMenu], [F_IsExpand], [F_IsFields], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_Description], [F_CreatorTime])
	VALUES (N'c0a10001-0000-4000-8000-000000000002', @parentId2, @parentLayer2 + 1, N'CourseManage', N'课程管理', N'fa fa-book', N'/InstructionalManagement/Course/Index', N'iframe', 1, 0, 0, 0, 0, 0, 2, 0, 1, N'课程新增/编辑/上下架/章节/资料', GETDATE())
END
GO

-- 在线学习
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a10001-0000-4000-8000-000000000003')
BEGIN
	DECLARE @parentId3 nvarchar(50), @parentLayer3 int
	SELECT TOP 1 @parentId3 = F_Id, @parentLayer3 = ISNULL(F_Layers, 1) FROM sys_module WHERE F_FullName = N'教学管理' AND ISNULL(F_DeleteMark, 0) = 0 ORDER BY F_Layers
	IF @parentId3 IS NULL BEGIN SET @parentId3 = '0' SET @parentLayer3 = 0 END
	INSERT INTO [dbo].[sys_module] ([F_Id], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Icon], [F_UrlAddress], [F_Target], [F_IsMenu], [F_IsExpand], [F_IsFields], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_Description], [F_CreatorTime])
	VALUES (N'c0a10001-0000-4000-8000-000000000003', @parentId3, @parentLayer3 + 1, N'CourseStudy', N'在线学习', N'fa fa-play-circle', N'/InstructionalManagement/CourseStudy/Index', N'iframe', 1, 0, 0, 0, 0, 0, 3, 0, 1, N'学生端在线学习', GETDATE())
END
GO

-- =============================================================
-- 8. 功能按钮
-- =============================================================
-- 课程分类按钮
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000101')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000101', N'c0a10001-0000-4000-8000-000000000001', N'0', 1, N'NF-add', N'新增分类', 1, N'add', N'/InstructionalManagement/CourseCategory/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000102')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000102', N'c0a10001-0000-4000-8000-000000000001', N'0', 1, N'NF-edit', N'修改分类', 2, N'edit', N'/InstructionalManagement/CourseCategory/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000103')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000103', N'c0a10001-0000-4000-8000-000000000001', N'0', 1, N'NF-delete', N'删除分类', 2, N'delete', N'/InstructionalManagement/CourseCategory/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- 课程管理按钮
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000201')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000201', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-add', N'新增课程', 1, N'add', N'/InstructionalManagement/Course/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000202')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000202', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-edit', N'修改课程', 2, N'edit', N'/InstructionalManagement/Course/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000203')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000203', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-chapter', N'章节管理', 2, N'chapter', N'/InstructionalManagement/Course/Chapter', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000204')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000204', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-ware', N'资料管理', 2, N'ware', N'/InstructionalManagement/Course/Ware', 0, 0, 0, 0, 4, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000205')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000205', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-online', N'上架', 2, N'online', N'/InstructionalManagement/Course/SetStatus', 0, 0, 0, 0, 5, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000206')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000206', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-offline', N'下架', 2, N'offline', N'/InstructionalManagement/Course/SetStatus', 0, 0, 0, 0, 6, 0, 1, GETDATE())
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10002-0000-4000-8000-000000000207')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10002-0000-4000-8000-000000000207', N'c0a10001-0000-4000-8000-000000000002', N'0', 1, N'NF-delete', N'删除课程', 2, N'delete', N'/InstructionalManagement/Course/DeleteForm', 0, 0, 0, 0, 7, 0, 1, GETDATE())
GO

-- =============================================================
-- 9. 角色授权（默认授权给管理员类角色，其他角色请在"系统管理-角色权限"中配置）
-- =============================================================
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_role r
CROSS JOIN sys_module m
WHERE ISNULL(r.F_DeleteMark, 0) = 0
  AND (r.F_EnCode IN (N'admin', N'administrators') OR r.F_FullName LIKE N'%管理员%')
  AND m.F_Id IN (N'c0a10001-0000-4000-8000-000000000001', N'c0a10001-0000-4000-8000-000000000002', N'c0a10001-0000-4000-8000-000000000003')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_role r
CROSS JOIN sys_modulebutton b
WHERE ISNULL(r.F_DeleteMark, 0) = 0
  AND (r.F_EnCode IN (N'admin', N'administrators') OR r.F_FullName LIKE N'%管理员%')
  AND b.F_ModuleId IN (N'c0a10001-0000-4000-8000-000000000001', N'c0a10001-0000-4000-8000-000000000002')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'课程管理模块初始化完成'
GO
