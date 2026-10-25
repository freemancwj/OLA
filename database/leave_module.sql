-- =============================================================
-- 请假管理模块 初始化脚本（幂等，可重复执行）
-- 内容：sys_leave 建表；请假申请单（表单设计器）；教师/学生请假流程模板；
--      请假管理菜单按钮与授权；审批菜单授权；班主任角色兜底补齐
-- 审批规则：学生请假由班主任审批；教师请假一天以内班主任审批，超过一天校长审批
-- =============================================================

-- -------------------------------------------------------------
-- 1. 建表：请假申请单
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sys_leave')
BEGIN
	CREATE TABLE [dbo].[sys_leave] (
		[F_Id] nvarchar(50) COLLATE Chinese_PRC_CI_AS NOT NULL,
		[F_FlowInstanceId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_ApplicantType] int NOT NULL DEFAULT 1,
		[F_EmployeeId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LeaveType] int NOT NULL DEFAULT 1,
		[F_StartTime] datetime NOT NULL,
		[F_EndTime] datetime NOT NULL,
		[F_LeaveDays] float NOT NULL DEFAULT 0,
		[F_Reason] nvarchar(500) COLLATE Chinese_PRC_CI_AS NULL,
		[F_Status] int NOT NULL DEFAULT 0,
		[F_CurrentApprover] nvarchar(200) COLLATE Chinese_PRC_CI_AS NULL,
		[F_CreatorTime] datetime NULL,
		[F_CreatorUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		[F_LastModifyTime] datetime NULL,
		[F_LastModifyUserId] nvarchar(50) COLLATE Chinese_PRC_CI_AS NULL,
		CONSTRAINT [PK_sys_leave] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 2. 表单：请假申请单（表单设计器，FrmType=0）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_form WHERE F_Id = 'c0a40001-0000-4000-8000-000000000001')
BEGIN
	INSERT INTO [dbo].[sys_form] ([F_Id],[F_Name],[F_FrmType],[F_WebId],[F_Fields],[F_ContentData],[F_ContentParse],[F_Content],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime],[F_Description],[F_OrganizeId],[F_DbName])
	VALUES ('c0a40001-0000-4000-8000-000000000001', N'请假申请单', 0, NULL, 8,
		N'申请人,所属部门,申请类型,请假类型,开始时间,结束时间,请假天数,请假原因', NULL,
		N'[{"id":"grid_1","index":0,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"申请人","index":0,"label":"申请人","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"所属部门","index":1,"label":"所属部门","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]}]},{"id":"grid_2","index":1,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"申请类型","index":0,"label":"申请类型","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"请假类型","index":1,"label":"请假类型","tag":"select","tagIcon":"select","labelWidth":null,"width":"100%","disabled":false,"required":true,"options":[{"value":"1","text":"年假","checked":false},{"value":"2","text":"病假","checked":false},{"value":"3","text":"事假","checked":false},{"value":"4","text":"婚假","checked":false},{"value":"5","text":"产假","checked":false}],"document":""}]}]},{"id":"grid_3","index":2,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"开始时间","index":0,"label":"开始时间","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"结束时间","index":1,"label":"结束时间","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]}]},{"id":"grid_4","index":3,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"请假天数","index":0,"label":"请假天数","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[]}]},{"id":"请假原因","index":4,"label":"请假原因","tag":"textarea","tagIcon":"textarea","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","maxlength":500,"showWordLimit":false,"readonly":false,"disabled":false,"required":true,"document":""}]',
		1, 1, 0, GETDATE(), N'请假管理内置表单', NULL, NULL)
END
GO
-- 兜底：若表单已存在但内容不完整，刷新为最新表单定义
UPDATE [dbo].[sys_form]
SET [F_ContentData] = N'申请人,所属部门,申请类型,请假类型,开始时间,结束时间,请假天数,请假原因',
	[F_Fields] = 8,
	[F_Content] = N'[{"id":"grid_1","index":0,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"申请人","index":0,"label":"申请人","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"所属部门","index":1,"label":"所属部门","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]}]},{"id":"grid_2","index":1,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"申请类型","index":0,"label":"申请类型","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"请假类型","index":1,"label":"请假类型","tag":"select","tagIcon":"select","labelWidth":null,"width":"100%","disabled":false,"required":true,"options":[{"value":"1","text":"年假","checked":false},{"value":"2","text":"病假","checked":false},{"value":"3","text":"事假","checked":false},{"value":"4","text":"婚假","checked":false},{"value":"5","text":"产假","checked":false}],"document":""}]}]},{"id":"grid_3","index":2,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"开始时间","index":0,"label":"开始时间","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[{"id":"结束时间","index":1,"label":"结束时间","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]}]},{"id":"grid_4","index":3,"tag":"grid","span":2,"columns":[{"span":12,"list":[{"id":"请假天数","index":0,"label":"请假天数","tag":"input","tagIcon":"input","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","clearable":false,"maxlength":null,"showWordLimit":false,"readonly":true,"disabled":false,"required":true,"expression":"","document":""}]},{"span":12,"list":[]}]},{"id":"请假原因","index":4,"label":"请假原因","tag":"textarea","tagIcon":"textarea","placeholder":"","defaultValue":null,"labelWidth":null,"width":"100%","maxlength":500,"showWordLimit":false,"readonly":false,"disabled":false,"required":true,"document":""}]'
