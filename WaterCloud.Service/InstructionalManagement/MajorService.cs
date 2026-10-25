using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 专业管理服务（复用 sys_organize 表，F_CategoryId = "zhuanye"）
    /// </summary>
    public class MajorService : BaseService<OrganizeEntity>, IDenpendency
    {
        public MajorService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 专业列表（分页）
        /// </summary>
        public async Task<List<OrganizeEntity>> GetLookList(SoulPage<OrganizeEntity> pagination, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_EnCode", "F_FullName", "F_SortCode", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_SortCode";
                pagination.order = "asc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "asc";
            }
            var query = repository.IQueryable()
                .Where(a => a.F_CategoryId == "zhuanye" && a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_FullName.Contains(kw) || a.F_EnCode.Contains(kw));
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<OrganizeEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 专业下拉列表
        /// </summary>
        public async Task<List<OrganizeEntity>> GetMajorList()
        {
            return await repository.IQueryable()
                .Where(a => a.F_CategoryId == "zhuanye" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(OrganizeEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_EnCode))
            {
                throw new Exception("专业代码不能为空");
            }
            if (string.IsNullOrWhiteSpace(entity.F_FullName))
            {
                throw new Exception("专业名称不能为空");
            }
            //专业代码在 zhuanye 类中唯一
            var exists = await repository.IQueryable()
                .Where(a => a.F_CategoryId == "zhuanye" && a.F_DeleteMark == false
                    && a.F_EnCode == entity.F_EnCode
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("专业代码已存在");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_CategoryId = "zhuanye";
                //父级为学校根节点，没有学校节点时挂到根
                var school = await repository.IQueryable()
                    .Where(a => a.F_CategoryId == "school" && a.F_DeleteMark == false)
                    .FirstAsync();
                entity.F_ParentId = school != null ? school.F_Id : "0";
                entity.F_Layers = 2;
                entity.F_EnabledMark = true;
                entity.F_DeleteMark = false;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                var data = await repository.FindEntity(keyValue);
                if (data == null || data.F_DeleteMark == true)
                {
                    throw new Exception("记录不存在");
                }
                data.F_EnCode = entity.F_EnCode;
                data.F_FullName = entity.F_FullName;
                data.F_ShortName = entity.F_ShortName;
                data.F_SortCode = entity.F_SortCode;
                data.F_Description = entity.F_Description;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new OrganizeEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
