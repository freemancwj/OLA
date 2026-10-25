-- =============================================================
-- 作业管理 + 在线考试与题库 模块初始化脚本（幂等，可重复执行）
-- 说明：
--   1. 新建 7 张业务表：作业、作业提交、题库、考试、试卷题目、考试记录、答题明细
--   2. 复用教学管理下已有的占位菜单（作业管理/考试管理/错题管理），补齐访问地址
--   3. 新增菜单：题库管理、我的作业、我的考试
--   4. 补齐按钮与角色授权
-- =============================================================

-- -------------------------------------------------------------
-- 1. 作业表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_homework]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_homework] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_Title] nvarchar(200) NOT NULL,
		[F_CourseId] nvarchar(50) NULL,
		[F_ClassId] nvarchar(50) NULL,
		[F_Content] nvarchar(max) NULL,
		[F_FileUrl] nvarchar(500) NULL,
		[F_FileName] nvarchar(200) NULL,
		[F_FileSize] bigint NULL,
		[F_Deadline] datetime2 NULL,
		[F_Status] tinyint NULL,
		[F_PublisherId] nvarchar(50) NULL,
		[F_PublisherName] nvarchar(50) NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_HOMEWORK] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_HOMEWORK_CLASS] ON [dbo].[school_homework] ([F_ClassId] ASC)
END
GO

-- -------------------------------------------------------------
-- 2. 作业提交表
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_homework_submit]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_homework_submit] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_HomeworkId] nvarchar(50) NOT NULL,
		[F_StudentId] nvarchar(50) NOT NULL,
		[F_Content] nvarchar(max) NULL,
		[F_FileUrl] nvarchar(500) NULL,
		[F_FileName] nvarchar(200) NULL,
		[F_FileSize] bigint NULL,
		[F_SubmitTime] datetime2 NULL,
		[F_Status] tinyint NULL,
		[F_Score] decimal(5,1) NULL,
		[F_Comment] nvarchar(500) NULL,
		[F_GradeTime] datetime2 NULL,
		[F_GraderId] nvarchar(50) NULL,
		[F_GraderName] nvarchar(50) NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_HOMEWORK_SUBMIT] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE UNIQUE NONCLUSTERED INDEX [UX_SCHOOL_HOMEWORK_SUBMIT] ON [dbo].[school_homework_submit] ([F_HomeworkId] ASC, [F_StudentId] ASC)
END
GO

-- -------------------------------------------------------------
-- 3. 题库表
--    F_Type: 1 单选 2 多选 3 判断 4 简答
--    判断题答案固定 A=正确 B=错误
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_question]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_question] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_CourseId] nvarchar(50) NULL,
		[F_Type] tinyint NULL,
		[F_Content] nvarchar(max) NULL,
		[F_OptionA] nvarchar(500) NULL,
		[F_OptionB] nvarchar(500) NULL,
		[F_OptionC] nvarchar(500) NULL,
		[F_OptionD] nvarchar(500) NULL,
		[F_Answer] nvarchar(max) NULL,
		[F_Analysis] nvarchar(max) NULL,
		[F_Difficulty] tinyint NULL,
		[F_Score] decimal(5,1) NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_QUESTION] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_QUESTION_COURSE] ON [dbo].[school_question] ([F_CourseId] ASC)
END
GO

-- -------------------------------------------------------------
-- 4. 考试表
--    F_Type: 1 章节测试 2 模拟考试；F_Status: 0 草稿 1 已发布 2 已结束
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_exam]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_exam] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_Title] nvarchar(200) NOT NULL,
		[F_CourseId] nvarchar(50) NULL,
		[F_ClassId] nvarchar(50) NULL,
		[F_Type] tinyint NULL,
		[F_StartTime] datetime2 NULL,
		[F_EndTime] datetime2 NULL,
		[F_Duration] int NULL,
		[F_TotalScore] decimal(6,1) NULL,
		[F_PassScore] decimal(6,1) NULL,
		[F_Status] tinyint NULL,
		[F_Remark] nvarchar(500) NULL,
		[F_PublisherId] nvarchar(50) NULL,
		[F_PublisherName] nvarchar(50) NULL,
		[F_EnabledMark] tinyint NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_EXAM] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
END
GO

-- -------------------------------------------------------------
-- 5. 试卷题目表（考试与题库的关联，含每题分值）
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_exam_question]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_exam_question] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_ExamId] nvarchar(50) NOT NULL,
		[F_QuestionId] nvarchar(50) NOT NULL,
		[F_Score] decimal(5,1) NULL,
		[F_SortCode] int NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_EXAM_QUESTION] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_EXAM_QUESTION] ON [dbo].[school_exam_question] ([F_ExamId] ASC)
