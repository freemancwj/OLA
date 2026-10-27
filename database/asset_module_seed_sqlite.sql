-- =============================================================
-- 学校资产管理模块 - 菜单与授权种子 (SQLite)
-- =============================================================

-- 一级菜单
INSERT INTO sys_module (F_Id,F_ParentId,F_Layers,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_Target,F_IsMenu,F_IsExpand,F_IsFields,F_IsPublic,F_AllowEdit,F_AllowDelete,F_SortCode,F_DeleteMark,F_EnabledMark,F_Description,F_CreatorTime)
SELECT 'c0b80001-0000-4000-8000-000000000001','87dc2de3-ccbc-4dab-bb90-89fc68cbde4f',2,'AssetManagement','资产管理','fa fa-cubes',NULL,'expand',1,1,0,0,0,0,7,0,1,'学校资产管理',datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id='c0b80001-0000-4000-8000-000000000001');

-- 子菜单
INSERT INTO sys_module (F_Id,F_ParentId,F_Layers,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_Target,F_IsMenu,F_IsExpand,F_IsFields,F_IsPublic,F_AllowEdit,F_AllowDelete,F_SortCode,F_DeleteMark,F_EnabledMark,F_Description,F_CreatorTime)
SELECT s.Id,'c0b80001-0000-4000-8000-000000000001',3,s.EnCode,s.Name,s.Icon,s.Url,'iframe',1,0,0,0,0,0,s.Sort,0,1,s.Name,datetime('now')
FROM (
  SELECT 'c0b80002-0000-4000-8000-000000000001' AS Id,'AstGrade' AS EnCode,'年级管理' AS Name,'fa fa-graduation-cap' AS Icon,'/AssetManagement/AstGrade/Index' AS Url,1 AS Sort UNION ALL
  SELECT 'c0b80003-0000-4000-8000-000000000001','AstClass','班级管理','fa fa-users','/AssetManagement/AstClass/Index',2 UNION ALL
  SELECT 'c0b80004-0000-4000-8000-000000000001','AstRoom','功能室/教室','fa fa-building','/AssetManagement/AstRoom/Index',3 UNION ALL
  SELECT 'c0b80005-0000-4000-8000-000000000001','AstCategory','资产分类','fa fa-tags','/AssetManagement/AstCategory/Index',4 UNION ALL
  SELECT 'c0b80006-0000-4000-8000-000000000001','AstSupplier','供应商','fa fa-truck','/AssetManagement/AstSupplier/Index',5 UNION ALL
  SELECT 'c0b80007-0000-4000-8000-000000000001','AstAsset','资产卡片','fa fa-archive','/AssetManagement/AstAsset/Index',6 UNION ALL
  SELECT 'c0b80008-0000-4000-8000-000000000001','AstBorrow','器材借用','fa fa-exchange','/AssetManagement/AstBorrow/Index',7 UNION ALL
  SELECT 'c0b80009-0000-4000-8000-000000000001','AstRepair','公物报修','fa fa-wrench','/AssetManagement/AstRepair/Index',8 UNION ALL
  SELECT 'c0b8000a-0000-4000-8000-000000000001','AstMaintenance','设备维保','fa fa-cog','/AssetManagement/AstMaintenance/Index',9 UNION ALL
  SELECT 'c0b8000b-0000-4000-8000-000000000001','AstConsumable','低值耗材','fa fa-pencil','/AssetManagement/AstConsumable/Index',10 UNION ALL
  SELECT 'c0b8000c-0000-4000-8000-000000000001','AstInventory','学期盘点','fa fa-check-square','/AssetManagement/AstInventory/Index',11 UNION ALL
  SELECT 'c0b8000d-0000-4000-8000-000000000001','AstReport','资产报表','fa fa-bar-chart','/AssetManagement/AstReport/Index',12
) s
WHERE NOT EXISTS (SELECT 1 FROM sys_module x WHERE x.F_Id=s.Id);

