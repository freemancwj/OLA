-- =============================================================
-- 大学教务扩展·批次3：选课管理 初始化脚本（幂等，可重复执行）
-- 内容：选课批次/选课设置(课程班次)/选课记录 + 学生端选课 + 菜单按钮授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 选课批次
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_elective_round')
BEGIN
	CREATE TABLE [dbo].[school_elective_round](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_Semester] [nvarchar](50) NULL,
		[F_Name] [nvarchar](100) NULL,
		[F_StartTime] [datetime2](7) NULL,
		[F_EndTime] [datetime2](7) NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_elective_round] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 选课设置（批次下开放的课程班次）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_elective_course')
BEGIN
	CREATE TABLE [dbo].[school_elective_course](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_RoundId] [nvarchar](50) NULL,
		[F_CourseId] [nvarchar](50) NULL,
		[F_TeacherId] [nvarchar](50) NULL,
		[F_Capacity] [int] NULL,
		[F_WeekDay] [int] NULL,
		[F_Section] [int] NULL,
		[F_ClassroomId] [nvarchar](50) NULL,
		[F_Remark] [nvarchar](500) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_elective_course] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_elective_course_round' AND object_id = OBJECT_ID('school_elective_course'))
	CREATE NONCLUSTERED INDEX [IX_school_elective_course_round] ON [dbo].[school_elective_course] ([F_RoundId] ASC)
GO

