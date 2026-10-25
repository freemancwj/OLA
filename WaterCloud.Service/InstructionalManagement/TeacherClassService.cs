using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemOrganize;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 教师任课班级服务（维护教师-班级映射，供学生/成绩模块按班过滤）
    /// </summary>
    public class TeacherClassService : BaseService<TeacherClassEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        public TeacherClassService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 任课关系列表（分页）
        /// </summary>
        public async Task<List<TeacherClassExtend>> GetLookList(SoulPage<TeacherClassExtend> pagination, string keyword, string classId)
        {
            var allowedFields = new HashSet<string> { "F_Account", "F_TeacherName", "F_ClassName", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.F_Id, a.F_TeacherId, a.F_ClassId, a.F_CreatorTime,
       u.F_Account, u.F_RealName AS F_TeacherName,
       o.F_FullName AS F_ClassName
FROM school_teacher_class a
LEFT JOIN sys_user u ON a.F_TeacherId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND a.F_ClassId = '" + Q(classId) + "'");
            }
            return await repository.Db.SqlQueryable<TeacherClassExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        public async Task<TeacherClassEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 教师下拉（非学生、非删除的启用用户）
        /// </summary>
        public async Task<List<UserEntity>> GetTeacherList(string keyword)
        {
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && (a.F_RoleId == null || !a.F_RoleId.Contains(StudentRoleId)));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_RealName.Contains(kw) || a.F_Account.Contains(kw));
            }
            return await query.OrderBy(a => a.F_Account).Take(200).ToListAsync();
        }

        /// <summary>
        /// 班级下拉
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            return await repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_EnCode)
                .ToListAsync();
        }

        /// <summary>
        /// 当前用户可见的班级Id集合。
        /// 返回 null 表示不限制（管理员）；返回空列表表示无任何可见班级。
        /// 学生：仅本班；教师：按任课映射。
        /// </summary>
        public async Task<List<string>> GetVisibleClassIds()
        {
            if (currentuser == null || string.IsNullOrEmpty(currentuser.UserId))
            {
                return new List<string>();
            }
            if (currentuser.IsSuperAdmin || currentuser.IsAdmin)
            {
                return null;
            }
            //校长、教务主任可查看全部学生与成绩
            if (await IsManagerRole())
            {
                return null;
            }
            if (!string.IsNullOrEmpty(currentuser.RoleId) && currentuser.RoleId.Contains(StudentRoleId))
            {
                //学生只看本班（F_OrganizeId 可能逗号分隔，取班级类节点）
                var orgIds = (currentuser.OrganizeId ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                if (orgIds.Count == 0)
                {
                    return new List<string>();
                }
                var classIds = await repository.Db.Queryable<OrganizeEntity>()
                    .Where(a => orgIds.Contains(a.F_Id) && a.F_CategoryId == "banji")
                    .Select(a => a.F_Id)
                    .ToListAsync();
                return classIds;
            }
            //教师：任课映射表 ∪ 账号所属部门中的班级节点（两种方式均可配置带班关系）
            var list = await repository.Db.Queryable<TeacherClassEntity>()
                .Where(a => a.F_TeacherId == currentuser.UserId && a.F_DeleteMark == false && (a.F_EnabledMark == null || a.F_EnabledMark == true))
                .Select(a => a.F_ClassId)
                .ToListAsync();
            var teacherOrgIds = (currentuser.OrganizeId ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
            if (teacherOrgIds.Count > 0)
            {
                var orgClasses = await repository.Db.Queryable<OrganizeEntity>()
                    .Where(a => teacherOrgIds.Contains(a.F_Id) && a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true)
                    .Select(a => a.F_Id)
                    .ToListAsync();
                list.AddRange(orgClasses);
            }
            return list.Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
        }

        /// <summary>
        /// 是否校长/教务主任（可查看全部学生与成绩）
        /// </summary>
        private async Task<bool> IsManagerRole()
        {
            if (currentuser == null || string.IsNullOrEmpty(currentuser.RoleId))
            {
                return false;
            }
            var roleIds = currentuser.RoleId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim()).ToList();
            if (roleIds.Count == 0)
            {
                return false;
            }
            return await repository.Db.Queryable<RoleEntity>()
                .Where(a => roleIds.Contains(a.F_Id))
                .Where(a => a.F_EnCode == "R_05" || a.F_EnCode == "R_07" || a.F_EnCode == "G_01" || a.F_EnCode == "G_02"
                    || a.F_FullName == "校长" || a.F_FullName == "教务主任")
                .AnyAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(TeacherClassEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.F_TeacherId))
            {
                throw new Exception("请选择教师");
            }
            if (string.IsNullOrWhiteSpace(entity.F_ClassId))
            {
                throw new Exception("请选择班级");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_TeacherId == entity.F_TeacherId && a.F_ClassId == entity.F_ClassId && a.F_DeleteMark == false
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该教师已关联此班级，请勿重复添加");
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
                data.F_TeacherId = entity.F_TeacherId;
                data.F_ClassId = entity.F_ClassId;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await repository.Update(a => ids.Contains(a.F_Id), a => new TeacherClassEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
