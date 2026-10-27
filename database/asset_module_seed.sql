-- =============================================================
-- 学校资产管理模块 - 菜单与授权种子 (SQL Server)
-- =============================================================

-- 一级菜单：资产管理（SortCode=7）
DECLARE @astMenuId nvarchar(50) = 'c0b80001-0000-4000-8000-000000000001'
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = @astMenuId)
BEGIN
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (@astMenuId, '87dc2de3-ccbc-4dab-bb90-89fc68cbde4f', 2, N'AssetManagement', N'资产管理', N'fa fa-cubes', NULL, N'expand', 1, 1, 0, 0, 0, 0, 7, 0, 1, N'学校资产管理', GETDATE())
END
GO

-- 子菜单
DECLARE @astMenuId nvarchar(50) = 'c0b80001-0000-4000-8000-000000000001'
DECLARE @sub TABLE (Id nvarchar(50) COLLATE Chinese_PRC_CI_AS, EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, Name nvarchar(50) COLLATE Chinese_PRC_CI_AS, Icon nvarchar(50) COLLATE Chinese_PRC_CI_AS, Url nvarchar(200) COLLATE Chinese_PRC_CI_AS, Sort int)
INSERT INTO @sub VALUES
 ('c0b80002-0000-4000-8000-000000000001', N'AstGrade', N'年级管理', N'fa fa-graduation-cap', N'/AssetManagement/AstGrade/Index', 1),
 ('c0b80003-0000-4000-8000-000000000001', N'AstClass', N'班级管理', N'fa fa-users', N'/AssetManagement/AstClass/Index', 2),
 ('c0b80004-0000-4000-8000-000000000001', N'AstRoom', N'功能室/教室', N'fa fa-building', N'/AssetManagement/AstRoom/Index', 3),
 ('c0b80005-0000-4000-8000-000000000001', N'AstCategory', N'资产分类', N'fa fa-tags', N'/AssetManagement/AstCategory/Index', 4),
 ('c0b80006-0000-4000-8000-000000000001', N'AstSupplier', N'供应商', N'fa fa-truck', N'/AssetManagement/AstSupplier/Index', 5),
 ('c0b80007-0000-4000-8000-000000000001', N'AstAsset', N'资产卡片', N'fa fa-archive', N'/AssetManagement/AstAsset/Index', 6),
 ('c0b80008-0000-4000-8000-000000000001', N'AstBorrow', N'器材借用', N'fa fa-exchange', N'/AssetManagement/AstBorrow/Index', 7),
 ('c0b80009-0000-4000-8000-000000000001', N'AstRepair', N'公物报修', N'fa fa-wrench', N'/AssetManagement/AstRepair/Index', 8),
 ('c0b8000a-0000-4000-8000-000000000001', N'AstMaintenance', N'设备维保', N'fa fa-cog', N'/AssetManagement/AstMaintenance/Index', 9),
 ('c0b8000b-0000-4000-8000-000000000001', N'AstConsumable', N'低值耗材', N'fa fa-pencil', N'/AssetManagement/AstConsumable/Index', 10),
 ('c0b8000c-0000-4000-8000-000000000001', N'AstInventory', N'学期盘点', N'fa fa-check-square', N'/AssetManagement/AstInventory/Index', 11),
 ('c0b8000d-0000-4000-8000-000000000001', N'AstReport', N'资产报表', N'fa fa-bar-chart', N'/AssetManagement/AstReport/Index', 12)

INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
SELECT s.Id, @astMenuId, 3, s.EnCode, s.Name, s.Icon, s.Url, N'iframe', 1, 0, 0, 0, 0, 0, s.Sort, 0, 1, s.Name, GETDATE()
FROM @sub s
WHERE NOT EXISTS (SELECT 1 FROM sys_module x WHERE x.F_Id = s.Id)
GO

-- 各菜单的标准按钮（增/改/删）
DECLARE @btnModules TABLE (ModuleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @btnModules VALUES
 ('c0b80002-0000-4000-8000-000000000001'),
 ('c0b80003-0000-4000-8000-000000000001'),
 ('c0b80004-0000-4000-8000-000000000001'),
 ('c0b80005-0000-4000-8000-000000000001'),
 ('c0b80006-0000-4000-8000-000000000001'),
 ('c0b80007-0000-4000-8000-000000000001'),
 ('c0b80008-0000-4000-8000-000000000001'),
 ('c0b80009-0000-4000-8000-000000000001'),
 ('c0b8000a-0000-4000-8000-000000000001'),
 ('c0b8000b-0000-4000-8000-000000000001'),
 ('c0b8000c-0000-4000-8000-000000000001')

DECLARE @btns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns VALUES (N'NF-add', N'新增', 1), (N'NF-edit', N'修改', 2), (N'NF-delete', N'删除', 3)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), m.ModuleId, NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @btnModules m CROSS JOIN @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = m.ModuleId AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO

-- 盘点额外按钮：明细/审批
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = 'c0b8000c-0000-4000-8000-000000000001' AND F_EnCode = N'NF-details' AND ISNULL(F_DeleteMark,0)=0)
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0b8000c-0000-4000-8000-000000000001', NULL, N'NF-details', N'明细', 1, NULL, NULL, 1, 0, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = 'c0b8000c-0000-4000-8000-000000000001' AND F_EnCode = N'NF-approve' AND ISNULL(F_DeleteMark,0)=0)
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0b8000c-0000-4000-8000-000000000001', NULL, N'NF-approve', N'完成', 1, NULL, NULL, 1, 0, GETDATE())
GO

-- 报表按钮：导出
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId = 'c0b8000d-0000-4000-8000-000000000001' AND F_EnCode = N'NF-export' AND ISNULL(F_DeleteMark,0)=0)
BEGIN
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0b8000d-0000-4000-8000-000000000001', NULL, N'NF-export', N'导出', 1, NULL, NULL, 1, 0, GETDATE())
END
GO

-- =============================================================
-- 角色授权：管理角色获得全部资产管理菜单与按钮
-- =============================================================
DECLARE @manageRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @manageRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0 AND F_EnCode IN (N'R_01', N'R_02', N'G_01', N'R_05', N'G_02', N'R_07', N'R_08', N'G_03', N'R_06')

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0b80001-0000-4000-8000-000000000001', 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0b80001-0000-4000-8000-000000000001' AND ra.F_ObjectId = s.RoleId)

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
CROSS JOIN (SELECT F_Id FROM sys_module WHERE F_ParentId = 'c0b80001-0000-4000-8000-000000000001' AND ISNULL(F_DeleteMark,0)=0) m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.F_Id AND ra.F_ObjectId = s.RoleId)

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @manageRoles s
CROSS JOIN (SELECT mb.F_Id FROM sys_modulebutton mb INNER JOIN sys_module m ON mb.F_ModuleId = m.F_Id WHERE m.F_ParentId = 'c0b80001-0000-4000-8000-000000000001' AND ISNULL(mb.F_DeleteMark,0)=0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)
GO
