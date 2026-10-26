using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.HRManagement
{
    /// <summary>
    /// 员工档案服务（扩展 sys_user 的人事信息）
    /// </summary>
    public class HrEmployeeService : BaseService<HrEmployeeEntity>, IDenpendency
    {
        public HrEmployeeService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 员工档案列表（联查 sys_user + sys_organize）
        /// </summary>
        public async Task<List<HrEmployeeExtend>> GetList(SoulPage<HrEmployeeExtend> pagination, string keyword, string organizeId, string education)
        {
            var queryable = repository.Db.Queryable<HrEmployeeEntity, UserEntity>((e, u) => new object[]
            {
                JoinType.Left, e.F_UserId == u.F_Id
            });
            queryable = queryable.Where((e, u) => e.F_DeleteMark == false && u.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword))
            {
                queryable = queryable.Where((e, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword) || u.F_MobilePhone.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(organizeId))
            {
                queryable = queryable.Where((e, u) => u.F_OrganizeId.Contains(organizeId));
            }
            if (!string.IsNullOrEmpty(education))
            {
                var edu = int.Parse(education);
                queryable = queryable.Where((e, u) => e.F_Education == edu);
            }
            // 联表查询不能用字符串排序，先按用户创建时间倒序
            queryable = queryable.OrderBy((e, u) => u.F_CreatorTime, OrderByType.Desc);
            var query = queryable.Select((e, u) => new HrEmployeeExtend
            {
                F_Id = e.F_Id,
                F_UserId = e.F_UserId,
                F_Education = e.F_Education,
                F_EntryDate = e.F_EntryDate,
                F_RegularDate = e.F_RegularDate,
                F_Photo = e.F_Photo,
                F_IdCard = e.F_IdCard,
                F_MaritalStatus = e.F_MaritalStatus,
                F_PoliticalStatus = e.F_PoliticalStatus,
                F_GraduateSchool = e.F_GraduateSchool,
                F_Major = e.F_Major,
                F_EmergencyContact = e.F_EmergencyContact,
                F_EmergencyPhone = e.F_EmergencyPhone,
                F_Address = e.F_Address,
                F_ContractNo = e.F_ContractNo,
                F_ContractStart = e.F_ContractStart,
                F_ContractEnd = e.F_ContractEnd,
                F_LeaveDate = e.F_LeaveDate,
                F_Remark = e.F_Remark,
                F_EnabledMark = e.F_EnabledMark,
                F_DeleteMark = e.F_DeleteMark,
                F_CreatorTime = e.F_CreatorTime,
                F_CreatorUserId = e.F_CreatorUserId,
                F_LastModifyTime = e.F_LastModifyTime,
                F_LastModifyUserId = e.F_LastModifyUserId,
                F_DeleteTime = e.F_DeleteTime,
                F_DeleteUserId = e.F_DeleteUserId,
                F_Account = u.F_Account,
                F_RealName = u.F_RealName,
                F_Gender = u.F_Gender,
                F_Birthday = u.F_Birthday,
                F_MobilePhone = u.F_MobilePhone,
                F_Email = u.F_Email,
                F_OrganizeName = u.F_OrganizeId,
                F_RoleName = u.F_RoleId
            });
            pagination.count = await query.CountAsync();
            var list = await query.ToPageListAsync(pagination.page, pagination.rows);
            // 补充部门名称和角色名称
            if (list.Count > 0)
            {
                var orgIds = list.Where(a => !string.IsNullOrEmpty(a.F_OrganizeName))
                    .SelectMany(a => a.F_OrganizeName.Split(',')).Distinct().ToList();
                var roleIds = list.Where(a => !string.IsNullOrEmpty(a.F_RoleName))
                    .SelectMany(a => a.F_RoleName.Split(',')).Distinct().ToList();
                var orgDict = new Dictionary<string, string>();
                var roleDict = new Dictionary<string, string>();
                if (orgIds.Count() > 0)
                {
                    var orgs = await repository.Db.Queryable<OrganizeEntity>()
                        .Where(a => orgIds.Contains(a.F_Id)).Select(a => new { a.F_Id, a.F_FullName }).ToListAsync();
                    orgDict = orgs.ToDictionary(a => a.F_Id, a => a.F_FullName);
                }
                if (roleIds.Count() > 0)
                {
                    var roles = await repository.Db.Queryable<RoleEntity>()
                        .Where(a => roleIds.Contains(a.F_Id)).Select(a => new { a.F_Id, a.F_FullName }).ToListAsync();
                    roleDict = roles.ToDictionary(a => a.F_Id, a => a.F_FullName);
                }
                foreach (var item in list)
                {
                    item.F_OrganizeName = ResolveNames(item.F_OrganizeName, orgDict);
                    item.F_RoleName = ResolveNames(item.F_RoleName, roleDict);
                }
            }
            return list;
        }

        private string ResolveNames(string ids, Dictionary<string, string> dict)
        {
            if (string.IsNullOrEmpty(ids)) return "";
            var parts = ids.Split(',').Select(a => a.Trim()).Where(a => a.Length > 0).ToList();
            var names = parts.Select(a => dict.ContainsKey(a) ? dict[a] : a).ToList();
            return string.Join(",", names);
        }

        /// <summary>
        /// 获取单个员工档案
        /// </summary>
        public async Task<HrEmployeeEntity> GetForm(string keyValue)
        {
            return await repository.Db.Queryable<HrEmployeeEntity>()
                .Where(a => a.F_Id == keyValue && a.F_DeleteMark == false)
                .FirstAsync();
        }

        /// <summary>
        /// 根据用户Id获取档案
        /// </summary>
        public async Task<HrEmployeeEntity> GetByUserId(string userId)
        {
            return await repository.Db.Queryable<HrEmployeeEntity>()
                .Where(a => a.F_UserId == userId && a.F_DeleteMark == false)
                .FirstAsync();
        }

        /// <summary>
        /// 提交员工档案（新增或修改）
        /// </summary>
        public async Task SubmitForm(HrEmployeeEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                // 检查是否已存在该用户的档案
                var exist = await GetByUserId(entity.F_UserId);
                if (exist != null)
                {
                    throw new Exception("该用户已存在员工档案，请勿重复添加");
                }
                entity.F_Id = Utils.GuId();
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                await repository.Insert(entity);
            }
            else
            {
                var old = await GetForm(keyValue);
                if (old == null)
                {
                    throw new Exception("员工档案不存在");
                }
                // 不允许修改关联用户
                entity.F_UserId = old.F_UserId;
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(entity).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 删除员工档案
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            await repository.Delete(a => a.F_Id == keyValue);
        }

        /// <summary>
        /// 获取未建档的用户列表（用于新增档案时选择）
        /// </summary>
        public async Task<List<UserEntity>> GetUnarchivedUsers()
        {
            var archivedUserIds = await repository.Db.Queryable<HrEmployeeEntity>()
                .Where(a => a.F_DeleteMark == false)
                .Select(a => a.F_UserId).ToListAsync();
            return await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true && !archivedUserIds.Contains(a.F_Id))
                .OrderBy(a => a.F_CreatorTime, OrderByType.Desc)
                .ToListAsync();
        }
    }
}
