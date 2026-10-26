-- =============================================================
-- 教务管理 + 后勤管理模块 初始化脚本（幂等，可重复执行）
-- 内容：ea_teaching_activity / lg_purchase / lg_contract 建表；
--      教务管理一级菜单；教学楼/教室/考务安排/考试时间段/监考/补考/清考移动到教务管理下；
--      教研活动培训管理子菜单；后勤管理一级菜单；采购管理/合同管理子菜单及按钮；授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 建表：教研活动培训通知表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ea_teaching_activity')
BEGIN
	CREATE TABLE [dbo].[ea_teaching_activity] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_Title] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Type] int NULL DEFAULT 1,
		[F_StartTime] datetime NULL,
		[F_EndTime] datetime NULL,
		[F_Location] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Speaker] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Content] nvarchar(max) COLLATE Chinese_PRC_CI_AS NULL,
		[F_FileUrl] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_FileName] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteTime] datetime NULL,
		[F_DeleteUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_ea_teaching_activity] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 建表：采购申请表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'lg_purchase')
BEGIN
	CREATE TABLE [dbo].[lg_purchase] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_ItemName] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Spec] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Quantity] decimal(18,2) NULL,
		[F_Unit] nvarchar(20) COLLATE Chinese_PRC_CI_AS NULL,
		[F_BudgetAmount] decimal(18,2) NULL,
		[F_Supplier] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Status] int NULL DEFAULT 0,
		[F_ApplyDate] datetime NULL,
		[F_ApplicantId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ApplicantName] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteTime] datetime NULL,
		[F_DeleteUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_lg_purchase] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 3. 建表：合同表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'lg_contract')
BEGIN
	CREATE TABLE [dbo].[lg_contract] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_ContractNo] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Name] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_PartyA] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_PartyB] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Amount] decimal(18,2) NULL,
		[F_SignDate] datetime NULL,
		[F_ExpireDate] datetime NULL,
		[F_Status] int NULL DEFAULT 0,
		[F_FileUrl] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_FileName] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_EnabledMark] tinyint NULL DEFAULT 1,
		[F_DeleteMark] tinyint NULL DEFAULT 0,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_DeleteTime] datetime NULL,
		[F_DeleteUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_lg_contract] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 4. 教务管理一级菜单（与人事管理平级，SortCode=6）
