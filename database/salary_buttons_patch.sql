-- 薪酬模块补充按钮（幂等）：工资单 NF-details/NF-reject、报表 NF-export 及授权
DECLARE @pb TABLE (EnCode nvarchar(50), FullName nvarchar(50), SortCode int)
INSERT INTO @pb VALUES (N'NF-details', N'明细', 5), (N'NF-reject', N'驳回', 6)
INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), 'c0a7000a-0000-4000-8000-000000000001', NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @pb b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId='c0a7000a-0000-4000-8000-000000000001' AND x.F_EnCode=b.EnCode AND ISNULL(x.F_DeleteMark,0)=0)
GO
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_ModuleId='c0a7000c-0000-4000-8000-000000000001' AND F_EnCode=N'NF-export' AND ISNULL(F_DeleteMark,0)=0)
BEGIN
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
	VALUES (LOWER(NEWID()), 'c0a7000c-0000-4000-8000-000000000001', NULL, N'NF-export', N'导出', 1, NULL, NULL, 1, 0, GETDATE())
END
GO
-- 新按钮授权给已拥有薪酬按钮的管理角色
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM (SELECT DISTINCT ra.F_ObjectId AS RoleId FROM sys_roleauthorize ra
      WHERE ra.F_ItemId IN (SELECT mb.F_Id FROM sys_modulebutton mb JOIN sys_module m ON mb.F_ModuleId=m.F_Id WHERE m.F_ParentId='c0a70001-0000-4000-8000-000000000001')) s
CROSS JOIN (SELECT mb.F_Id FROM sys_modulebutton mb WHERE mb.F_EnCode IN (N'NF-details',N'NF-reject',N'NF-export')
            AND mb.F_ModuleId IN ('c0a7000a-0000-4000-8000-000000000001','c0a7000c-0000-4000-8000-000000000001') AND ISNULL(mb.F_DeleteMark,0)=0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemId=b.F_Id AND x.F_ObjectId=s.RoleId)
GO
