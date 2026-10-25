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
    /// 课程资料服务
    /// </summary>
    public class CourseWareService : BaseService<CourseWareEntity>, IDenpendency
    {
        public CourseWareService(ISqlSugarClient context) : base(context)
        {
        }

        public async Task<List<CourseWareEntity>> GetList(string courseId)
        {
            return await repository.IQueryable()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_CreatorTime, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<CourseWareEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        public async Task SubmitForm(CourseWareEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.F_DownCount = 0;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            await repository.Delete(a => ids.Contains(a.F_Id));
        }

        /// <summary>
        /// 下载计数 +1
        /// </summary>
        public async Task AddDownCount(string keyValue)
        {
            await repository.Db.Updateable<CourseWareEntity>()
                .SetColumns(a => a.F_DownCount == (a.F_DownCount ?? 0) + 1)
                .Where(a => a.F_Id == keyValue)
                .ExecuteCommandAsync();
        }
    }
}
