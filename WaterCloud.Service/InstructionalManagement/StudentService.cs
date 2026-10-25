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
    /// 学生管理服务（基本信息取 sys_user，附加信息取 school_student）
    /// </summary>
    public class StudentService : BaseService<StudentEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";
        /// <summary>
        /// 学生岗位
        /// </summary>
        private const string StudentDutyId = "08dadd10-89fd-4da0-81fc-142ee54f6c59";

        public StudentService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 任课班级服务（按教师带班过滤数据范围）
        /// </summary>
        public TeacherClassService teacherClassApp { get; set; }

        #region 获取数据

        /// <summary>
        /// 学生列表（分页）
        /// </summary>
        public async Task<List<StudentExtend>> GetLookList(SoulPage<StudentExtend> pagination, string keyword, string classId)
        {
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var allowedFields = new HashSet<string>
            {
                "F_Account", "F_RealName", "F_ClassName", "F_Gender", "F_Birthday",
                "F_StudentNo", "F_CreatorTime"
            };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_Account";
                pagination.order = "asc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sql = BuildQuerySql(keyword, classId, visibleClassIds);
            return await repository.SqlQueryable<StudentExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildQuerySql(string keyword, string classId, List<string> visibleClassIds)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT u.F_Id, u.F_Account, u.F_RealName, u.F_Gender, u.F_Birthday, u.F_MobilePhone,
       u.F_OrganizeId, u.F_EnabledMark, u.F_CreatorTime,
       s.F_Id AS F_StudentInfoId, ISNULL(NULLIF(s.F_StudentNo, ''), u.F_Account) AS F_StudentNo,
       s.F_NativePlace, s.F_ParentName, s.F_ParentPhone,
       o.F_FullName AS F_ClassName
FROM sys_user u
LEFT JOIN school_student s ON s.F_UserId = u.F_Id COLLATE Chinese_PRC_CI_AS AND ISNULL(s.F_DeleteMark, 0) = 0
LEFT JOIN sys_organize o ON u.F_OrganizeId = o.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(u.F_DeleteMark, 0) = 0 AND ISNULL(u.F_IsAdmin, 0) = 0
  AND (u.F_RoleId LIKE '%" + StudentRoleId + @"%' OR u.F_RoleId LIKE '%" + StudentDutyId + @"%')");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR s.F_StudentNo COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR s.F_ParentName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR s.F_ParentPhone COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(classId))
            {
                sb.Append(" AND u.F_OrganizeId = '" + Q(classId) + "'");
            }
            //按教师任课班级限制数据范围（null 不限制，空列表表示无任何可见班级）
            if (visibleClassIds != null)
            {
                if (visibleClassIds.Count == 0)
                {
                    sb.Append(" AND 1 = 0");
                }
                else
                {
                    sb.Append(" AND u.F_OrganizeId IN ('" + string.Join("','", visibleClassIds.Select(Q)) + "')");
                }
            }
            return sb.ToString();
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        /// <summary>
        /// 班级列表（非管理员仅返回任课/本班班级）
        /// </summary>
        public async Task<List<OrganizeEntity>> GetClassList()
        {
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var query = repository.Db.Queryable<OrganizeEntity>()
                .Where(a => a.F_CategoryId == "banji" && a.F_DeleteMark == false && a.F_EnabledMark == true);
            if (visibleClassIds != null)
            {
                query = query.Where(a => visibleClassIds.Contains(a.F_Id));
            }
            return await query.OrderBy(a => a.F_EnCode).ToListAsync();
        }

        /// <summary>
        /// 新增时的默认值（默认学生角色、学生岗位、当前用户所属学校）
        /// </summary>
        public async Task<StudentFormModel> GetInit()
        {
            var roleName = await GetRoleName(StudentRoleId);
            return new StudentFormModel
            {
                F_CompanyId = currentuser.CompanyId,
                F_RoleId = StudentRoleId,
                F_RoleName = roleName,
                F_DutyId = StudentDutyId
            };
        }

        /// <summary>
        /// 根据角色Id（支持逗号分隔）取角色名称
        /// </summary>
        private async Task<string> GetRoleName(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                return string.Empty;
            }
            var ids = roleId.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
            if (ids.Length == 0)
            {
                return string.Empty;
            }
            var names = await repository.Db.Queryable<RoleEntity>()
                .Where(a => ids.Contains(a.F_Id))
                .Select(a => a.F_FullName)
                .ToListAsync();
            return string.Join(",", names);
        }

        /// <summary>
        /// 学生表单数据
        /// </summary>
        public async Task<StudentFormModel> GetForm(string keyValue)
        {
            var user = await repository.Db.Queryable<UserEntity>().InSingleAsync(keyValue);
            if (user == null)
            {
                throw new Exception("学生信息不存在");
            }
            var stu = await repository.Db.Queryable<StudentEntity>()
                .Where(a => a.F_UserId == keyValue && a.F_DeleteMark == false)
                .FirstAsync();
            return new StudentFormModel
            {
                F_Id = user.F_Id,
                F_Account = user.F_Account,
                F_CompanyId = user.F_CompanyId,
                F_RoleId = user.F_RoleId,
                F_RoleName = await GetRoleName(user.F_RoleId),
                F_DutyId = user.F_DutyId,
                F_RealName = user.F_RealName,
                F_OrganizeId = user.F_OrganizeId,
                F_Gender = user.F_Gender,
                F_Birthday = user.F_Birthday,
                F_MobilePhone = user.F_MobilePhone,
                F_StudentNo = stu != null ? stu.F_StudentNo : null,
                F_NativePlace = stu != null ? stu.F_NativePlace : null,
                F_ParentName = stu != null ? stu.F_ParentName : null,
                F_ParentPhone = stu != null ? stu.F_ParentPhone : null
            };
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 新增/修改学生
        /// </summary>
        public async Task SubmitForm(StudentFormModel model, string keyValue)
        {
            if (model == null)
            {
                throw new Exception("参数不能为空");
            }
            if (string.IsNullOrWhiteSpace(model.F_Account))
            {
                throw new Exception("账号不能为空");
            }
            if (string.IsNullOrWhiteSpace(model.F_RealName))
            {
                throw new Exception("学生姓名不能为空");
            }
            if (string.IsNullOrWhiteSpace(model.F_CompanyId))
            {
                throw new Exception("请选择学校");
            }
            if (string.IsNullOrWhiteSpace(model.F_OrganizeId))
            {
                throw new Exception("请选择所属班级");
            }
            if (string.IsNullOrWhiteSpace(model.F_RoleId))
            {
                throw new Exception("请选择角色");
            }
            if (string.IsNullOrWhiteSpace(model.F_DutyId))
            {
                throw new Exception("请选择职位");
            }
            if (string.IsNullOrEmpty(keyValue) && string.IsNullOrWhiteSpace(model.F_UserPassword))
            {
                throw new Exception("密码不能为空");
            }
            if (!string.IsNullOrWhiteSpace(model.F_UserPassword)
                && (model.F_UserPassword.Length < 6 || model.F_UserPassword.Length > 20))
            {
                throw new Exception("密码长度需为 6-20 位");
            }
            model.F_Account = model.F_Account.Trim();
            model.F_StudentNo = string.IsNullOrWhiteSpace(model.F_StudentNo) ? null : model.F_StudentNo.Trim();

            // 账号唯一（sys_user 上有唯一索引，需在全表范围内校验）
            var accountExists = await repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_Account == model.F_Account
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (accountExists)
            {
                throw new Exception("账号已存在，请更换账号");
            }
            // 学籍号唯一（非必填，填写时才校验）
            if (!string.IsNullOrEmpty(model.F_StudentNo))
            {
                var studentNoExists = await repository.Db.Queryable<StudentEntity>()
                    .Where(a => a.F_StudentNo == model.F_StudentNo && a.F_DeleteMark == false
                        && (string.IsNullOrEmpty(keyValue) || a.F_UserId != keyValue))
                    .AnyAsync();
                if (studentNoExists)
                {
                    throw new Exception("学籍号已存在，请更换学籍号");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.F_MobilePhone) && !IsPhone(model.F_MobilePhone))
            {
                throw new Exception("学生手机号格式不正确");
            }
            if (!string.IsNullOrWhiteSpace(model.F_ParentPhone) && !IsPhone(model.F_ParentPhone))
            {
                throw new Exception("家长手机格式不正确");
            }

            repository.Db.Ado.BeginTran();
            try
            {
                if (string.IsNullOrEmpty(keyValue))
                {
                    // 新增：先建用户，再建学生附加信息
                    var user = new UserEntity
                    {
                        F_Account = model.F_Account,
                        F_RealName = model.F_RealName,
                        F_Gender = model.F_Gender,
                        F_Birthday = model.F_Birthday,
                        F_MobilePhone = model.F_MobilePhone,
                        F_OrganizeId = model.F_OrganizeId,
                        F_RoleId = model.F_RoleId,
                        F_DutyId = model.F_DutyId,
                        F_CompanyId = model.F_CompanyId,
                        F_IsAdmin = false,
                        F_IsBoss = false,
                        F_IsSenior = false,
                        F_IsLeaderInDepts = false,
                        F_EnabledMark = true,
                        F_DeleteMark = false,
                        F_Description = "学生"
                    };
                    user.Create();
                    await repository.Db.Insertable(user).ExecuteCommandAsync();

                    var logon = new UserLogOnEntity
                    {
                        F_Id = user.F_Id,
                        F_UserId = user.F_Id,
                        F_ErrorNum = 0,
                        F_UserOnLine = false,
                        F_LogOnCount = 0
                    };
                    logon.F_UserSecretkey = Md5.md5(Utils.CreateNo(), 16).ToLower();
                    logon.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(model.F_UserPassword, 32).ToLower(), logon.F_UserSecretkey).ToLower(), 32).ToLower();
                    await repository.Db.Insertable(logon).ExecuteCommandAsync();

                    var student = new StudentEntity
                    {
                        F_UserId = user.F_Id,
                        F_StudentNo = model.F_StudentNo,
                        F_NativePlace = model.F_NativePlace,
                        F_ParentName = model.F_ParentName,
                        F_ParentPhone = model.F_ParentPhone,
                        F_EnabledMark = true,
                        F_DeleteMark = false
                    };
                    student.Create();
                    await repository.Db.Insertable(student).ExecuteCommandAsync();
                }
                else
                {
                    // 修改：更新用户基本信息 + 学生附加信息
                    var user = await repository.Db.Queryable<UserEntity>().InSingleAsync(keyValue);
                    if (user == null)
                    {
                        throw new Exception("学生信息不存在");
                    }
                    user.F_Account = model.F_Account;
                    user.F_RealName = model.F_RealName;
                    user.F_Gender = model.F_Gender;
                    user.F_Birthday = model.F_Birthday;
                    user.F_MobilePhone = model.F_MobilePhone;
                    user.F_OrganizeId = model.F_OrganizeId;
                    user.F_RoleId = model.F_RoleId;
                    user.F_DutyId = model.F_DutyId;
                    user.F_CompanyId = model.F_CompanyId;
                    user.Modify(keyValue);
                    await repository.Db.Updateable(user).ExecuteCommandAsync();

                    // 修改了密码则同步更新登录信息
                    if (!string.IsNullOrWhiteSpace(model.F_UserPassword))
                    {
                        var logon = await repository.Db.Queryable<UserLogOnEntity>()
                            .Where(a => a.F_UserId == keyValue)
                            .FirstAsync();
                        var secretkey = Md5.md5(Utils.CreateNo(), 16).ToLower();
                        var password = Md5.md5(DESEncrypt.Encrypt(Md5.md5(model.F_UserPassword, 32).ToLower(), secretkey).ToLower(), 32).ToLower();
                        if (logon == null)
                        {
                            logon = new UserLogOnEntity
                            {
                                F_Id = keyValue,
                                F_UserId = keyValue,
                                F_UserSecretkey = secretkey,
                                F_UserPassword = password,
                                F_ErrorNum = 0,
                                F_UserOnLine = false,
                                F_LogOnCount = 0
                            };
                            await repository.Db.Insertable(logon).ExecuteCommandAsync();
                        }
                        else
                        {
                            logon.F_UserSecretkey = secretkey;
                            logon.F_UserPassword = password;
                            await repository.Db.Updateable(logon).ExecuteCommandAsync();
                        }
                    }

                    var student = await repository.Db.Queryable<StudentEntity>()
                        .Where(a => a.F_UserId == keyValue && a.F_DeleteMark == false)
                        .FirstAsync();
                    if (student == null)
                    {
                        student = new StudentEntity
                        {
                            F_UserId = keyValue,
                            F_StudentNo = model.F_StudentNo,
                            F_NativePlace = model.F_NativePlace,
                            F_ParentName = model.F_ParentName,
                            F_ParentPhone = model.F_ParentPhone,
                            F_EnabledMark = true,
                            F_DeleteMark = false
                        };
                        student.Create();
                        await repository.Db.Insertable(student).ExecuteCommandAsync();
                    }
                    else
                    {
                        student.F_StudentNo = model.F_StudentNo;
                        student.F_NativePlace = model.F_NativePlace;
                        student.F_ParentName = model.F_ParentName;
                        student.F_ParentPhone = model.F_ParentPhone;
                        student.Modify(student.F_Id);
                        await repository.Db.Updateable(student).ExecuteCommandAsync();
                    }
                }
                repository.Db.Ado.CommitTran();
            }
            catch (Exception)
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 删除学生（同时删除用户、登录信息、附加信息）
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("请选择要删除的学生");
            }
            var ids = keyValue.Split(',').Where(a => !string.IsNullOrEmpty(a)).ToArray();
            if (ids.Contains(currentuser.UserId))
            {
                throw new Exception("不能删除当前登录用户");
            }
            repository.Db.Ado.BeginTran();
            try
            {
                await repository.Db.Deleteable<UserEntity>().Where(a => ids.Contains(a.F_Id)).ExecuteCommandAsync();
                await repository.Db.Deleteable<UserLogOnEntity>().Where(a => ids.Contains(a.F_UserId)).ExecuteCommandAsync();
                await repository.Db.Deleteable<StudentEntity>().Where(a => ids.Contains(a.F_UserId)).ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch (Exception)
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        private static bool IsPhone(string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), @"^1\d{10}$");
        }

        #endregion
    }
}