WHERE F_Id = 'c0a40001-0000-4000-8000-000000000001'
GO

-- -------------------------------------------------------------
-- 3. 流程模板：教师请假流程（一天以内班主任审批，超过一天校长审批）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_flowscheme WHERE F_SchemeCode = 'TeacherLeave' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_flowscheme] ([F_Id],[F_SchemeCode],[F_SchemeName],[F_SchemeType],[F_SchemeVersion],[F_SchemeCanUser],[F_SchemeContent],[F_FrmId],[F_FrmType],[F_AuthorizeType],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a40002-0000-4000-8000-000000000001', N'TeacherLeave', N'教师请假流程', N'请假管理', '1.0', NULL,
		N'{"title":"教师请假流程","initNum":4,"nodes":[{"name":"开始","left":360,"top":20,"type":"start round mix","id":"leave_start","width":120,"height":30,"alt":true},{"name":"班主任审批","left":180,"top":130,"type":"node","id":"leave_head","width":120,"height":40,"alt":true,"setInfo":{"NodeName":"班主任审批","NodeCode":"leave_head","NodeRejectType":"0","NodeDesignate":"SPECIAL_ROLE","NodeConfluenceType":"all","ThirdPartyUrl":"","NodeDesignateData":{"users":[],"roles":["08dd953d-47b0-4c64-8efd-d84852af5dc8"],"orgs":[],"currentDepart":false}}},{"name":"校长审批","left":540,"top":130,"type":"node","id":"leave_boss","width":120,"height":40,"alt":true,"setInfo":{"NodeName":"校长审批","NodeCode":"leave_boss","NodeRejectType":"0","NodeDesignate":"SPECIAL_ROLE","NodeConfluenceType":"all","ThirdPartyUrl":"","NodeDesignateData":{"users":[],"roles":["08dd92e2-0437-48c4-8189-11ae540da61f"],"orgs":[],"currentDepart":false}}},{"name":"结束","left":360,"top":250,"type":"end round","id":"leave_end","width":120,"height":30,"alt":true}],"lines":[{"type":"sl","from":"leave_start","to":"leave_head","id":"leave_line_sh","name":"一天以内","dash":false,"Compares":[{"FieldName":"请假天数","FieldType":"form","Operation":"<=","Value":"1","Name":"1","Condition":"and"}]},{"type":"sl","from":"leave_start","to":"leave_boss","id":"leave_line_sb","name":"超过一天","dash":false,"Compares":[{"FieldName":"请假天数","FieldType":"form","Operation":">","Value":"1","Name":"1","Condition":"and"}]},{"type":"sl","from":"leave_head","to":"leave_end","id":"leave_line_he","name":"","dash":false},{"type":"sl","from":"leave_boss","to":"leave_end","id":"leave_line_be","name":"","dash":false}],"areas":[]}',
		'c0a40001-0000-4000-8000-000000000001', 0, 0, 1, 0, 1, N'教师请假：一天以内班主任审批，超过一天校长审批', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 4. 流程模板：学生请假流程（班主任审批）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_flowscheme WHERE F_SchemeCode = 'StudentLeave' AND ISNULL(F_DeleteMark,0) = 0)