END
GO

-- -------------------------------------------------------------
-- 6. 考试记录表
--    F_Status: 0 待批改 1 已完成
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_exam_record]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_exam_record] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_ExamId] nvarchar(50) NOT NULL,
		[F_StudentId] nvarchar(50) NOT NULL,
		[F_StartTime] datetime2 NULL,
		[F_SubmitTime] datetime2 NULL,
		[F_ObjectiveScore] decimal(6,1) NULL,
		[F_SubjectiveScore] decimal(6,1) NULL,
		[F_TotalScore] decimal(6,1) NULL,
		[F_Status] tinyint NULL,
		[F_IsPassed] tinyint NULL,
		[F_GraderId] nvarchar(50) NULL,
		[F_GraderName] nvarchar(50) NULL,
		[F_GradeTime] datetime2 NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_EXAM_RECORD] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE UNIQUE NONCLUSTERED INDEX [UX_SCHOOL_EXAM_RECORD] ON [dbo].[school_exam_record] ([F_ExamId] ASC, [F_StudentId] ASC)
END
GO

-- -------------------------------------------------------------
-- 7. 答题明细表
--    F_IsCorrect: 1 正确 0 错误 2 待人工批改
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[school_exam_answer]') AND type IN ('U'))
BEGIN
	CREATE TABLE [dbo].[school_exam_answer] (
		[F_Id] nvarchar(50) NOT NULL,
		[F_ExamId] nvarchar(50) NOT NULL,
		[F_RecordId] nvarchar(50) NOT NULL,
		[F_QuestionId] nvarchar(50) NOT NULL,
		[F_QuestionType] tinyint NULL,
		[F_Answer] nvarchar(max) NULL,
		[F_IsCorrect] tinyint NULL,
		[F_Score] decimal(5,1) NULL,
		[F_FullScore] decimal(5,1) NULL,
		[F_Comment] nvarchar(500) NULL,
		[F_DeleteMark] tinyint NULL,
		[F_CreatorTime] datetime2 NULL,
		[F_CreatorUserId] nvarchar(50) NULL,
		[F_LastModifyTime] datetime2 NULL,
		[F_LastModifyUserId] nvarchar(50) NULL,
		[F_DeleteTime] datetime2 NULL,
		[F_DeleteUserId] nvarchar(50) NULL,
		CONSTRAINT [PK_SCHOOL_EXAM_ANSWER] PRIMARY KEY CLUSTERED ([F_Id] ASC)
	)
	CREATE NONCLUSTERED INDEX [IX_SCHOOL_EXAM_ANSWER] ON [dbo].[school_exam_answer] ([F_RecordId] ASC)
END
GO

-- -------------------------------------------------------------
-- 8. 复用已有占位菜单，补齐地址并启用为菜单
-- -------------------------------------------------------------
UPDATE sys_module SET F_UrlAddress = N'/InstructionalManagement/Homework/Index', F_IsMenu = 1,
	F_Icon = N'fa fa-file-text-o', F_SortCode = 7, F_Description = N'教师发布作业、查看提交与批改'
WHERE F_EnCode = N'ContentManage'
GO

UPDATE sys_module SET F_UrlAddress = N'/InstructionalManagement/Exam/Index', F_IsMenu = 1,
	F_Icon = N'fa fa-edit', F_SortCode = 6, F_Description = N'创建考试、试卷组卷、成绩记录'
WHERE F_EnCode = N'Examination'
GO

UPDATE sys_module SET F_UrlAddress = N'/InstructionalManagement/Exam/Wrong', F_IsMenu = 1,
	F_FullName = N'错题记录', F_SortCode = 4, F_Description = N'学生错题记录与复习'
WHERE F_EnCode = N'ErrorQuestions'
GO

-- -------------------------------------------------------------
-- 9. 新增菜单：题库管理、我的作业、我的考试
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a30001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a30001-0000-4000-8000-000000000001', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'QuestionBank', N'题库管理', N'fa fa-question-circle', N'/InstructionalManagement/Question/Index', N'iframe', 1, 0, 0, 0, 0, 0, 10, 0, 1, N'单选/多选/判断/简答题库', GETDATE())
GO

IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a30001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a30001-0000-4000-8000-000000000002', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'MyHomework', N'我的作业', N'fa fa-book', N'/InstructionalManagement/Homework/My', N'iframe', 1, 0, 0, 0, 0, 0, 8, 0, 1, N'学生查看与提交作业', GETDATE())
GO

