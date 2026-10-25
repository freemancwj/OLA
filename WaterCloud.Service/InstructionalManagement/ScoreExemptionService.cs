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
    /// 免考/缓考/缺考登记服务
    /// </summary>
    public class ScoreExemptionService : BaseService<ScoreExemptionEntity>, IDenpendency
    {
        /// <summary>
        /// 学生角色
        /// </summary>
        private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

        /// <summary>
        /// 任课班级服务（按教师带班过滤数据范围）
        /// </summary>
        public TeacherClassService teacherClassApp { get; set; }

        public ScoreExemptionService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 登记列表（分页）。非管理员按学生所在班级过滤
        /// </summary>
        public async Task<List<ScoreExemptionExtend>> GetLookList(SoulPage<ScoreExemptionExtend> pagination, string keyword, string type, string status)
        {
            var allowedFields = new HashSet<string> { "F_StudentName", "F_Account", "F_ClassName", "F_CourseName", "F_Type", "F_Status", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowedFields.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            var sb = new StringBuilder();
            sb.Append(BaseSql + " WHERE ISNULL(a.F_DeleteMark,0) = 0");
            //非管理员：学生所属班级必须在可见班级范围内（学生F_OrganizeId逗号分隔，逗号包裹后模糊匹配）
            if (visibleClassIds != null)
            {
                if (visibleClassIds.Count == 0)
                {
                    sb.Append(" AND 1 = 0");
                }
                else
                {
                    sb.Append(" AND EXISTS(SELECT 1 FROM sys_organize x WHERE x.F_CategoryId = 'banji'")
                      .Append(" AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE '%,' + x.F_Id + ',%'")
                      .Append(" AND x.F_Id IN ('" + string.Join("','", visibleClassIds.Select(Q)) + "'))");
                }
            }
            if (!string.IsNullOrEmpty(type))
            {
                sb.Append(" AND a.F_Type = N'" + Q(type) + "'");
            }
            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out var statusValue))
            {
                sb.Append(" AND ISNULL(a.F_Status,0) = " + statusValue);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (u.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR u.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_Name COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.Db.SqlQueryable<ScoreExemptionExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 单条登记（含学生/课程等显示字段）
        /// </summary>
        public async Task<ScoreExemptionExtend> GetForm(string keyValue)
        {
            var sql = BaseSql + " WHERE ISNULL(a.F_DeleteMark,0) = 0 AND a.F_Id = '" + Q(keyValue) + "'";
            return await repository.Db.SqlQueryable<ScoreExemptionExtend>(sql).FirstAsync();
        }

        /// <summary>
        /// 列表基础SQL（班级名由学生所属班级子查询取得）
        /// </summary>
        private string BaseSql
        {
            get
            {
                return @"
SELECT a.*,
       u.F_RealName AS F_StudentName, u.F_Account,
       (SELECT TOP 1 o.F_FullName FROM sys_organize o WHERE o.F_CategoryId = 'banji'
           AND (',' + ISNULL(u.F_OrganizeId,'') + ',') LIKE '%,' + o.F_Id + ',%') AS F_ClassName,
       c.F_Name AS F_CourseName,
       au.F_RealName AS F_AuditUserName
FROM school_score_exemption a
LEFT JOIN sys_user u ON a.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN school_course c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user au ON a.F_AuditUserId = au.F_Id COLLATE Chinese_PRC_CI_AS";
            }
        }

        /// <summary>
        /// 学生下拉（启用未删除，最多200条）
        /// </summary>
        public async Task<List<UserEntity>> GetStudentList(string keyword)
        {
            var query = repository.Db.Queryable<UserEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true
                    && a.F_RoleId != null && a.F_RoleId.Contains(StudentRoleId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(a => a.F_RealName.Contains(kw) || a.F_Account.Contains(kw));
            }
            return await query.OrderBy(a => a.F_Account).Take(200).ToListAsync();
        }

        /// <summary>
        /// 有效课程下拉
        /// </summary>
        public async Task<List<CourseEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<CourseEntity>()
                .Where(a => a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(ScoreExemptionEntity entity, string keyValue)
        {
            if (entity == null || string.IsNullOrEmpty(entity.F_StudentId))
            {
                throw new Exception("请选择学生");
            }
            if (string.IsNullOrEmpty(entity.F_CourseId))
            {
                throw new Exception("请选择课程");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Type))
            {
                throw new Exception("请选择类型");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Reason))
            {
                throw new Exception("请填写原因");
            }
            //同学生+同课程+同类型，仅允许存在一条未拒绝的登记
            var duplicate = await repository.IQueryable()
                .Where(a => a.F_DeleteMark == false && a.F_StudentId == entity.F_StudentId
                    && a.F_CourseId == entity.F_CourseId && a.F_Type == entity.F_Type
                    && (a.F_Status == null || a.F_Status != 2)
                    && (string.IsNullOrEmpty(keyValue) || a.F_Id != keyValue))
                .AnyAsync();
            if (duplicate)
            {
                throw new Exception("已存在该类型的登记");
            }
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Status = 0;
                entity.F_AuditUserId = null;
                entity.F_AuditTime = null;
                entity.F_AuditRemark = null;
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
                if (data.F_Status != 0)
                {
                    throw new Exception("仅待审批的登记允许修改");
                }
                data.F_StudentId = entity.F_StudentId;
                data.F_CourseId = entity.F_CourseId;
                data.F_Type = entity.F_Type;
                data.F_Reason = entity.F_Reason;
                data.Modify(keyValue);
                await repository.Update(data);
            }
        }

        /// <summary>
        /// 审批登记（仅管理员，不改动成绩表）
        /// </summary>
        public async Task Audit(string keyValue, int status, string remark)
        {
            if (!currentuser.IsSuperAdmin && !currentuser.IsAdmin)
            {
                throw new Exception("仅管理员可审批");
            }
            if (status != 1 && status != 2)
            {
                throw new Exception("审批状态不正确");
            }
            var data = await repository.FindEntity(keyValue);
            if (data == null || data.F_DeleteMark == true)
            {
                throw new Exception("登记记录不存在");
            }
            if (data.F_Status != 0)
            {
                throw new Exception("该登记已审批，请勿重复操作");
            }
            data.F_Status = status;
            data.F_AuditUserId = currentuser.UserId;
            data.F_AuditTime = DateTime.Now;
            data.F_AuditRemark = remark;
            data.Modify(keyValue);
            await repository.Update(data);
        }

        /// <summary>
        /// 批量软删（已通过的登记不允许删除）
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = await repository.IQueryable().Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false).ToListAsync();
            foreach (var item in list)
            {
                if (item.F_Status == 1)
                {
                    throw new Exception("已通过的登记不允许删除");
                }
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ScoreExemptionEntity
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