BEGIN
	INSERT INTO [dbo].[sys_flowscheme] ([F_Id],[F_SchemeCode],[F_SchemeName],[F_SchemeType],[F_SchemeVersion],[F_SchemeCanUser],[F_SchemeContent],[F_FrmId],[F_FrmType],[F_AuthorizeType],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES ('c0a40003-0000-4000-8000-000000000001', N'StudentLeave', N'学生请假流程', N'请假管理', '1.0', NULL,
		N'{"title":"学生请假流程","initNum":3,"nodes":[{"name":"开始","left":360,"top":20,"type":"start round mix","id":"leave_start","width":120,"height":30,"alt":true},{"name":"班主任审批","left":360,"top":130,"type":"node","id":"leave_head","width":120,"height":40,"alt":true,"setInfo":{"NodeName":"班主任审批","NodeCode":"leave_head","NodeRejectType":"0","NodeDesignate":"SPECIAL_ROLE","NodeConfluenceType":"all","ThirdPartyUrl":"","NodeDesignateData":{"users":[],"roles":["08dd953d-47b0-4c64-8efd-d84852af5dc8"],"orgs":[],"currentDepart":false}}},{"name":"结束","left":360,"top":250,"type":"end round","id":"leave_end","width":120,"height":30,"alt":true}],"lines":[{"type":"sl","from":"leave_start","to":"leave_head","id":"leave_line_sh","name":"","dash":false},{"type":"sl","from":"leave_head","to":"leave_end","id":"leave_line_he","name":"","dash":false}],"areas":[]}',
		'c0a40001-0000-4000-8000-000000000001', 0, 0, 2, 0, 1, N'学生请假：班主任审批', GETDATE())
END
GO

-- -------------------------------------------------------------
-- 5. 请假管理菜单按钮（申请/修改/撤回/进度/删除）
-- -------------------------------------------------------------
DECLARE @btns TABLE (EnCode nvarchar(50) COLLATE Chinese_PRC_CI_AS, FullName nvarchar(50) COLLATE Chinese_PRC_CI_AS, SortCode int)
INSERT INTO @btns (EnCode, FullName, SortCode) VALUES
 (N'NF-add', N'申请请假', 1),
 (N'NF-edit', N'修改申请', 2),
 (N'NF-cancle', N'撤回申请', 3),
 (N'NF-details', N'审批进度', 4),
 (N'NF-delete', N'删除申请', 5)

INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_SortCode],[F_EnabledMark],[F_DeleteMark],[F_CreatorTime])
SELECT LOWER(NEWID()), '08ddd5b3-451b-426b-889b-ef2f40abb3e3', NULL, b.EnCode, b.FullName, NULL, NULL, b.SortCode, 1, 0, GETDATE()
FROM @btns b
WHERE NOT EXISTS (SELECT 1 FROM sys_modulebutton x WHERE x.F_ModuleId = '08ddd5b3-451b-426b-889b-ef2f40abb3e3' AND x.F_EnCode = b.EnCode AND ISNULL(x.F_DeleteMark,0) = 0)
GO
UPDATE sys_modulebutton SET F_DeleteMark = 0
WHERE F_ModuleId = '08ddd5b3-451b-426b-889b-ef2f40abb3e3'
GO

-- -------------------------------------------------------------
-- 6. 授权
-- -------------------------------------------------------------
-- 6.1 请假管理菜单+按钮：所有有效角色（教师、班主任、校长、教务主任、学生、管理员均可申请）
DECLARE @allRoles TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @allRoles (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, '08ddd5b3-451b-426b-889b-ef2f40abb3e3', 1, s.RoleId, 0, GETDATE()
FROM @allRoles s
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = '08ddd5b3-451b-426b-889b-ef2f40abb3e3' AND ra.F_ObjectId = s.RoleId)

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, s.RoleId, 0, GETDATE()
FROM @allRoles s
CROSS JOIN (SELECT F_Id FROM sys_modulebutton WHERE F_ModuleId = '08ddd5b3-451b-426b-889b-ef2f40abb3e3' AND ISNULL(F_DeleteMark,0) = 0) b
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = b.F_Id AND ra.F_ObjectId = s.RoleId)
GO

-- 6.2 审批相关菜单（待处理/已处理/我的流程）：教师类角色（R_04教师、R_06班主任、R_07教务主任）
DECLARE @approver TABLE (RoleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @approver (RoleId)
SELECT F_Id FROM sys_role WHERE ISNULL(F_DeleteMark,0) = 0 AND F_EnCode IN (N'R_04', N'R_06', N'R_07', N'G_02', N'G_03')

DECLARE @flowModules TABLE (ModuleId nvarchar(50) COLLATE Chinese_PRC_CI_AS)
INSERT INTO @flowModules (ModuleId) VALUES
 ('605444e5-704f-4cca-8d00-75175e2aef05'), -- 待处理流程
 ('ee136db7-178a-4bb0-b878-51287a5e2e2b'), -- 已处理流程
 ('c87cd44f-d064-4d3c-a43e-de01a7a8785e')  -- 我的流程

INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.ModuleId, 1, s.RoleId, 0, GETDATE()
FROM @approver s
CROSS JOIN @flowModules m
WHERE NOT EXISTS (SELECT 1 FROM sys_roleauthorize ra WHERE ra.F_ItemId = m.ModuleId AND ra.F_ObjectId = s.RoleId)
GO

-- -------------------------------------------------------------
-- 7. 班主任角色兜底：系统尚无班主任时，将班主任角色赋予肖老师
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_user WHERE ISNULL(F_DeleteMark,0) = 0 AND ISNULL(F_RoleId,'') LIKE '%08dd953d-47b0-4c64-8efd-d84852af5dc8%')
BEGIN
	UPDATE sys_user
	SET F_RoleId = ISNULL(F_RoleId,'') + ',08dd953d-47b0-4c64-8efd-d84852af5dc8'
	WHERE F_Account = 'xiaoyan' AND ISNULL(F_DeleteMark,0) = 0
END
GO