-- 标准按钮（增/改/删）
INSERT INTO sys_modulebutton (F_Id,F_ModuleId,F_ParentId,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_SortCode,F_EnabledMark,F_DeleteMark,F_CreatorTime)
SELECT lower(hex(randomblob(16))), m.ModuleId, NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, datetime('now')
FROM (
  SELECT 'c0b80002-0000-4000-8000-000000000001' AS ModuleId UNION ALL SELECT 'c0b80003-0000-4000-8000-000000000001' UNION ALL
  SELECT 'c0b80004-0000-4000-8000-000000000001' UNION ALL SELECT 'c0b80005-0000-4000-8000-000000000001' UNION ALL
  SELECT 'c0b80006-0000-4000-8000-000000000001' UNION ALL SELECT 'c0b80007-0000-4000-8000-000000000001' UNION ALL
  SELECT 'c0b80008-0000-4000-8000-000000000001' UNION ALL SELECT 'c0b80009-0000-4000-8000-000000000001' UNION ALL
  SELECT 'c0b8000a-0000-4000-8000-000000000001' UNION ALL SELECT 'c0b8000b-0000-4000-8000-000000000001' UNION ALL
  SELECT 'c0b8000c-0000-4000-8000-000000000001'
) m
CROSS JOIN (
  SELECT 'NF-add' AS EnCode,'新增' AS FullName,1 AS SortCode UNION ALL
  SELECT 'NF-edit','修改',2 UNION ALL
  SELECT 'NF-delete','删除',3
) b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId=m.ModuleId AND x.F_EnCode=b.EnCode AND ifnull(x.F_DeleteMark,0)=0);

-- 盘点额外按钮
INSERT INTO sys_modulebutton (F_Id,F_ModuleId,F_ParentId,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_SortCode,F_EnabledMark,F_DeleteMark,F_CreatorTime)
SELECT lower(hex(randomblob(16))),'c0b8000c-0000-4000-8000-000000000001',NULL,'NF-details','明细',1,NULL,NULL,1,0,datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId='c0b8000c-0000-4000-8000-000000000001' AND F_EnCode='NF-details' AND ifnull(F_DeleteMark,0)=0);

INSERT INTO sys_modulebutton (F_Id,F_ModuleId,F_ParentId,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_SortCode,F_EnabledMark,F_DeleteMark,F_CreatorTime)
SELECT lower(hex(randomblob(16))),'c0b8000c-0000-4000-8000-000000000001',NULL,'NF-approve','完成',1,NULL,NULL,1,0,datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId='c0b8000c-0000-4000-8000-000000000001' AND F_EnCode='NF-approve' AND ifnull(F_DeleteMark,0)=0);

-- 报表导出按钮
INSERT INTO sys_modulebutton (F_Id,F_ModuleId,F_ParentId,F_EnCode,F_FullName,F_Icon,F_UrlAddress,F_SortCode,F_EnabledMark,F_DeleteMark,F_CreatorTime)
SELECT lower(hex(randomblob(16))),'c0b8000d-0000-4000-8000-000000000001',NULL,'NF-export','导出',1,NULL,NULL,1,0,datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId='c0b8000d-0000-4000-8000-000000000001' AND F_EnCode='NF-export' AND ifnull(F_DeleteMark,0)=0);

-- 角色授权：一级菜单
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))),1,'c0b80001-0000-4000-8000-000000000001',1,r.F_Id,0,datetime('now')
FROM sys_role r
WHERE ifnull(r.F_DeleteMark,0)=0 AND r.F_EnCode IN ('R_01','R_02','G_01','R_05','G_02','R_07','R_08','G_03','R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId='c0b80001-0000-4000-8000-000000000001' AND ra.F_ObjectId=r.F_Id);

-- 子菜单授权
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))),1,m.F_Id,1,r.F_Id,0,datetime('now')
FROM sys_role r
CROSS JOIN sys_module m
WHERE ifnull(r.F_DeleteMark,0)=0 AND r.F_EnCode IN ('R_01','R_02','G_01','R_05','G_02','R_07','R_08','G_03','R_06')
  AND m.F_ParentId='c0b80001-0000-4000-8000-000000000001' AND ifnull(m.F_DeleteMark,0)=0
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId=m.F_Id AND ra.F_ObjectId=r.F_Id);

-- 按钮授权
INSERT INTO sys_roleauthorize (F_Id,F_ItemType,F_ItemId,F_ObjectType,F_ObjectId,F_SortCode,F_CreatorTime)
SELECT lower(hex(randomblob(16))),2,mb.F_Id,1,r.F_Id,0,datetime('now')
FROM sys_role r
CROSS JOIN sys_modulebutton mb
INNER JOIN sys_module m ON mb.F_ModuleId=m.F_Id
WHERE ifnull(r.F_DeleteMark,0)=0 AND r.F_EnCode IN ('R_01','R_02','G_01','R_05','G_02','R_07','R_08','G_03','R_06')
  AND m.F_ParentId='c0b80001-0000-4000-8000-000000000001' AND ifnull(mb.F_DeleteMark,0)=0
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId=mb.F_Id AND ra.F_ObjectId=r.F_Id);
