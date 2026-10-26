using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.EducationalAffairs;

namespace WaterCloud.Service.EducationalAffairs
{
    /// <summary>
    /// 教研活动培训通知服务
    /// </summary>
    public class TeachingActivityService : BaseService<TeachingActivityEntity>, IDenpendency
    {
        public TeachingActivityService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<TeachingActivityEntity>> GetLookList(SoulPage<TeachingActivityEntity> pagination, string keyword, string type)
        {
            var allowedFields = new HashSet<string> { "F_Title", "F_StartTime", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_Title.Contains(kw) || a.F_Location.Contains(kw) || a.F_Speaker.Contains(kw));
            }
            if (!string.IsNullOrEmpty(type))
            {
                var t = int.Parse(type);
                query = query.Where(a => a.F_Type == t);
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<TeachingActivityEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(TeachingActivityEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Title))
            {
                throw new Exception("活动标题不能为空");
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
                var data = await repository.FindEntity(keyValue);
                if (data == null || data.F_DeleteMark == true)
                {
                    throw new Exception("记录不存在");
                }
                data.F_Title = entity.F_Title;
                data.F_Type = entity.F_Type;
                data.F_StartTime = entity.F_StartTime;
                data.F_EndTime = entity.F_EndTime;
                data.F_Location = entity.F_Location;
                data.F_Speaker = entity.F_Speaker;
                data.F_Content = entity.F_Content;
                data.F_FileUrl = entity.F_FileUrl;
                data.F_FileName = entity.F_FileName;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new TeachingActivityEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
