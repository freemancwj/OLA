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
    /// 选课批次服务
    /// </summary>
    public class ElectiveRoundService : BaseService<ElectiveRoundEntity>, IDenpendency
    {
        public ElectiveRoundService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<ElectiveRoundEntity>> GetLookList(SoulPage<ElectiveRoundEntity> pagination, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_Semester", "F_Name", "F_StartTime", "F_EndTime", "F_CreatorTime" };
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
                query = query.Where(a => a.F_Name.Contains(kw) || a.F_Semester.Contains(kw));
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<ElectiveRoundEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 批次下拉
        /// </summary>
        public async Task<List<ElectiveRoundEntity>> GetRoundList()
        {
            return await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false)
                .OrderBy(a => a.F_Semester)
                .ToListAsync();
        }

        /// <summary>
        /// 当前开放的批次（启用且在时间窗口内）
        /// </summary>
        public async Task<ElectiveRoundEntity> GetOpenRound()
        {
            var now = DateTime.Now;
            return await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && a.F_StartTime <= now && a.F_EndTime >= now)
                .OrderBy(a => a.F_StartTime, OrderByType.Desc)
                .FirstAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ElectiveRoundEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Semester))
            {
                throw new Exception("请填写学期");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("请填写批次名称");
            }
            if (entity.F_StartTime == null || entity.F_EndTime == null || entity.F_StartTime >= entity.F_EndTime)
            {
                throw new Exception("请正确设置选课起止时间");
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
                data.F_Semester = entity.F_Semester;
                data.F_Name = entity.F_Name;
                data.F_StartTime = entity.F_StartTime;
                data.F_EndTime = entity.F_EndTime;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            //已产生选课记录的批次不允许删除
            var hasRecord = await repository.Db.Queryable<ElectiveRecordEntity>()
                .Where(a => ids.Contains(a.F_RoundId) && a.F_DeleteMark == false)
                .AnyAsync();
            if (hasRecord)
            {
                throw new Exception("该批次已产生选课记录，不允许删除");
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ElectiveRoundEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
