-- =====================================================
-- 学校资产管理模块 - SQL Server 建表脚本
-- 适配学校学期制：年级/班级/功能室/资产/借用/报修/维保/耗材/盘点/调拨/报废/折旧
-- =====================================================
IF OBJECT_ID('ast_grade', 'U') IS NULL
CREATE TABLE ast_grade (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_GradeCode nvarchar(50) NULL,
    F_GradeName nvarchar(100) NOT NULL,
    F_SortCode int NULL DEFAULT 0,
    F_Description nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_class', 'U') IS NULL
CREATE TABLE ast_class (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_GradeId nvarchar(50) NULL,
    F_ClassName nvarchar(100) NOT NULL,
    F_HeadTeacherId nvarchar(50) NULL,
    F_RoomId nvarchar(50) NULL,
    F_Description nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_room', 'U') IS NULL
CREATE TABLE ast_room (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_RoomCode nvarchar(50) NULL,
    F_RoomName nvarchar(100) NOT NULL,
    F_RoomType nvarchar(50) NULL,
    F_Building nvarchar(100) NULL,
    F_ManagerId nvarchar(50) NULL,
    F_Description nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_category', 'U') IS NULL
CREATE TABLE ast_category (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_CategoryCode nvarchar(50) NULL,
    F_CategoryName nvarchar(100) NOT NULL,
    F_ParentId nvarchar(50) NULL,
    F_DepreciationYears int NULL DEFAULT 0,
    F_IsConsumable bit NULL DEFAULT 0,
    F_SortCode int NULL DEFAULT 0,
    F_Description nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_supplier', 'U') IS NULL
CREATE TABLE ast_supplier (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_SupplierName nvarchar(200) NOT NULL,
    F_ContactPerson nvarchar(100) NULL,
    F_Phone nvarchar(50) NULL,
    F_Address nvarchar(500) NULL,
    F_Description nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_asset', 'U') IS NULL
CREATE TABLE ast_asset (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetCode nvarchar(100) NULL,
    F_AssetName nvarchar(200) NOT NULL,
    F_CategoryId nvarchar(50) NULL,
    F_SupplierId nvarchar(50) NULL,
    F_SpecModel nvarchar(200) NULL,
    F_SerialNumber nvarchar(200) NULL,
    F_PurchaseDate datetime NULL,
    F_PurchasePrice decimal(18,2) NULL DEFAULT 0,
    F_PurchaseSource nvarchar(50) NULL,
    F_DepreciationYears int NULL DEFAULT 0,
    F_Status nvarchar(50) NULL DEFAULT N'在用',
    F_IsTeachingDevice bit NULL DEFAULT 0,
    F_GradeId nvarchar(50) NULL,
    F_ClassId nvarchar(50) NULL,
    F_RoomId nvarchar(50) NULL,
    F_ResponsibleUserId nvarchar(50) NULL,
    F_Location nvarchar(200) NULL,
    F_DamageCount int NULL DEFAULT 0,
    F_NetValue decimal(18,2) NULL DEFAULT 0,
    F_Remark nvarchar(1000) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_class_relation', 'U') IS NULL
CREATE TABLE ast_class_relation (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_GradeId nvarchar(50) NULL,
    F_ClassId nvarchar(50) NULL,
    F_Term nvarchar(50) NULL,
    F_BindTime datetime NULL,
    F_UnbindTime datetime NULL,
    F_Status nvarchar(50) NULL DEFAULT N'在用',
    F_Remark nvarchar(500) NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_borrow', 'U') IS NULL
CREATE TABLE ast_borrow (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_BorrowerId nvarchar(50) NULL,
    F_BorrowerName nvarchar(100) NULL,
    F_Purpose nvarchar(500) NULL,
    F_BorrowDate datetime NULL,
    F_ExpectedReturnDate datetime NULL,
    F_ActualReturnDate datetime NULL,
    F_Status nvarchar(50) NULL DEFAULT N'借出',
    F_DamageNote nvarchar(500) NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_repair', 'U') IS NULL
CREATE TABLE ast_repair (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_ReporterId nvarchar(50) NULL,
    F_ReporterName nvarchar(100) NULL,
    F_ReportTime datetime NULL,
    F_FaultDescription nvarchar(1000) NULL,
    F_Status nvarchar(50) NULL DEFAULT N'待受理',
    F_Repairer nvarchar(100) NULL,
    F_RepairTime datetime NULL,
    F_RepairCost decimal(18,2) NULL DEFAULT 0,
    F_PartsReplaced nvarchar(500) NULL,
    F_RepairResult nvarchar(1000) NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_maintenance', 'U') IS NULL
CREATE TABLE ast_maintenance (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_MaintenanceType nvarchar(50) NULL,
    F_PlanDate datetime NULL,
    F_ActualDate datetime NULL,
    F_Operator nvarchar(100) NULL,
    F_Cost decimal(18,2) NULL DEFAULT 0,
    F_Result nvarchar(1000) NULL,
    F_NextDate datetime NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_consumable', 'U') IS NULL
CREATE TABLE ast_consumable (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_CategoryId nvarchar(50) NULL,
    F_Name nvarchar(200) NOT NULL,
    F_Spec nvarchar(200) NULL,
    F_Unit nvarchar(50) NULL,
    F_StockQty decimal(18,2) NULL DEFAULT 0,
    F_StockWarnQty decimal(18,2) NULL DEFAULT 0,
    F_UnitPrice decimal(18,2) NULL DEFAULT 0,
    F_SupplierId nvarchar(50) NULL,
    F_Remark nvarchar(500) NULL,
    F_EnabledMark bit NULL DEFAULT 1,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL,
    F_DeleteTime datetime NULL,
    F_DeleteUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_consumable_record', 'U') IS NULL
CREATE TABLE ast_consumable_record (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_ConsumableId nvarchar(50) NULL,
    F_OperateType nvarchar(50) NULL,
    F_Qty decimal(18,2) NULL DEFAULT 0,
    F_ClassId nvarchar(50) NULL,
    F_UserId nvarchar(50) NULL,
    F_OperatorId nvarchar(50) NULL,
    F_OperateTime datetime NULL,
    F_Remark nvarchar(500) NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_inventory', 'U') IS NULL
CREATE TABLE ast_inventory (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_Term nvarchar(50) NULL,
    F_InventoryType nvarchar(50) NULL,
    F_Status nvarchar(50) NULL DEFAULT N'草稿',
    F_CreateTime datetime NULL,
    F_FinishTime datetime NULL,
    F_Remark nvarchar(500) NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_inventory_item', 'U') IS NULL
CREATE TABLE ast_inventory_item (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_InventoryId nvarchar(50) NULL,
    F_AssetId nvarchar(50) NULL,
    F_GradeId nvarchar(50) NULL,
    F_ClassId nvarchar(50) NULL,
    F_SystemStatus nvarchar(50) NULL,
    F_ActualStatus nvarchar(50) NULL,
    F_DiffResult nvarchar(50) NULL,
    F_HandlerId nvarchar(50) NULL,
    F_ConfirmTime datetime NULL,
    F_Remark nvarchar(500) NULL
);

IF OBJECT_ID('ast_transfer', 'U') IS NULL
CREATE TABLE ast_transfer (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_FromClassId nvarchar(50) NULL,
    F_ToClassId nvarchar(50) NULL,
    F_FromRoomId nvarchar(50) NULL,
    F_ToRoomId nvarchar(50) NULL,
    F_FromUserId nvarchar(50) NULL,
    F_ToUserId nvarchar(50) NULL,
    F_TransferDate datetime NULL,
    F_OperatorId nvarchar(50) NULL,
    F_Reason nvarchar(500) NULL,
    F_Status nvarchar(50) NULL DEFAULT N'已完成',
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_scrap', 'U') IS NULL
CREATE TABLE ast_scrap (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_ApplyUserId nvarchar(50) NULL,
    F_ApplyTime datetime NULL,
    F_Reason nvarchar(1000) NULL,
    F_Status nvarchar(50) NULL DEFAULT N'待审批',
    F_ApproverId nvarchar(50) NULL,
    F_ApproveTime datetime NULL,
    F_ApproveRemark nvarchar(500) NULL,
    F_OriginalValue decimal(18,2) NULL DEFAULT 0,
    F_NetValue decimal(18,2) NULL DEFAULT 0,
    F_DisposalMethod nvarchar(100) NULL,
    F_DisposalDate datetime NULL,
    F_DeleteMark bit NULL DEFAULT 0,
    F_CreatorTime datetime NULL,
    F_CreatorUserId nvarchar(50) NULL,
    F_LastModifyTime datetime NULL,
    F_LastModifyUserId nvarchar(50) NULL
);

IF OBJECT_ID('ast_depreciation', 'U') IS NULL
CREATE TABLE ast_depreciation (
    F_Id nvarchar(50) NOT NULL PRIMARY KEY,
    F_AssetId nvarchar(50) NULL,
    F_Period nvarchar(20) NULL,
    F_MonthlyDepreciation decimal(18,2) NULL DEFAULT 0,
    F_AccumulatedDepreciation decimal(18,2) NULL DEFAULT 0,
    F_NetValue decimal(18,2) NULL DEFAULT 0,
    F_CreateTime datetime NULL
);
