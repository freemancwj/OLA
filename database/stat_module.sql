-- =============================================================
-- 大学教务扩展·批次4：统计查询增强 初始化脚本（幂等，可重复执行）
-- 内容：成绩统计/选课统计/教师工作量/补考清考统计 菜单授权（纯查询页，无新表）
-- =============================================================

-- -------------------------------------------------------------
-- 1. 菜单（教学管理 08dafb93-99da-47e5-883f-8ff1acc7a050 下）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ScoreStat' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a54001-0000-4000-8000-0000000000a1', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ScoreStat', N'成绩统计', N'fa fa-bar-chart', N'/InstructionalManagement/ScoreStat/Index', N'iframe', 1, 0, 0, 1, 1, 42, 0, 1, N'按学期/课程/班级统计平均分及格率优秀率及分数段', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ElectiveStat' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a54002-0000-4000-8000-0000000000a2', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ElectiveStat', N'选课统计', N'fa fa-pie-chart', N'/InstructionalManagement/ElectiveStat/Index', N'iframe', 1, 0, 0, 1, 1, 43, 0, 1, N'按课程班次统计选课人数与容量使用率', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'WorkloadStat' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a54003-0000-4000-8000-0000000000a3', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'WorkloadStat', N'教师工作量', N'fa fa-tasks', N'/InstructionalManagement/WorkloadStat/Index', N'iframe', 1, 0, 0, 1, 1, 44, 0, 1, N'按学期统计教师课时与监考工作量', GETDATE())
END
GO
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_EnCode = 'ExamStat' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_module]
		([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES
		(N'c0a54004-0000-4000-8000-0000000000a4', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 2, N'ExamStat', N'补考清考统计', N'fa fa-line-chart', N'/InstructionalManagement/ExamStat/Index', N'iframe', 1, 0, 0, 1, 1, 45, 0, 1, N'按学期课程统计补考清考人次与通过率', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 2. 菜单授权（R_01/R_02/R_04/R_05/R_06）
-- -------------------------------------------------------------
DECLARE @menu_ids TABLE (F_ObjectId nvarchar(50))
INSERT INTO @menu_ids (F_ObjectId) VALUES
	(N'c0a54001-0000-4000-8000-0000000000a1'),
	(N'c0a54002-0000-4000-8000-0000000000a2'),
	(N'c0a54003-0000-4000-8000-0000000000a3'),
	(N'c0a54004-0000-4000-8000-0000000000a4')

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

PRINT N'统计查询模块初始化完成'
GO
