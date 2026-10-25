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
    /// 考试时间段服务
    /// </summary>
    public class ExamPeriodService : BaseService<ExamPeriodEntity>, IDenpendency
    {
        public ExamPeriodService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 考试时间段列表（分页）
        /// </summary>
        public async Task<List<ExamPeriodEntity>> GetLookList(SoulPage<ExamPeriodEntity> pagination, string keyword)
        {
            var allowedFields = new HashSet<string> { "F_Name", "F_StartTime", "F_EndTime", "F_SortCode", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_SortCode";
                pagination.order = "asc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "asc";
            }
            var query = repository.IQueryable().Where(a => a.F_DeleteMark == false);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_Name.Contains(kw));
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<ExamPeriodEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 时间段下拉列表
        /// </summary>
        public async Task<List<ExamPeriodEntity>> GetPeriodList()
        {
            return await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ExamPeriodEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("时段名称不能为空");
            }
            if (string.IsNullOrWhiteSpace(entity.F_StartTime) || string.IsNullOrWhiteSpace(entity.F_EndTime))
            {
                throw new Exception("开始/结束时间不能为空");
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
                data.F_Name = entity.F_Name;
                data.F_StartTime = entity.F_StartTime;
                data.F_EndTime = entity.F_EndTime;
                data.F_SortCode = entity.F_SortCode;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ExamPeriodEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
