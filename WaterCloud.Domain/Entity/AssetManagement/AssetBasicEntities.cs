using System;
using SqlSugar;

namespace WaterCloud.Domain.AssetManagement
{
    /// <summary>年级</summary>
    [SugarTable("ast_grade")]
    public class AstGradeEntity : IEntity<AstGradeEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_GradeCode { get; set; }
        public string F_GradeName { get; set; }
        public int? F_SortCode { get; set; }
        public string F_Description { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>班级</summary>
    [SugarTable("ast_class")]
    public class AstClassEntity : IEntity<AstClassEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_GradeId { get; set; }
        public string F_ClassName { get; set; }
        public string F_HeadTeacherId { get; set; }
        public string F_RoomId { get; set; }
        public string F_Description { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>功能室/教室</summary>
    [SugarTable("ast_room")]
    public class AstRoomEntity : IEntity<AstRoomEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_RoomCode { get; set; }
        public string F_RoomName { get; set; }
        /// <summary>教室/功能室/实验室/体育室/宿舍/办公</summary>
        public string F_RoomType { get; set; }
        public string F_Building { get; set; }
        public string F_ManagerId { get; set; }
        public string F_Description { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>资产分类</summary>
    [SugarTable("ast_category")]
    public class AstCategoryEntity : IEntity<AstCategoryEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_CategoryCode { get; set; }
        public string F_CategoryName { get; set; }
        public string F_ParentId { get; set; }
        public int? F_DepreciationYears { get; set; }
        public bool? F_IsConsumable { get; set; }
        public int? F_SortCode { get; set; }
        public string F_Description { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }

    /// <summary>供应商</summary>
    [SugarTable("ast_supplier")]
    public class AstSupplierEntity : IEntity<AstSupplierEntity>, ICreationAudited, IModificationAudited, IDeleteAudited
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public string F_Id { get; set; }
        public string F_SupplierName { get; set; }
        public string F_ContactPerson { get; set; }
        public string F_Phone { get; set; }
        public string F_Address { get; set; }
        public string F_Description { get; set; }
        public bool? F_EnabledMark { get; set; }
        public bool? F_DeleteMark { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }
}
