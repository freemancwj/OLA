using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 课程分类服务
    /// </summary>
    public class CourseCategoryService : BaseService<CourseCategoryEntity>, IDenpendency
    {
        public CourseCategoryService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<CourseCategoryEntity>> GetList(string keyword = "")
        {
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.F_Name.Contains(keyword));
            }
            return await query.OrderBy(a => a.F_SortCode).ToListAsync();
        }

        public async Task<CourseCategoryEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(CourseCategoryEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(entity.F_ParentId))
            {
                entity.F_ParentId = "0";
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                if (keyValue == entity.F_ParentId)
                {
                    throw new Exception("父级不能是自己");
                }
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            if (await repository.Db.Queryable<CourseCategoryEntity>().AnyAsync(a => ids.Contains(a.F_ParentId) && a.F_DeleteMark == false))
            {
                throw new Exception("存在子分类，无法删除");
            }
            if (await repository.Db.Queryable<CourseEntity>().AnyAsync(a => ids.Contains(a.F_CategoryId) && a.F_DeleteMark == false))
            {
                throw new Exception("该分类下存在课程，无法删除");
            }
            await repository.Delete(a => ids.Contains(a.F_Id));
        }

        #endregion
    }
}
