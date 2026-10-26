using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.LogisticsManagement;

namespace WaterCloud.Service.LogisticsManagement
{
    /// <summary>
    /// 合同管理服务
    /// </summary>
    public class ContractService : BaseService<ContractEntity>, IDenpendency
    {
        public ContractService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        public async Task<List<ContractEntity>> GetLookList(SoulPage<ContractEntity> pagination, string keyword, string status)
        {
            var allowedFields = new HashSet<string> { "F_ContractNo", "F_Name", "F_SignDate", "F_ExpireDate", "F_CreatorTime" };
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
                query = query.Where(a => a.F_ContractNo.Contains(kw) || a.F_Name.Contains(kw) || a.F_PartyA.Contains(kw) || a.F_PartyB.Contains(kw));
            }
            if (!string.IsNullOrEmpty(status))
            {
                var s = int.Parse(status);
                query = query.Where(a => a.F_Status == s);
            }
            return await query.ToPageListAsync(pagination);
        }

        public async Task<ContractEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ContractEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_ContractNo))
            {
                throw new Exception("合同编号不能为空");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Name))
            {
                throw new Exception("合同名称不能为空");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_ContractNo == entity.F_ContractNo && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("合同编号已存在");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                entity.F_Status = entity.F_Status ?? 0;
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
                data.F_ContractNo = entity.F_ContractNo;
                data.F_Name = entity.F_Name;
                data.F_PartyA = entity.F_PartyA;
                data.F_PartyB = entity.F_PartyB;
                data.F_Amount = entity.F_Amount;
                data.F_SignDate = entity.F_SignDate;
                data.F_ExpireDate = entity.F_ExpireDate;
                data.F_Status = entity.F_Status;
                data.F_FileUrl = entity.F_FileUrl;
                data.F_FileName = entity.F_FileName;
                data.F_Remark = entity.F_Remark;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new ContractEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion
    }
}