-- -------------------------------------------------------------
-- 3. 选课记录
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'school_elective_record')
BEGIN
	CREATE TABLE [dbo].[school_elective_record](
		[F_Id] [nvarchar](50) NOT NULL,
		[F_RoundId] [nvarchar](50) NULL,
		[F_ElectiveCourseId] [nvarchar](50) NULL,
		[F_StudentId] [nvarchar](50) NULL,
		[F_Status] [int] NULL,
		[F_SelectTime] [datetime2](7) NULL,
		[F_DropTime] [datetime2](7) NULL,
		[F_EnabledMark] [tinyint] NULL,
		[F_DeleteMark] [tinyint] NULL,
		[F_CreatorTime] [datetime2](7) NULL,
		[F_CreatorUserId] [nvarchar](50) NULL,
		[F_LastModifyTime] [datetime2](7) NULL,
		[F_LastModifyUserId] [nvarchar](50) NULL,
		[F_DeleteTime] [datetime2](7) NULL,
		[F_DeleteUserId] [nvarchar](50) NULL,
		CONSTRAINT [PK_school_elective_record] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_elective_record_sc' AND object_id = OBJECT_ID('school_elective_record'))
	CREATE NONCLUSTERED INDEX [IX_school_elective_record_sc] ON [dbo].[school_elective_record] ([F_StudentId] ASC, [F_ElectiveCourseId] ASC)
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_elective_record_ec' AND object_id = OBJECT_ID('school_elective_record'))
	CREATE NONCLUSTERED INDEX [IX_school_elective_record_ec] ON [dbo].[school_elective_record] ([F_ElectiveCourseId] ASC, [F_Status] ASC)
GO

-- -------------------------------------------------------------
-- 4. 菜单（教学管理 08dafb93-99da-47e5-883f-8ff1acc7a050 下）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ElectiveRound' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a53001-0000-4000-8000-0000000000a1', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ElectiveRound', N'选课批次', N'fa fa-clock-o', N'/InstructionalManagement/ElectiveRound/Index', N'iframe', 1, 0, 0, 1, 1, 38, 0, 1, N'选课批次时间窗口管理', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ElectiveCourse' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a53002-0000-4000-8000-0000000000a2', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ElectiveCourse', N'选课设置', N'fa fa-list-alt', N'/InstructionalManagement/ElectiveCourse/Index', N'iframe', 1, 0, 0, 1, 1, 39, 0, 1, N'批次下开放课程班次与容量', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ElectiveRecord' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a53003-0000-4000-8000-0000000000a3', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ElectiveRecord', N'选课名单', N'fa fa-users', N'/InstructionalManagement/ElectiveRecord/Index', N'iframe', 1, 0, 0, 1, 1, 40, 0, 1, N'选课名单查询与移除', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'MyElective' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a53004-0000-4000-8000-0000000000a4', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'MyElective', N'学生选课', N'fa fa-hand-pointer-o', N'/InstructionalManagement/MyElective/Index', N'iframe', 1, 0, 0, 1, 1, 41, 0, 1, N'学生在线选课退课', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 5. 按钮（选课批次/选课设置 三按钮；选课名单 仅移除）
-- -------------------------------------------------------------
DECLARE @btn_modules TABLE (F_ModuleId nvarchar(50), Prefix nvarchar(50))
INSERT INTO @btn_modules (F_ModuleId, Prefix) VALUES
	(N'c0a53001-0000-4000-8000-0000000000a1', N'ElectiveRound'),
	(N'c0a53002-0000-4000-8000-0000000000a2', N'ElectiveCourse')

DECLARE @mid nvarchar(50), @prefix nvarchar(50)
DECLARE btn_cursor CURSOR FOR SELECT F_ModuleId, Prefix FROM @btn_modules
OPEN btn_cursor
FETCH NEXT FROM btn_cursor INTO @mid, @prefix
WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = 'NF-add')
		INSERT INTO [dbo].[sys_modulebutton]
			([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_JsEvent],[F_UrlAddress],[F_SortCode],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
		VALUES
			(LOWER(NEWID()), @mid, N'0', 1, N'NF-add', N'新增', N'fa fa-plus', N'btn_add()', NULL, 1, 0, 0, 1, 1, 0, 1, NULL, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = 'NF-edit')
		INSERT INTO [dbo].[sys_modulebutton]
			([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_JsEvent],[F_UrlAddress],[F_SortCode],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
		VALUES
			(LOWER(NEWID()), @mid, N'0', 1, N'NF-edit', N'修改', N'fa fa-pencil', N'btn_edit()', NULL, 2, 0, 0, 1, 1, 0, 1, NULL, GETDATE())
	IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = @mid AND F_EnCode = 'NF-delete')
		INSERT INTO [dbo].[sys_modulebutton]
			([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_JsEvent],[F_UrlAddress],[F_SortCode],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
		VALUES
			(LOWER(NEWID()), @mid, N'0', 1, N'NF-delete', N'删除', N'fa fa-trash-o', N'btn_delete()', NULL, 3, 0, 0, 1, 1, 0, 1, NULL, GETDATE())
	FETCH NEXT FROM btn_cursor INTO @mid, @prefix
END
CLOSE btn_cursor
DEALLOCATE btn_cursor
GO

-- 选课名单 仅移除按钮
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = N'c0a53003-0000-4000-8000-0000000000a3' AND F_EnCode = 'NF-delete')
	INSERT INTO [dbo].[sys_modulebutton]
		([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_JsEvent],[F_UrlAddress],[F_SortCode],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(LOWER(NEWID()), N'c0a53003-0000-4000-8000-0000000000a3', N'0', 1, N'NF-delete', N'移除', N'fa fa-trash-o', N'btn_delete()', NULL, 1, 0, 0, 1, 1, 0, 1, NULL, GETDATE())
GO

-- -------------------------------------------------------------
-- 6. 菜单授权（管理端三菜单 → R_01/R_02/R_04/R_05/R_06；学生选课 → 另加学生角色）
-- -------------------------------------------------------------
DECLARE @menu_ids TABLE (F_ObjectId nvarchar(50))
INSERT INTO @menu_ids (F_ObjectId) VALUES
	(N'c0a53001-0000-4000-8000-0000000000a1'),
	(N'c0a53002-0000-4000-8000-0000000000a2'),
	(N'c0a53003-0000-4000-8000-0000000000a3'),
	(N'c0a53004-0000-4000-8000-0000000000a4')

DECLARE @role_ids TABLE (F_ObjectId nvarchar(50))
INSERT INTO @role_ids (F_ObjectId)
SELECT F_Id FROM sys_role WHERE F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06') AND ISNULL(F_DeleteMark,0) = 0

DECLARE @moid nvarchar(50), @roid nvarchar(50)
DECLARE menu_cursor CURSOR FOR SELECT F_ObjectId FROM @menu_ids
OPEN menu_cursor
FETCH NEXT FROM menu_cursor INTO @moid
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE role_cursor CURSOR FOR SELECT F_ObjectId FROM @role_ids
	OPEN role_cursor
	FETCH NEXT FROM role_cursor INTO @roid
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM sys_roleauthorize WHERE F_ItemType = 1 AND F_ItemId = @moid AND F_ObjectType = 1 AND F_ObjectId = @roid)
			INSERT INTO [dbo].[sys_roleauthorize]
				([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_CreatorTime])
			VALUES
				(LOWER(NEWID()), 1, @moid, 1, @roid, GETDATE())
		FETCH NEXT FROM role_cursor INTO @roid
	END
	CLOSE role_cursor
	DEALLOCATE role_cursor
	FETCH NEXT FROM menu_cursor INTO @moid
END
CLOSE menu_cursor
DEALLOCATE menu_cursor
GO

-- 学生选课菜单授权给学生角色
IF NOT EXISTS (SELECT 1 FROM sys_roleauthorize WHERE F_ItemType = 1 AND F_ItemId = N'c0a53004-0000-4000-8000-0000000000a4' AND F_ObjectType = 1 AND F_ObjectId = N'08da9c6b-f455-46c0-8ffe-81edad14c7be')
	INSERT INTO [dbo].[sys_roleauthorize]
		([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_CreatorTime])
	VALUES
		(LOWER(NEWID()), 1, N'c0a53004-0000-4000-8000-0000000000a4', 1, N'08da9c6b-f455-46c0-8ffe-81edad14c7be', GETDATE())
GO

-- -------------------------------------------------------------
-- 7. 按钮授权（管理端模块按钮 → R_01/R_02/R_04/R_05/R_06）
-- -------------------------------------------------------------
DECLARE @btn2 TABLE (F_ModuleId nvarchar(50))
INSERT INTO @btn2 (F_ModuleId) VALUES
	(N'c0a53001-0000-4000-8000-0000000000a1'),
	(N'c0a53002-0000-4000-8000-0000000000a2'),
	(N'c0a53003-0000-4000-8000-0000000000a3')

DECLARE @bmid nvarchar(50), @bid nvarchar(50), @brid nvarchar(50)
DECLARE bm_cursor CURSOR FOR SELECT F_ModuleId FROM @btn2
OPEN bm_cursor
FETCH NEXT FROM bm_cursor INTO @bmid
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE b_cursor CURSOR FOR SELECT F_Id FROM sys_modulebutton WHERE F_ModuleId = @bmid AND ISNULL(F_DeleteMark,0) = 0
	OPEN b_cursor
	FETCH NEXT FROM b_cursor INTO @bid
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE r_cursor CURSOR FOR SELECT F_Id FROM sys_role WHERE F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06') AND ISNULL(F_DeleteMark,0) = 0
		OPEN r_cursor
		FETCH NEXT FROM r_cursor INTO @brid
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM sys_roleauthorize WHERE F_ItemType = 2 AND F_ItemId = @bid AND F_ObjectType = 1 AND F_ObjectId = @brid)
				INSERT INTO [dbo].[sys_roleauthorize]
					([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_CreatorTime])
				VALUES
					(LOWER(NEWID()), 2, @bid, 1, @brid, GETDATE())
			FETCH NEXT FROM r_cursor INTO @brid
		END
		CLOSE r_cursor
		DEALLOCATE r_cursor
		FETCH NEXT FROM b_cursor INTO @bid
	END
	CLOSE b_cursor
	DEALLOCATE b_cursor
	FETCH NEXT FROM bm_cursor INTO @bmid
END
CLOSE bm_cursor
DEALLOCATE bm_cursor
GO

PRINT N'选课管理模块初始化完成'
GO
