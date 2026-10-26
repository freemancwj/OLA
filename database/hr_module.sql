-- =============================================================
-- 人事管理模块 初始化脚本（幂等，可重复执行）
-- 内容：hr_employee / hr_attendance_rule / hr_attendance_record 建表；
--      人事管理一级菜单；档案管理/打卡规则/打卡记录/员工打卡子菜单及按钮；
--      请假管理菜单移动到人事管理下；授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 建表：员工档案扩展表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'hr_employee')
BEGIN
	CREATE TABLE [dbo].[hr_employee] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Education] int NULL,
		[F_EntryDate] datetime NULL,
		[F_RegularDate] datetime NULL,
		[F_Photo] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_IdCard] nvarchar(20) COLLATE Chinese_PRC_CI_AS NULL,
		[F_MaritalStatus] int NULL,
		[F_PoliticalStatus] int NULL,
		[F_GraduateSchool] nvarchar(100) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Major] nvarchar(100) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EmergencyContact] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EmergencyPhone] nvarchar(20) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Address] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ContractNo] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ContractStart] datetime NULL,
		[F_ContractEnd] datetime NULL,
		[F_LeaveDate] datetime NULL,
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteTime] datetime NULL,
		[F_DeleteUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_hr_employee] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE UNIQUE INDEX [IX_hr_employee_F_UserId] ON [dbo].[hr_employee] ([F_UserId] ASC) WHERE [F_DeleteMark] = 0
END
GO

-- -------------------------------------------------------------
-- 2. 建表：打卡规则表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'hr_attendance_rule')
BEGIN
	CREATE TABLE [dbo].[hr_attendance_rule] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_StartTime] nvarchar(5) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EndTime] nvarchar(5) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LateGrace] int NULL DEFAULT 10,
		[F_EarlyGrace] int NULL DEFAULT 10,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_hr_attendance_rule] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 3. 建表：打卡记录表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'hr_attendance_record')
BEGIN
	CREATE TABLE [dbo].[hr_attendance_record] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_UserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Date] datetime NULL,
		[F_ClockInTime] datetime NULL,
		[F_ClockInStatus] int NULL DEFAULT 0,
		[F_ClockOutTime] datetime NULL,
		[F_ClockOutStatus] int NULL DEFAULT 0,
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_CreatorTime] datetime NULL,
		CONSTRAINT [PK_hr_attendance_record] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE INDEX [IX_hr_attendance_record_F_UserId_F_Date] ON [dbo].[hr_attendance_record] ([F_UserId] ASC, [F_Date] ASC)
END
GO

-- -------------------------------------------------------------
-- 4. 人事管理一级菜单（与教学管理平级，SortCode=5）
-- -------------------------------------------------------------
DECLARE @hrMenuId nvarchar(50) = 'c0a60001-0000-4000-8000-000000000001'
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @hrMenuId)
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (@hrMenuId, '87dc2de3-ccbc-4dab-bb90-89fc68cbde4f', 2, N'HRManagement', N'人事管理', N'fa fa-users', NULL, N'expand', 1, 1, 0, 0, 0, 0, 5, 0, 1, N'人事管理模块', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 5. 人事管理子菜单
-- -------------------------------------------------------------
DECLARE @hrMenuId nvarchar(50) = 'c0a60001-0000-4000-8000-000000000001'

-- 5.1 档案管理
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a60002-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a60002-0000-4000-8000-000000000001', @hrMenuId, 3, N'HrEmployee', N'档案管理', N'fa fa-id-card', N'/HRManagement/HrEmployee/Index', N'iframe', 1, 0, 0, 0, 0, 0, 1, 0, 1, N'员工档案管理', GETDATE())
END

-- 5.2 打卡规则
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a60003-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a60003-0000-4000-8000-000000000001', @hrMenuId, 3, N'AttendanceRule', N'打卡规则', N'fa fa-cog', N'/HRManagement/AttendanceRule/Index', N'iframe', 1, 0, 0, 0, 0, 0, 2, 0, 1, N'考勤打卡规则设置', GETDATE())
END

-- 5.3 打卡记录
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a60004-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a60004-0000-4000-8000-000000000001', @hrMenuId, 3, N'AttendanceRecord', N'打卡记录', N'fa fa-table', N'/HRManagement/AttendanceRecord/Index', N'iframe', 1, 0, 0, 0, 0, 0, 3, 0, 1, N'员工打卡记录查询', GETDATE())
END

-- 5.4 员工打卡
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a60005-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a60005-0000-4000-8000-000000000001', @hrMenuId, 3, N'AttendanceClock', N'员工打卡', N'fa fa-clock-o', N'/HRManagement/AttendanceClock/Index', N'iframe', 1, 0, 0, 0, 0, 0, 4, 0, 1, N'员工上下班打卡', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 6. 请假管理菜单移动到人事管理下
-- -------------------------------------------------------------
UPDATE sys_module
SET F_ParentId = 'c0a60001-0000-4000-8000-000000000001',
    F_Layers = 3,
    F_SortCode = 5
WHERE F_Id = '08ddd5b3-451b-426b-889b-ef2f40abb3e3'
GO

-- -------------------------------------------------------------
-- 7. 档案管理菜单按钮
-- -------------------------------------------------------------
DECLARE @btns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns (EnCode, FullName, SortCode) VALUES
 (N'NF-add', N'新增档案', 1),
 (N'NF-edit', N'编辑档案', 2),
 (N'NF-delete', N'删除档案', 3)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), 'c0a60002-0000-4000-8000-000000000001', NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = 'c0a60002-0000-4000-8000-000000000001' AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO
UPDATE sys_modulebutton SET F_DeleteMark = 0
WHERE F_ModuleId = 'c0a60002-0000-4000-8000-000000000001'
GO

-- -------------------------------------------------------------
-- 8. 授权
-- -------------------------------------------------------------
-- 8.1 人事管理菜单及档案管理/打卡规则/打卡记录：管理员角色（G_02系统管理员）
DECLARE @adminRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @adminRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0 AND F_EnCode IN (N'G_02', N'G_03')

-- 一级菜单
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a60001-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a60001-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

-- 档案管理
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a60002-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a60002-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

-- 打卡规则
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a60003-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a60003-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

-- 打卡记录
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a60004-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a60004-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

-- 档案管理按钮
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
CROSS JOIN (SELECT F_Id FROM sys_modulebutton WHERE F_ModuleId = 'c0a60002-0000-4000-8000-000000000001' AND ISNULL(F_DeleteMark,0) = 0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)
GO

-- 8.2 员工打卡菜单：所有有效角色
DECLARE @allRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @allRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a60005-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @allRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a60005-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)
GO
