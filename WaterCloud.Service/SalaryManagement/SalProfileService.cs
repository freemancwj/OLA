using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.SalaryManagement
{
    /// <summary>教职工薪酬档案服务</summary>
    public class SalProfileService : BaseService<SalProfileEntity>, IDenpendency
    {
        public SalProfileService(ISqlSugarClient context) : base(context) { }

        public async Task<List<SalProfileExtend>> GetList(SoulPage<SalProfileExtend> pagination, string keyword, string postType, string titleLevel)
        {
            var queryable = repository.Db.Queryable<SalProfileEntity, UserEntity>((e, u) => new object[] { JoinType.Left, e.F_UserId == u.F_Id })
                .Where((e, u) => e.F_DeleteMark == false && u.F_DeleteMark == false);
            if (!string.IsNullOrEmpty(keyword)) queryable = queryable.Where((e, u) => u.F_Account.Contains(keyword) || u.F_RealName.Contains(keyword) || e.F_JobNumber.Contains(keyword));
            if (!string.IsNullOrEmpty(postType)) { var p = int.Parse(postType); queryable = queryable.Where((e, u) => e.F_PostType == p); }
            if (!string.IsNullOrEmpty(titleLevel)) { var t = int.Parse(titleLevel); queryable = queryable.Where((e, u) => e.F_TitleLevel == t); }
            queryable = queryable.OrderBy((e, u) => e.F_CreatorTime, OrderByType.Desc);
            var query = queryable.Select((e, u) => new SalProfileExtend
            {
                F_Id = e.F_Id, F_UserId = e.F_UserId, F_JobNumber = e.F_JobNumber, F_EntryDate = e.F_EntryDate,
                F_TeachingYears = e.F_TeachingYears, F_PostType = e.F_PostType, F_TitleLevel = e.F_TitleLevel,
                F_IsHeadTeacher = e.F_IsHeadTeacher, F_IsDeputyHead = e.F_IsDeputyHead, F_IsGroupLeader = e.F_IsGroupLeader,
                F_IsLessonLeader = e.F_IsLessonLeader, F_IsGraduationTeacher = e.F_IsGraduationTeacher, F_ClassId = e.F_ClassId,
                F_SocialBase = e.F_SocialBase, F_TaxDeduction = e.F_TaxDeduction, F_Remark = e.F_Remark,
                F_EnabledMark = e.F_EnabledMark, F_DeleteMark = e.F_DeleteMark, F_CreatorTime = e.F_CreatorTime,
                F_Account = u.F_Account, F_RealName = u.F_RealName, F_OrganizeName = u.F_OrganizeId, F_ClassName = e.F_ClassId
            });
            pagination.count = await query.CountAsync();
            var list = await query.ToPageListAsync(pagination.page, pagination.rows);
            // 解析部门/班级名称
            if (list.Count > 0)
            {
                var ids = list.SelectMany(a => (a.F_OrganizeName ?? "").Split(',')).Concat(list.Select(a => a.F_ClassName ?? "")).Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
                if (ids.Count > 0)
                {
                    var orgs = await repository.Db.Queryable<OrganizeEntity>().Where(a => ids.Contains(a.F_Id)).Select(a => new { a.F_Id, a.F_FullName }).ToListAsync();
                    var dict = orgs.ToDictionary(a => a.F_Id, a => a.F_FullName);
                    foreach (var item in list)
                    {
                        item.F_OrganizeName = ResolveNames(item.F_OrganizeName, dict);
                        item.F_ClassName = dict.ContainsKey(item.F_ClassName ?? "") ? dict[item.F_ClassName] : "";
                    }
                }
            }
            return list;
        }
        private string ResolveNames(string ids, Dictionary<string, string> dict)
        {
            if (string.IsNullOrEmpty(ids)) return "";
            return string.Join(",", ids.Split(',').Select(a => a.Trim()).Where(a => a.Length > 0).Select(a => dict.ContainsKey(a) ? dict[a] : a));
        }

        public async Task<SalProfileEntity> GetForm(string keyValue) => await repository.Db.Queryable<SalProfileEntity>().Where(a => a.F_Id == keyValue && a.F_DeleteMark == false).FirstAsync();
        public async Task<SalProfileEntity> GetByUserId(string userId) => await repository.Db.Queryable<SalProfileEntity>().Where(a => a.F_UserId == userId && a.F_DeleteMark == false).FirstAsync();

        public async Task SubmitForm(SalProfileEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                if (await GetByUserId(entity.F_UserId) != null) throw new Exception("该用户已存在薪酬档案，请勿重复添加");
                entity.F_Id = Utils.GuId(); entity.F_CreatorTime = DateTime.Now; entity.F_CreatorUserId = currentuser.UserId; entity.F_DeleteMark = false; entity.F_EnabledMark = true;
                await repository.Insert(entity);
            }
            else
            {
                var old = await GetForm(keyValue); if (old == null) throw new Exception("薪酬档案不存在");
                entity.F_UserId = old.F_UserId; entity.F_LastModifyTime = DateTime.Now; entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Db.Updateable(entity).Where(a => a.F_Id == keyValue).ExecuteCommandAsync();
            }
        }
        public async Task DeleteForm(string keyValue) => await repository.Delete(a => a.F_Id == keyValue);

        /// <summary>已有薪酬档案的用户Id列表</summary>
        public async Task<List<string>> GetProfileUserIds() => await repository.Db.Queryable<SalProfileEntity>().Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true).Select(a => a.F_UserId).ToListAsync();

        /// <summary>未建薪酬档案的用户（新增档案时选择）</summary>
        public async Task<List<UserEntity>> GetUnarchivedUsers()
        {
            var ids = await GetProfileUserIds();
            return await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true && !ids.Contains(a.F_Id))
                .OrderBy(a => a.F_CreatorTime, OrderByType.Desc).ToListAsync();
        }

        /// <summary>已建薪酬档案的用户（采集数据录入时选择教职工）</summary>
        public async Task<List<UserEntity>> GetProfileUsers()
        {
            var ids = await GetProfileUserIds();
            if (ids.Count == 0) return new List<UserEntity>();
            return await repository.Db.Queryable<UserEntity>()
                .Where(a => ids.Contains(a.F_Id))
                .OrderBy(a => a.F_Account).ToListAsync();
        }
    }
}
