using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 成绩修改申请服务（教师发起、管理员审批，通过后回写成绩）
    /// </summary>
    public class ScoreChangeService : BaseService<ScoreChangeEntity>, IDenpendency
    {
        //成绩类型字典
        private const string ScoreTypeItemId = "08dde7aa-d604-4397-82ad-902671433107";

        /// <summary>
        /// 任课班级服务（按教师带班过滤数据范围）
        /// </summary>
        public TeacherClassService teacherClassApp { get; set; }

        public ScoreChangeService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 申请列表（分页）。非管理员只看自己发起的申请
        /// </summary>
        public async Task<List<ScoreChangeExtend>> GetLookList(SoulPage<ScoreChangeExtend> pagination, string keyword, string status)
        {
            var allowedFields = new HashSet<string> { "F_StudentName", "F_Account", "F_ClassName", "F_CourseName", "F_OldScore", "F_NewScore", "F_Status", "F_CreatorTime" };
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
            sb.Append(BaseSql + " WHERE ISNULL(a.F_DeleteMark,0) = 0");
            //非管理员只看自己发起的申请
            if (!currentuser.IsSuperAdmin && !currentuser.IsAdmin)
            {
                sb.Append(" AND a.F_CreatorUserId = '" + Q(currentuser.UserId) + "'");
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
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            return await repository.SqlQueryable<ScoreChangeExtend>(sb.ToString()).ToPageListAsync(pagination);
        }

        /// <summary>
        /// 单条申请（含学生/课程等显示字段）
        /// </summary>
        public async Task<ScoreChangeExtend> GetForm(string keyValue)
        {
            var sql = BaseSql + " WHERE ISNULL(a.F_DeleteMark,0) = 0 AND a.F_Id = '" + Q(keyValue) + "'";
            return await repository.SqlQueryable<ScoreChangeExtend>(sql).FirstAsync();
        }

        /// <summary>
        /// 列表基础SQL（关联成绩/学生/班级/课程/成绩类型/审批人）
        /// </summary>
        private string BaseSql
        {
            get
            {
                return @"
SELECT a.*,
       u.F_RealName AS F_StudentName, u.F_Account,
       o.F_FullName AS F_ClassName,
       c.F_ItemName AS F_CourseName,
       st.F_ItemName AS F_ScoreTypeName,
       s.F_Semester,
       au.F_RealName AS F_AuditUserName
FROM school_score_change a
LEFT JOIN school_scores s ON a.F_ScoreId = s.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_user u ON a.F_StudentId = u.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON s.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail st ON s.F_ScoreType = st.F_ItemCode COLLATE Chinese_PRC_CI_AS AND st.F_ItemId = '" + ScoreTypeItemId + @"'
LEFT JOIN sys_user au ON a.F_AuditUserId = au.F_Id COLLATE Chinese_PRC_CI_AS";
            }
        }

        /// <summary>
        /// 可申请修改的成绩记录（已审核通过、未删除，最多200条）。
        /// 非管理员按任课班级过滤
        /// </summary>
        public async Task<List<ScoresExtend>> GetScoreList(string keyword)
        {
            var visibleClassIds = await teacherClassApp.GetVisibleClassIds();
            //空列表表示无任何可见班级
            if (visibleClassIds != null && visibleClassIds.Count == 0)
            {
                return new List<ScoresExtend>();
            }
            var sb = new StringBuilder();
            sb.Append(@"
SELECT " + SqlDialect.Top(200) + @"a.F_Id, a.F_StudentId, a.F_CourseId, a.F_Score, a.F_Semester,
       b.F_RealName, b.F_Account,
       o.F_FullName AS F_ClassName,
       c.F_ItemName AS F_CourseName,
       st.F_ItemName AS F_ScoreTypeName
FROM school_scores a
LEFT JOIN sys_user b ON a.F_StudentId = b.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_organize o ON a.F_ClassId = o.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
LEFT JOIN sys_itemsdetail st ON a.F_ScoreType = st.F_ItemCode COLLATE Chinese_PRC_CI_AS AND st.F_ItemId = '" + ScoreTypeItemId + @"'
WHERE ISNULL(a.F_DeleteMark,0) = 0 AND ISNULL(a.F_AuditStatus,0) = 1");
            if (visibleClassIds != null)
            {
                sb.Append(" AND a.F_ClassId IN ('" + string.Join("','", visibleClassIds.Select(Q)) + "')");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (b.F_RealName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR b.F_Account COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR o.F_FullName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            sb.Append(" ORDER BY a.F_CreatorTime DESC" + SqlDialect.Limit(200));
            return await repository.SqlQueryable<ScoresExtend>(sb.ToString()).ToListAsync();
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 新增申请（不允许修改）
        /// </summary>
        public async Task SubmitForm(ScoreChangeEntity entity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                throw new Exception("申请不允许修改");
            }
            if (entity == null || string.IsNullOrEmpty(entity.F_ScoreId))
            {
                throw new Exception("请选择成绩记录");
            }
            if (!entity.F_NewScore.HasValue)
            {
                throw new Exception("请填写新分数");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Reason))
            {
                throw new Exception("请填写申请原因");
            }
            //快照成绩记录
            var score = await repository.Db.Queryable<ScoresEntity>()
                .Where(a => a.F_Id == entity.F_ScoreId && a.F_DeleteMark == false)
                .FirstAsync();
            if (score == null)
            {
                throw new Exception("成绩记录不存在");
            }
            var exists = await repository.IQueryable()
                .Where(a => a.F_ScoreId == entity.F_ScoreId && a.F_Status == 0 && a.F_DeleteMark == false)
                .AnyAsync();
            if (exists)
            {
                throw new Exception("该成绩已有待审批的修改申请");
            }
            entity.F_StudentId = score.F_StudentId;
            entity.F_CourseId = score.F_CourseId;
            entity.F_OldScore = score.F_Score;
            entity.F_Status = 0;
            entity.F_AuditUserId = null;
            entity.F_AuditTime = null;
            entity.F_AuditRemark = null;
            entity.F_DeleteMark = false;
            entity.F_EnabledMark = true;
            entity.Create();
            await repository.Insert(entity);
        }

        /// <summary>
        /// 审批申请（仅管理员）。通过后回写成绩分数
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
                throw new Exception("申请记录不存在");
            }
            if (data.F_Status != 0)
            {
                throw new Exception("该申请已审批，请勿重复操作");
            }
            repository.Db.Ado.BeginTran();
            try
            {
                data.F_Status = status;
                data.F_AuditUserId = currentuser.UserId;
                data.F_AuditTime = DateTime.Now;
                data.F_AuditRemark = remark;
                data.Modify(keyValue);
                await repository.Update(data);
                if (status == 1)
                {
                    //审批通过，按主键回写成绩分数
                    await repository.Db.Updateable<ScoresEntity>()
                        .SetColumns(a => new ScoresEntity { F_Score = data.F_NewScore, F_LastModifyTime = DateTime.Now, F_LastModifyUserId = currentuser.UserId })
                        .Where(a => a.F_Id == data.F_ScoreId)
                        .ExecuteCommandAsync();
                }
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 批量软删（已通过的申请不允许删除）
        /// </summary>
        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = await repository.IQueryable().Where(a => ids.Contains(a.F_Id) && a.F_DeleteMark == false).ToListAsync();
            foreach (var item in list)
            {
                if (item.F_Status == 1)
                {
                    throw new Exception("已通过的申请不允许删除");
                }
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new ScoreChangeEntity
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
