-- =============================================================
-- 作业管理重构 初始化脚本（幂等，可重复执行）
-- 内容：school_homework 增加满分字段；修正作业管理菜单结构；
--      补齐按钮（发布/取消发布/提交批改/批改/提交）；授权教师与学生角色
-- =============================================================

-- -------------------------------------------------------------
-- 1. 表结构补充：作业满分
-- -------------------------------------------------------------
IF COL_LENGTH('school_homework', 'F_FullScore') IS NULL
	ALTER TABLE [dbo].[school_homework] ADD [F_FullScore] [decimal](5,1) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_homework_submit_hw' AND object_id = OBJECT_ID('school_homework_submit'))
	CREATE NONCLUSTERED INDEX [IX_school_homework_submit_hw] ON [dbo].[school_homework_submit] ([F_HomeworkId] ASC, [F_StudentId] ASC)
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_school_homework_class' AND object_id = OBJECT_ID('school_homework'))
	CREATE NONCLUSTERED INDEX [IX_school_homework_class] ON [dbo].[school_homework] ([F_ClassId] ASC, [F_Status] ASC)
GO

-- -------------------------------------------------------------
-- 2. 修正作业管理菜单（原编码沿用 ContentManage，改为 HomeworkManage）
-- -------------------------------------------------------------
UPDATE sys_module
SET F_EnCode = N'HomeworkManage',
    F_FullName = N'作业管理',
    F_UrlAddress = N'/InstructionalManagement/Homework/Index',
    F_IsMenu = 1,
    F_EnabledMark = 1
WHERE F_Id = 'bcd52760-009f-4673-80e5-ff166aa07687'
GO

-- 移除原"瞎做"的子菜单（作业类别/作业发布，指向内容管理控制器）
UPDATE sys_module
SET F_DeleteMark = 1, F_DeleteTime = GETDATE()
WHERE F_Id IN ('4efd6f84-a4a9-4176-aedd-153e7748cbac', 'f3277ddd-1bf1-4202-8a4b-15c29a405bd5')
  AND ISNULL(F_DeleteMark,0) = 0
GO
UPDATE sys_modulebutton
SET F_DeleteMark = 1
WHERE F_ModuleId IN ('4efd6f84-a4a9-4176-aedd-153e7748cbac', 'f3277ddd-1bf1-4202-8a4b-15c29a405bd5')
GO

-- 我的作业（学生端）菜单
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = 'c0a30001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_EnCode],[F_FullName],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsPublic],[F_EnabledMark],[F_DeleteMark],[F_SortCode],[F_CreatorTime])
	VALUES ('c0a30001-0000-4000-8000-000000000002', '08dafb93-99da-47e5-883f-8ff1acc7a050', N'MyHomework', N'我的作业', N'/InstructionalManagement/Homework/My', N'', 1, 0, 1, 0, 8, GETDATE())
GO
UPDATE sys_module
SET F_EnCode = N'MyHomework', F_FullName = N'我的作业',
    F_UrlAddress = N'/InstructionalManagement/Homework/My', F_EnabledMark = 1, F_DeleteMark = 0
WHERE F_Id = 'c0a30001-0000-4000-8000-000000000002'
GO

-- -------------------------------------------------------------
-- 3. 补齐按钮（按模块+F_EnCode 判重）
-- -------------------------------------------------------------
DECLARE @btns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, Url nvarchar(200) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns (EnCode, FullName, Url, SortCode) VALUES
 (N'NF-add', N'发布作业', N'/InstructionalManagement/Homework/Form', 1),
 (N'NF-edit', N'编辑作业', N'/InstructionalManagement/Homework/Form', 2),
 (N'NF-publish', N'发布作业', N'/InstructionalManagement/Homework/SetStatus', 3),
 (N'NF-unpublish', N'取消发布', N'/InstructionalManagement/Homework/SetStatus', 4),
 (N'NF-review', N'提交与批改', N'/InstructionalManagement/Homework/Review', 5),
 (N'NF-grade', N'作业批改', N'/InstructionalManagement/Homework/Grade', 6),
 (N'NF-submit', N'提交作业', N'/InstructionalManagement/Homework/Submit', 7),
 (N'NF-delete', N'删除作业', N'/InstructionalManagement/Homework/DeleteForm', 8)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), 'bcd52760-009f-4673-80e5-ff166aa07687', NULL, b.EnCode, b.FullName, NULL, b.Url, b.SortCode, 1, 0, GETDATE()
FROM @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = 'bcd52760-009f-4673-80e5-ff166aa07687' AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO

-- 恢复被误删的按钮（如历史数据已标记删除）
UPDATE sys_modulebutton SET F_DeleteMark = 0
WHERE F_ModuleId = 'bcd52760-009f-4673-80e5-ff166aa07687'
GO

-- -------------------------------------------------------------
-- 4. 授权
-- -------------------------------------------------------------
-- 4.1 教师/管理类角色：作业管理菜单
DECLARE @staff TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @staff (RoleId)
SELECT F_Id FROM sys_role WHERE F_DeleteMark = 0 AND F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06', N'R_07', N'G_01', N'G_02', N'G_03', N'G_04', N'G_05', N'G_06', N'G_07', N'G_08', N'G_09')

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @staff s
CROSS JOIN (SELECT F_Id FROM sys_module WHERE F_Id = 'bcd52760-009f-4673-80e5-ff166aa07687') m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.F_Id AND ra.F_ObjectId = s.RoleId)

-- 4.2 教师/管理类角色：作业管理按钮
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @staff s
CROSS JOIN (SELECT F_Id FROM sys_modulebutton WHERE F_ModuleId = 'bcd52760-009f-4673-80e5-ff166aa07687' AND ISNULL(F_DeleteMark,0) = 0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)

-- 4.3 学生角色 + 管理员：我的作业菜单
DECLARE @stu TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @stu (RoleId)
SELECT F_Id FROM sys_role WHERE F_DeleteMark = 0 AND F_EnCode IN (N'R_03', N's_01', N'R_01', N'R_02')

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, 'c0a30001-0000-4000-8000-000000000002', 1, s.RoleId, 0, GETDATE()
FROM @stu s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = 'c0a30001-0000-4000-8000-000000000002' AND ra.F_ObjectId = s.RoleId)
GO

-- -------------------------------------------------------------
-- 5. 数据修正：历史 NULL 删除标记
-- -------------------------------------------------------------
UPDATE school_homework SET F_DeleteMark = 0 WHERE F_DeleteMark IS NULL
GO
UPDATE school_homework_submit SET F_DeleteMark = 0 WHERE F_DeleteMark IS NULL
GO
UPDATE school_homework SET F_FullScore = 100 WHERE F_FullScore IS NULL
GO
