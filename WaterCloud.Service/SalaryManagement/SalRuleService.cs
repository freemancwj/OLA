using SqlSugar;
using System;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;

namespace WaterCloud.Service.SalaryManagement
{
    /// <summary>薪酬规则参数服务</summary>
    public class SalRuleService : BaseService<SalRuleEntity>, IDenpendency
    {
        public SalRuleService(ISqlSugarClient context) : base(context) { }

        /// <summary>获取当前生效规则（单行，无则返回默认）</summary>
        public async Task<SalRuleEntity> GetRule()
        {
            var rule = await repository.Db.Queryable<SalRuleEntity>().OrderBy(a => a.F_CreatorTime).FirstAsync();
            if (rule == null)
            {
                rule = new SalRuleEntity { F_Id = Utils.GuId(), F_CreatorTime = DateTime.Now, F_EnabledMark = true };
                await repository.Insert(rule);
            }
            return rule;
        }

        public async Task SaveRule(SalRuleEntity entity)
        {
            var old = await GetRule();
            entity.F_Id = old.F_Id;
            entity.F_EnabledMark = true;
            entity.F_LastModifyTime = DateTime.Now;
            entity.F_LastModifyUserId = currentuser.UserId;
            await repository.Db.Updateable(entity).Where(a => a.F_Id == old.F_Id).ExecuteCommandAsync();
        }
    }
}