-- -------------------------------------------------------------
DECLARE @eaMenuId nvarchar(50) = 'c0a70001-0000-4000-8000-000000000001'
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @eaMenuId)
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (@eaMenuId, '87dc2de3-ccbc-4dab-bb90-89fc68cbde4f', 2, N'EducationalAffairs', N'教务管理', N'fa fa-graduation-cap', NULL, N'expand', 1, 1, 0, 0, 0, 0, 6, 0, 1, N'教务管理模块', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 5. 教学管理下的7个菜单移动到教务管理下
-- -------------------------------------------------------------
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 1 WHERE F_Id = 'c0a51002-0000-4000-8000-000000000001' -- 教学楼管理
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 2 WHERE F_Id = 'c0a51003-0000-4000-8000-000000000001' -- 教室管理
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 3 WHERE F_Id = 'c0a51005-0000-4000-8000-000000000001' -- 考务安排
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 4 WHERE F_Id = 'c0a51004-0000-4000-8000-000000000001' -- 考试时间段
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 5 WHERE F_Id = 'c0a51006-0000-4000-8000-000000000001' -- 监考管理
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 6 WHERE F_Id = 'c0a51007-0000-4000-8000-000000000001' -- 补考管理
UPDATE sys_module SET F_ParentId = 'c0a70001-0000-4000-8000-000000000001', F_Layers = 3, F_SortCode = 7 WHERE F_Id = 'c0a51008-0000-4000-8000-000000000001' -- 清考管理
GO

-- -------------------------------------------------------------
-- 6. 教研活动培训管理子菜单
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a70002-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a70002-0000-4000-8000-000000000001', 'c0a70001-0000-4000-8000-000000000001', 3, N'TeachingActivity', N'教研活动培训', N'fa fa-bullhorn', N'/EducationalAffairs/TeachingActivity/Index', N'iframe', 1, 0, 0, 0, 0, 0, 8, 0, 1, N'发布教研和培训活动通知', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 7. 后勤管理一级菜单（SortCode=7）
-- -------------------------------------------------------------
DECLARE @lgMenuId nvarchar(50) = 'c0a80001-0000-4000-8000-000000000001'
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @lgMenuId)
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (@lgMenuId, '87dc2de3-ccbc-4dab-bb90-89fc68cbde4f', 2, N'LogisticsManagement', N'后勤管理', N'fa fa-truck', NULL, N'expand', 1, 1, 0, 0, 0, 0, 7, 0, 1, N'后勤管理模块', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 8. 后勤管理子菜单
-- -------------------------------------------------------------
-- 8.1 采购管理
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a80002-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a80002-0000-4000-8000-000000000001', 'c0a80001-0000-4000-8000-000000000001', 3, N'PurchaseManage', N'采购管理', N'fa fa-shopping-cart', N'/LogisticsManagement/Purchase/Index', N'iframe', 1, 0, 0, 0, 0, 0, 1, 0, 1, N'采购申请管理', GETDATE())
END

-- 8.2 合同管理
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a80003-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a80003-0000-4000-8000-000000000001', 'c0a80001-0000-4000-8000-000000000001', 3, N'ContractManage', N'合同管理', N'fa fa-file-text', N'/LogisticsManagement/Contract/Index', N'iframe', 1, 0, 0, 0, 0, 0, 2, 0, 1, N'合同信息管理', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 9. 子菜单按钮（教研活动/采购/合同）
-- -------------------------------------------------------------
DECLARE @btns TABLE (ModuleId nvarchar(50) COLLATE Chinese_PRC_CI_AS, EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns (ModuleId, EnCode, FullName, SortCode) VALUES
 (N'c0a70002-0000-4000-8000-000000000001', N'NF-add', N'发布通知', 1),
 (N'c0a70002-0000-4000-8000-000000000001', N'NF-edit', N'修改通知', 2),
 (N'c0a70002-0000-4000-8000-000000000001', N'NF-delete', N'删除通知', 3),
 (N'c0a80002-0000-4000-8000-000000000001', N'NF-add', N'新增采购', 1),
 (N'c0a80002-0000-4000-8000-000000000001', N'NF-edit', N'修改采购', 2),
 (N'c0a80002-0000-4000-8000-000000000001', N'NF-delete', N'删除采购', 3),
 (N'c0a80003-0000-4000-8000-000000000001', N'NF-add', N'新增合同', 1),
 (N'c0a80003-0000-4000-8000-000000000001', N'NF-edit', N'修改合同', 2),
 (N'c0a80003-0000-4000-8000-000000000001', N'NF-delete', N'删除合同', 3)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), b.ModuleId, NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = b.ModuleId AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO
UPDATE sys_modulebutton SET F_DeleteMark = 0
WHERE F_ModuleId IN ('c0a70002-0000-4000-8000-000000000001', 'c0a80002-0000-4000-8000-000000000001', 'c0a80003-0000-4000-8000-000000000001')
GO

-- -------------------------------------------------------------
-- 10. 授权：管理员角色（G_02系统管理员、G_03管理员）
-- -------------------------------------------------------------
DECLARE @adminRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @adminRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0 AND F_EnCode IN (N'G_02', N'G_03')

-- 10.1 教务管理/后勤管理一级菜单
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
CROSS JOIN (SELECT 'c0a70001-0000-4000-8000-000000000001' AS F_Id UNION ALL SELECT 'c0a80001-0000-4000-8000-000000000001') m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.F_Id AND ra.F_ObjectId = s.RoleId)

-- 10.2 教研活动/采购/合同子菜单
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
CROSS JOIN (SELECT 'c0a70002-0000-4000-8000-000000000001' AS F_Id UNION ALL SELECT 'c0a80002-0000-4000-8000-000000000001' UNION ALL SELECT 'c0a80003-0000-4000-8000-000000000001') m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.F_Id AND ra.F_ObjectId = s.RoleId)

-- 10.3 按钮
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @adminRoles s
CROSS JOIN (SELECT F_Id FROM sys_modulebutton WHERE F_ModuleId IN ('c0a70002-0000-4000-8000-000000000001', 'c0a80002-0000-4000-8000-000000000001', 'c0a80003-0000-4000-8000-000000000001') AND ISNULL(F_DeleteMark,0) = 0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)
GO
