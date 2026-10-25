using System;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 排课参数服务（单条配置）
    /// </summary>
    public class ScheduleParamService : BaseService<ScheduleParamEntity>, IDenpendency
    {
        public ScheduleParamService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 获取排课参数（无记录时返回默认值）
        /// </summary>
        public async Task<ScheduleParamEntity> GetParam()
        {
            var data = await repository.IQueryable().Where(a => a.F_DeleteMark == false).FirstAsync();
            if (data == null)
            {
                data = new ScheduleParamEntity
                {
                    F_DaysPerWeek = 5,
                    F_SectionsPerDay = 8,
                    F_MaxSameCourseDaily = 2,
                    F_MaxConsecutive = 2
                };
            }
            return data;
        }

        /// <summary>
        /// 保存排课参数（存在则更新，不存在则新增）
        /// </summary>
        public async Task SubmitForm(ScheduleParamEntity entity)
        {
            if (entity == null)
            {
                throw new Exception("参数不能为空");
            }
            if (entity.F_DaysPerWeek == null || entity.F_DaysPerWeek < 1 || entity.F_DaysPerWeek > 7)
            {
                throw new Exception("每周排课天数须在1-7之间");
            }
            if (entity.F_SectionsPerDay == null || entity.F_SectionsPerDay < 1 || entity.F_SectionsPerDay > 20)
            {
                throw new Exception("每天节数须在1-20之间");
            }
            if (entity.F_MaxSameCourseDaily == null || entity.F_MaxSameCourseDaily < 1)
            {
                throw new Exception("同科每天最多节数须大于0");
            }
            var data = await repository.IQueryable().Where(a => a.F_DeleteMark == false).FirstAsync();
            if (data == null)
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                data.F_DaysPerWeek = entity.F_DaysPerWeek;
                data.F_SectionsPerDay = entity.F_SectionsPerDay;
                data.F_MaxSameCourseDaily = entity.F_MaxSameCourseDaily;
                data.F_MaxConsecutive = entity.F_MaxConsecutive ?? 0;
                data.F_Description = entity.F_Description;
                data.Modify(data.F_Id);
                await repository.Update(data);
            }
        }
    }
}
