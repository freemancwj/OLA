-- =====================================================
-- 学校资产管理模块 - SQLite 建表脚本
-- =====================================================
CREATE TABLE IF NOT EXISTS ast_grade (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_GradeCode TEXT,
    F_GradeName TEXT NOT NULL,
    F_SortCode INTEGER DEFAULT 0,
    F_Description TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_class (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_GradeId TEXT,
    F_ClassName TEXT NOT NULL,
    F_HeadTeacherId TEXT,
    F_RoomId TEXT,
    F_Description TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_room (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_RoomCode TEXT,
    F_RoomName TEXT NOT NULL,
    F_RoomType TEXT,
    F_Building TEXT,
    F_ManagerId TEXT,
    F_Description TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_category (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_CategoryCode TEXT,
    F_CategoryName TEXT NOT NULL,
    F_ParentId TEXT,
    F_DepreciationYears INTEGER DEFAULT 0,
    F_IsConsumable INTEGER DEFAULT 0,
    F_SortCode INTEGER DEFAULT 0,
    F_Description TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_supplier (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_SupplierName TEXT NOT NULL,
    F_ContactPerson TEXT,
    F_Phone TEXT,
    F_Address TEXT,
    F_Description TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_asset (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetCode TEXT,
    F_AssetName TEXT NOT NULL,
    F_CategoryId TEXT,
    F_SupplierId TEXT,
    F_SpecModel TEXT,
    F_SerialNumber TEXT,
    F_PurchaseDate TEXT,
    F_PurchasePrice REAL DEFAULT 0,
    F_PurchaseSource TEXT,
    F_DepreciationYears INTEGER DEFAULT 0,
    F_Status TEXT DEFAULT '在用',
    F_IsTeachingDevice INTEGER DEFAULT 0,
    F_GradeId TEXT,
    F_ClassId TEXT,
    F_RoomId TEXT,
    F_ResponsibleUserId TEXT,
    F_Location TEXT,
    F_DamageCount INTEGER DEFAULT 0,
    F_NetValue REAL DEFAULT 0,
    F_Remark TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_class_relation (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_GradeId TEXT,
    F_ClassId TEXT,
    F_Term TEXT,
    F_BindTime TEXT,
    F_UnbindTime TEXT,
    F_Status TEXT DEFAULT '在用',
    F_Remark TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_borrow (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_BorrowerId TEXT,
    F_BorrowerName TEXT,
    F_Purpose TEXT,
    F_BorrowDate TEXT,
    F_ExpectedReturnDate TEXT,
    F_ActualReturnDate TEXT,
    F_Status TEXT DEFAULT '借出',
    F_DamageNote TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_repair (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_ReporterId TEXT,
    F_ReporterName TEXT,
    F_ReportTime TEXT,
    F_FaultDescription TEXT,
    F_Status TEXT DEFAULT '待受理',
    F_Repairer TEXT,
    F_RepairTime TEXT,
    F_RepairCost REAL DEFAULT 0,
    F_PartsReplaced TEXT,
    F_RepairResult TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_maintenance (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_MaintenanceType TEXT,
    F_PlanDate TEXT,
    F_ActualDate TEXT,
    F_Operator TEXT,
    F_Cost REAL DEFAULT 0,
    F_Result TEXT,
    F_NextDate TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_consumable (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_CategoryId TEXT,
    F_Name TEXT NOT NULL,
    F_Spec TEXT,
    F_Unit TEXT,
    F_StockQty REAL DEFAULT 0,
    F_StockWarnQty REAL DEFAULT 0,
    F_UnitPrice REAL DEFAULT 0,
    F_SupplierId TEXT,
    F_Remark TEXT,
    F_EnabledMark INTEGER DEFAULT 1,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT,
    F_DeleteTime TEXT,
    F_DeleteUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_consumable_record (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_ConsumableId TEXT,
    F_OperateType TEXT,
    F_Qty REAL DEFAULT 0,
    F_ClassId TEXT,
    F_UserId TEXT,
    F_OperatorId TEXT,
    F_OperateTime TEXT,
    F_Remark TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_inventory (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_Term TEXT,
    F_InventoryType TEXT,
    F_Status TEXT DEFAULT '草稿',
    F_CreateTime TEXT,
    F_FinishTime TEXT,
    F_Remark TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_inventory_item (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_InventoryId TEXT,
    F_AssetId TEXT,
    F_GradeId TEXT,
    F_ClassId TEXT,
    F_SystemStatus TEXT,
    F_ActualStatus TEXT,
    F_DiffResult TEXT,
    F_HandlerId TEXT,
    F_ConfirmTime TEXT,
    F_Remark TEXT
);

CREATE TABLE IF NOT EXISTS ast_transfer (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_FromClassId TEXT,
    F_ToClassId TEXT,
    F_FromRoomId TEXT,
    F_ToRoomId TEXT,
    F_FromUserId TEXT,
    F_ToUserId TEXT,
    F_TransferDate TEXT,
    F_OperatorId TEXT,
    F_Reason TEXT,
    F_Status TEXT DEFAULT '已完成',
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_scrap (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_ApplyUserId TEXT,
    F_ApplyTime TEXT,
    F_Reason TEXT,
    F_Status TEXT DEFAULT '待审批',
    F_ApproverId TEXT,
    F_ApproveTime TEXT,
    F_ApproveRemark TEXT,
    F_OriginalValue REAL DEFAULT 0,
    F_NetValue REAL DEFAULT 0,
    F_DisposalMethod TEXT,
    F_DisposalDate TEXT,
    F_DeleteMark INTEGER DEFAULT 0,
    F_CreatorTime TEXT,
    F_CreatorUserId TEXT,
    F_LastModifyTime TEXT,
    F_LastModifyUserId TEXT
);

CREATE TABLE IF NOT EXISTS ast_depreciation (
    F_Id TEXT NOT NULL PRIMARY KEY,
    F_AssetId TEXT,
    F_Period TEXT,
    F_MonthlyDepreciation REAL DEFAULT 0,
    F_AccumulatedDepreciation REAL DEFAULT 0,
    F_NetValue REAL DEFAULT 0,
    F_CreateTime TEXT
);
