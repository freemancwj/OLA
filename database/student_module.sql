-- =============================================================
-- 学生管理模块初始化脚本（幂等，可重复执行）
-- 说明：
--   学生基本信息沿用主系统 sys_user 表；
--   本脚本只新增学生附加信息表 school_student，
--   并为已存在的"学生管理"菜单补齐增删改按钮及角色授权。
-- =============================================================

-- -------------------------------------------------------------
-- 1. 学生附加信息表 school_student
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_student]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_student] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_UserId] nvarchar(50) NOT NULL,
		[F_StudentNo] nvarchar(50) NULL,
		[F_NativePlace] nvarchar(100) NULL,
		[F_ParentName] nvarchar(50) NULL,
		[F_ParentPhone] nvarchar(20) NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_STUDENT] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE UNIQUE NONCLUSTERED INDEX [UX_SCHOOL_STUDENT_USER] ON [dbo].[school_student] ([F_UserId] ASC)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_STUDENT_NO] ON [dbo].[school_student] ([F_StudentNo] ASC)
END
GO

-- -------------------------------------------------------------
-- 2. 功能按钮（挂在"学生管理"菜单下）
--    学生管理菜单 F_Id = 08dd92e6-b966-429a-8700-d72f96bdaddb
-- -------------------------------------------------------------
DECLARE @moduleId nvarchar(50) = N'08dd92e6-b966-429a-8700-d72f96bdaddb'

IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @moduleId)
BEGIN
	PRINT N'未找到"学生管理"菜单，请先确认菜单已存在'
	RETURN
END

IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10003-0000-4000-8000-000000000301')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10003-0000-4000-8000-000000000301', @moduleId, N'0', 1, N'NF-add', N'新增学生', 1, N'add', N'/SystemOrganize/Student/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
GO

DECLARE @moduleId2 nvarchar(50) = N'08dd92e6-b966-429a-8700-d72f96bdaddb'
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10003-0000-4000-8000-000000000302')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10003-0000-4000-8000-000000000302', @moduleId2, N'0', 1, N'NF-edit', N'修改学生', 2, N'edit', N'/SystemOrganize/Student/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
GO

DECLARE @moduleId3 nvarchar(50) = N'08dd92e6-b966-429a-8700-d72f96bdaddb'
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a10003-0000-4000-8000-000000000303')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id], [F_ModuleId], [F_ParentId], [F_Layers], [F_EnCode], [F_FullName], [F_Location], [F_JsEvent], [F_UrlAddress], [F_Split], [F_IsPublic], [F_AllowEdit], [F_AllowDelete], [F_SortCode], [F_DeleteMark], [F_EnabledMark], [F_CreatorTime])
	VALUES (N'c0a10003-0000-4000-8000-000000000303', @moduleId3, N'0', 1, N'NF-delete', N'删除学生', 2, N'delete', N'/SystemOrganize/Student/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- -------------------------------------------------------------
-- 3. 角色授权
--    3.1 已拥有"学生管理"菜单的角色，自动授予 3 个按钮
--    3.2 管理员类角色自动授予菜单 + 按钮
-- -------------------------------------------------------------
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, ra.F_ObjectId, 0, GETDATE()
FROM sys_roleauthorize ra
CROSS JOIN sys_modulebutton b
WHERE ra.F_ItemType = 1
  AND ra.F_ObjectType = 1
  AND ra.F_ItemId = N'08dd92e6-b966-429a-8700-d72f96bdaddb'
  AND b.F_ModuleId = N'08dd92e6-b966-429a-8700-d72f96bdaddb'
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = ra.F_ObjectId)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 1, N'08dd92e6-b966-429a-8700-d72f96bdaddb', 1, r.F_Id, 0, GETDATE()
FROM sys_role r
WHERE ISNULL(r.F_DeleteMark, 0) = 0
  AND (r.F_EnCode IN (N'admin', N'administrators') OR r.F_FullName LIKE N'%管理员%')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = N'08dd92e6-b966-429a-8700-d72f96bdaddb' AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id], [F_ItemType], [F_ItemId], [F_ObjectType], [F_ObjectId], [F_SortCode], [F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_role r
CROSS JOIN sys_modulebutton b
WHERE ISNULL(r.F_DeleteMark, 0) = 0
  AND (r.F_EnCode IN (N'admin', N'administrators') OR r.F_FullName LIKE N'%管理员%')
  AND b.F_ModuleId = N'08dd92e6-b966-429a-8700-d72f96bdaddb'
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'学生管理模块初始化完成'
GO
