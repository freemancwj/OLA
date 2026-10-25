-- =============================================================
-- 教师任课班级模块初始化脚本（幂等，可重复执行）
-- 功能：维护教师-班级映射，学生管理/成绩管理按任课班级过滤数据
-- =============================================================

-- -------------------------------------------------------------
-- 1. 教师任课班级表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_teacher_class')
BEGIN
	CREATE TABLE [dbo].[school_teacher_class](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_TeacherId] [nvarchar](50) NULL,
		[F_ClassId] [nvarchar](50) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_teacher_class] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- 同一教师同一班级仅一条有效记录
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_teacher_class_tc' AND object_id = OBJECT_ID('school_teacher_class'))
	CREATE UNIQUE NONCLUSTERED INDEX [IX_school_teacher_class_tc] ON [dbo].[school_teacher_class] ([F_TeacherId] ASC, [F_ClassId] ASC)
	WHERE [F_DeleteMark] = 0
GO

-- -------------------------------------------------------------
-- 2. 菜单：任课管理（挂在教学管理下）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a40001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a40001-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'TeacherClass', N'任课管理', N'fa fa-users', N'/InstructionalManagement/TeacherClass/Index', N'iframe', 1, 0, 0, 0, 0, 0, 11, 0, 1, N'教师任课班级维护，控制教师可见数据范围', GETDATE())
GO

-- -------------------------------------------------------------
-- 3. 按钮：新增/修改/删除
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a41001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a41001-0000-4000-8000-000000000001', N'c0a40001-0000-4000-8000-000000000001', N'0', 1, N'NF-add', N'新增任课', 1, N'add', N'/InstructionalManagement/TeacherClass/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a41001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a41001-0000-4000-8000-000000000002', N'c0a40001-0000-4000-8000-000000000001', N'0', 1, N'NF-edit', N'修改任课', 2, N'edit', N'/InstructionalManagement/TeacherClass/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a41001-0000-4000-8000-000000000003')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a41001-0000-4000-8000-000000000003', N'c0a40001-0000-4000-8000-000000000001', N'0', 1, N'NF-delete', N'删除任课', 2, N'delete', N'/InstructionalManagement/TeacherClass/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- -------------------------------------------------------------
-- 4. 授权：教师类角色（R_01 系统管理员、R_02 普通管理员、R_04 老师、R_05 校长、R_06 班主任）
-- -------------------------------------------------------------
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_module m
CROSS JOIN sys_role r
WHERE m.F_Id = N'c0a40001-0000-4000-8000-000000000001'
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_modulebutton b
CROSS JOIN sys_role r
WHERE b.F_ModuleId = N'c0a40001-0000-4000-8000-000000000001'
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

PRINT N'教师任课班级模块初始化完成'
GO
