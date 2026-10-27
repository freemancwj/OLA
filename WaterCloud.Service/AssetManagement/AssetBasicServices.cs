using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;

namespace WaterCloud.Service.AssetManagement
{
    /// <summary>基础数据通用 CRUD 基类（按名称/编码模糊检索）</summary>
    public abstract class AstBasicService<TEntity> : BaseService<TEntity>, IDenpendency
        where TEntity : class, new()
    {
        public AstBasicService(ISqlSugarClient context) : base(context) { }

        protected abstract string NameField { get; }
        protected abstract string CodeField { get; }

        public async Task<List<TEntity>> GetList(SoulPage<TEntity> pagination, string keyword)
        {
            var queryable = repository.Db.Queryable<TEntity>().Where("F_DeleteMark = @dm", new { dm = false });
            if (!string.IsNullOrEmpty(keyword))
            {
                var like = "%" + keyword + "%";
                if (!string.IsNullOrEmpty(CodeField))
                    queryable = queryable.Where($"({NameField} like @kw OR {CodeField} like @kw)", new { kw = like });
                else
                    queryable = queryable.Where($"{NameField} like @kw", new { kw = like });
            }
            pagination.count = await queryable.CountAsync();
            return await queryable.OrderBy("F_CreatorTime desc").ToPageListAsync(pagination.page, pagination.rows);
        }

        public async Task<TEntity> GetForm(string keyValue)
            => await repository.FindEntity(keyValue);

        public async Task SubmitForm(TEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                typeof(TEntity).GetProperty("F_Id").SetValue(entity, Utils.GuId());
                typeof(TEntity).GetProperty("F_CreatorTime").SetValue(entity, DateTime.Now);
                typeof(TEntity).GetProperty("F_CreatorUserId").SetValue(entity, currentuser.UserId);
                typeof(TEntity).GetProperty("F_DeleteMark").SetValue(entity, false);
                await repository.Insert(entity);
            }
            else
            {
                typeof(TEntity).GetProperty("F_LastModifyTime").SetValue(entity, DateTime.Now);
                typeof(TEntity).GetProperty("F_LastModifyUserId").SetValue(entity, currentuser.UserId);
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            await repository.Db.Deleteable<TEntity>().In(ids).ExecuteCommandAsync();
        }
    }

    public class AstGradeService : AstBasicService<AstGradeEntity>
    {
        public AstGradeService(ISqlSugarClient c) : base(c) { }
        protected override string NameField => "F_GradeName";
        protected override string CodeField => "F_GradeCode";
    }

    public class AstClassService : AstBasicService<AstClassEntity>
    {
        public AstClassService(ISqlSugarClient c) : base(c) { }
        protected override string NameField => "F_ClassName";
        protected override string CodeField => null;
    }

    public class AstRoomService : AstBasicService<AstRoomEntity>
    {
        public AstRoomService(ISqlSugarClient c) : base(c) { }
        protected override string NameField => "F_RoomName";
        protected override string CodeField => "F_RoomCode";
    }

    public class AstCategoryService : AstBasicService<AstCategoryEntity>
    {
        public AstCategoryService(ISqlSugarClient c) : base(c) { }
        protected override string NameField => "F_CategoryName";
        protected override string CodeField => "F_CategoryCode";
    }

    public class AstSupplierService : AstBasicService<AstSupplierEntity>
    {
        public AstSupplierService(ISqlSugarClient c) : base(c) { }
        protected override string NameField => "F_SupplierName";
        protected override string CodeField => null;
    }
}