IF NOT EXISTS (SELECT 1 FROM sys_module WHERE F_Id = N'c0a30001-0000-4000-8000-000000000003')
	INSERT INTO [dbo].[sys_module] ([F_Id],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Icon],[F_UrlAddress],[F_Target],[F_IsMenu],[F_IsExpand],[F_IsFields],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_Description],[F_CreatorTime])
	VALUES (N'c0a30001-0000-4000-8000-000000000003', N'08dafb93-99da-47e5-883f-8ff1acc7a050', 3, N'MyExam', N'我的考试', N'fa fa-pencil-square-o', N'/InstructionalManagement/Exam/My', N'iframe', 1, 0, 0, 0, 0, 0, 9, 0, 1, N'学生在线考试与成绩查询', GETDATE())
GO

-- -------------------------------------------------------------
-- 10. 按钮定义
--     作业管理：NF-add/NF-edit/NF-delete/NF-publish/NF-review/NF-grade（教师）
--               NF-submit（学生提交）
--     考试管理：NF-add/NF-edit/NF-delete/NF-publish/NF-record/NF-grade（教师）
--               NF-take/NF-result（学生）
--     题库管理：NF-add/NF-edit/NF-delete
-- -------------------------------------------------------------
DECLARE @homeworkModule nvarchar(50) = N'bcd52760-009f-4673-80e5-ff166aa07687'
DECLARE @examModule nvarchar(50) = N'08dafb95-7df5-4638-863d-bdcfb600de04'
DECLARE @questionModule nvarchar(50) = N'c0a30001-0000-4000-8000-000000000001'

IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000001', @homeworkModule, N'0', 1, N'NF-add', N'新增作业', 1, N'add', N'/InstructionalManagement/Homework/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000002', @homeworkModule, N'0', 1, N'NF-edit', N'编辑作业', 2, N'edit', N'/InstructionalManagement/Homework/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000003')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000003', @homeworkModule, N'0', 1, N'NF-delete', N'删除作业', 2, N'delete', N'/InstructionalManagement/Homework/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000004')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000004', @homeworkModule, N'0', 1, N'NF-publish', N'发布作业', 2, N'publish', N'/InstructionalManagement/Homework/SetStatus', 0, 0, 0, 0, 4, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000005')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000005', @homeworkModule, N'0', 1, N'NF-review', N'提交与批改', 2, N'review', N'/InstructionalManagement/Homework/Review', 0, 0, 0, 0, 5, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000006')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000006', @homeworkModule, N'0', 1, N'NF-grade', N'作业批改', 2, N'grade', N'/InstructionalManagement/Homework/Grade', 0, 0, 0, 0, 6, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a31001-0000-4000-8000-000000000007')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a31001-0000-4000-8000-000000000007', @homeworkModule, N'0', 1, N'NF-submit', N'提交作业', 2, N'submit', N'/InstructionalManagement/Homework/Submit', 0, 0, 0, 0, 7, 0, 1, GETDATE())

IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000001', @examModule, N'0', 1, N'NF-add', N'新建考试', 1, N'add', N'/InstructionalManagement/Exam/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000002', @examModule, N'0', 1, N'NF-edit', N'编辑考试', 2, N'edit', N'/InstructionalManagement/Exam/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000003')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000003', @examModule, N'0', 1, N'NF-delete', N'删除考试', 2, N'delete', N'/InstructionalManagement/Exam/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000004')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000004', @examModule, N'0', 1, N'NF-publish', N'发布考试', 2, N'publish', N'/InstructionalManagement/Exam/SetStatus', 0, 0, 0, 0, 4, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000005')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000005', @examModule, N'0', 1, N'NF-record', N'成绩记录', 2, N'record', N'/InstructionalManagement/Exam/Record', 0, 0, 0, 0, 5, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000006')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000006', @examModule, N'0', 1, N'NF-grade', N'试卷批改', 2, N'grade', N'/InstructionalManagement/Exam/Grade', 0, 0, 0, 0, 6, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000007')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000007', @examModule, N'0', 1, N'NF-take', N'参加考试', 2, N'take', N'/InstructionalManagement/Exam/Take', 0, 0, 0, 0, 7, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a32001-0000-4000-8000-000000000008')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a32001-0000-4000-8000-000000000008', @examModule, N'0', 1, N'NF-result', N'成绩详情', 2, N'result', N'/InstructionalManagement/Exam/Result', 0, 0, 0, 0, 8, 0, 1, GETDATE())

IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a33001-0000-4000-8000-000000000001')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a33001-0000-4000-8000-000000000001', @questionModule, N'0', 1, N'NF-add', N'新增题目', 1, N'add', N'/InstructionalManagement/Question/Form', 0, 0, 0, 0, 1, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a33001-0000-4000-8000-000000000002')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a33001-0000-4000-8000-000000000002', @questionModule, N'0', 1, N'NF-edit', N'编辑题目', 2, N'edit', N'/InstructionalManagement/Question/Form', 0, 0, 0, 0, 2, 0, 1, GETDATE())
IF NOT EXISTS (SELECT 1 FROM sys_modulebutton WHERE F_Id = N'c0a33001-0000-4000-8000-000000000003')
	INSERT INTO [dbo].[sys_modulebutton] ([F_Id],[F_ModuleId],[F_ParentId],[F_Layers],[F_EnCode],[F_FullName],[F_Location],[F_JsEvent],[F_UrlAddress],[F_Split],[F_IsPublic],[F_AllowEdit],[F_AllowDelete],[F_SortCode],[F_DeleteMark],[F_EnabledMark],[F_CreatorTime])
	VALUES (N'c0a33001-0000-4000-8000-000000000003', @questionModule, N'0', 1, N'NF-delete', N'删除题目', 2, N'delete', N'/InstructionalManagement/Question/DeleteForm', 0, 0, 0, 0, 3, 0, 1, GETDATE())
GO

-- -------------------------------------------------------------
-- 11. 角色授权
--     教师类：R_01 系统管理员、R_02 普通管理员、R_04 老师、R_05 校长、R_06 班主任
--     学生：R_03
-- -------------------------------------------------------------
-- 11.1 学生不再直接使用"作业管理""考试管理"（改用"我的作业""我的考试"）
DELETE FROM sys_roleauthorize
WHERE F_ItemType = 1 AND F_ObjectType = 1 AND F_ObjectId = N'08da9c6b-f455-46c0-8ffe-81edad14c7be'
  AND F_ItemId IN (N'bcd52760-009f-4673-80e5-ff166aa07687', N'08dafb95-7df5-4638-863d-bdcfb600de04')
GO

-- 11.2 教师类角色：菜单授权
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_module m
CROSS JOIN sys_role r
WHERE m.F_Id IN (N'bcd52760-009f-4673-80e5-ff166aa07687', N'08dafb95-7df5-4638-863d-bdcfb600de04', N'c0a30001-0000-4000-8000-000000000001')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

-- 11.3 教师类角色：按钮授权（作业/考试/题库的教师按钮）
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, r.F_Id, 0, GETDATE()
FROM sys_modulebutton b
CROSS JOIN sys_role r
WHERE b.F_ModuleId IN (N'bcd52760-009f-4673-80e5-ff166aa07687', N'08dafb95-7df5-4638-863d-bdcfb600de04', N'c0a30001-0000-4000-8000-000000000001')
  AND b.F_EnCode NOT IN (N'NF-submit', N'NF-take', N'NF-result')
  AND r.F_EnCode IN (N'R_01', N'R_02', N'R_04', N'R_05', N'R_06')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = r.F_Id)
GO

-- 11.4 学生角色：我的作业、我的考试、错题记录 菜单授权
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 1, m.F_Id, 1, N'08da9c6b-f455-46c0-8ffe-81edad14c7be', 0, GETDATE()
FROM sys_module m
WHERE m.F_Id IN (N'c0a30001-0000-4000-8000-000000000002', N'c0a30001-0000-4000-8000-000000000003', N'08dafb96-5360-48a8-89c9-58b6d70448da')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 1 AND x.F_ItemId = m.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = N'08da9c6b-f455-46c0-8ffe-81edad14c7be')
GO

-- 11.5 学生角色：提交作业 / 参加考试 / 成绩详情 按钮授权
INSERT INTO [dbo].[sys_roleauthorize] ([F_Id],[F_ItemType],[F_ItemId],[F_ObjectType],[F_ObjectId],[F_SortCode],[F_CreatorTime])
SELECT LOWER(NEWID()), 2, b.F_Id, 1, N'08da9c6b-f455-46c0-8ffe-81edad14c7be', 0, GETDATE()
FROM sys_modulebutton b
WHERE b.F_EnCode IN (N'NF-submit', N'NF-take', N'NF-result')
  AND b.F_ModuleId IN (N'bcd52760-009f-4673-80e5-ff166aa07687', N'08dafb95-7df5-4638-863d-bdcfb600de04')
  AND NOT EXISTS (SELECT 1 FROM sys_roleauthorize x WHERE x.F_ItemType = 2 AND x.F_ItemId = b.F_Id AND x.F_ObjectType = 1 AND x.F_ObjectId = N'08da9c6b-f455-46c0-8ffe-81edad14c7be')
GO

PRINT N'作业管理与在线考试模块初始化完成'
GO
