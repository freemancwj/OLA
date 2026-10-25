using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Domain.SystemManage;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 题库服务
    /// </summary>
    public class QuestionService : BaseService<QuestionEntity>, IDenpendency
    {
        //科目字典
        private const string CourseItemId = "08dd9f77-fcba-446c-8a54-fb2b95717c33";

        public QuestionService(ISqlSugarClient context) : base(context)
        {
        }

        #region 获取数据

        /// <summary>
        /// 题库列表（分页）
        /// </summary>
        public async Task<List<QuestionExtend>> GetLookList(SoulPage<QuestionExtend> pagination, string keyword, string courseId, string type)
        {
            var allowed = new HashSet<string> { "F_Type", "F_TypeName", "F_Content", "F_CourseName", "F_Difficulty", "F_DifficultyName", "F_Score", "F_CreatorTime" };
            if (string.IsNullOrEmpty(pagination.field) || !allowed.Contains(pagination.field))
            {
                pagination.field = "F_CreatorTime";
                pagination.order = "desc";
            }
            else if (pagination.order != "asc" && pagination.order != "desc")
            {
                pagination.order = "desc";
            }
            var sql = BuildQuerySql(keyword, courseId, type);
            return await repository.Db.SqlQueryable<QuestionExtend>(sql).ToPageListAsync(pagination);
        }

        private string BuildQuerySql(string keyword, string courseId, string type)
        {
            var sb = new StringBuilder();
            sb.Append(@"
SELECT a.*,
       c.F_ItemName AS F_CourseName,
       CASE a.F_Type WHEN 1 THEN N'单选题' WHEN 2 THEN N'多选题' WHEN 3 THEN N'判断题' WHEN 4 THEN N'简答题' ELSE N'' END AS F_TypeName,
       CASE a.F_Difficulty WHEN 1 THEN N'易' WHEN 2 THEN N'中' WHEN 3 THEN N'难' ELSE N'' END AS F_DifficultyName
FROM school_question a
LEFT JOIN sys_itemsdetail c ON a.F_CourseId = c.F_Id COLLATE Chinese_PRC_CI_AS
WHERE ISNULL(a.F_DeleteMark,0) = 0");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = Q(keyword.Trim());
                sb.Append(" AND (a.F_Content COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR a.F_Analysis COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%'")
                  .Append(" OR c.F_ItemName COLLATE Chinese_PRC_CI_AS LIKE N'%" + kw + "%')");
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                sb.Append(" AND a.F_CourseId = '" + Q(courseId) + "'");
            }
            if (int.TryParse(type, out var typeValue) && typeValue >= 1 && typeValue <= 4)
            {
                sb.Append(" AND a.F_Type = " + typeValue);
            }
            return sb.ToString();
        }

        private static string Q(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        public async Task<QuestionEntity> GetForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        /// <summary>
        /// 科目列表（题库所属科目下拉）
        /// </summary>
        public async Task<List<ItemsDetailEntity>> GetCourseList()
        {
            return await repository.Db.Queryable<ItemsDetailEntity>()
                .Where(a => a.F_ItemId == CourseItemId && a.F_DeleteMark == false && a.F_EnabledMark == true)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        #endregion

        #region 提交数据

        public async Task SubmitForm(QuestionEntity entity, string keyValue)
        {
            if (entity == null)
            {
                throw new Exception("题目信息不能为空！");
            }
            if (string.IsNullOrWhiteSpace(entity.F_Content))
            {
                throw new Exception("请填写题目内容！");
            }
            if (string.IsNullOrEmpty(entity.F_CourseId))
            {
                throw new Exception("请选择所属科目！");
            }
            if (!entity.F_Type.HasValue || entity.F_Type.Value < 1 || entity.F_Type.Value > 4)
            {
                throw new Exception("请选择题型！");
            }
            if (!entity.F_Score.HasValue)
            {
                throw new Exception("请填写默认分值！");
            }

            entity.F_Content = entity.F_Content.Trim();
            entity.F_OptionA = TrimOption(entity.F_OptionA);
            entity.F_OptionB = TrimOption(entity.F_OptionB);
            entity.F_OptionC = TrimOption(entity.F_OptionC);
            entity.F_OptionD = TrimOption(entity.F_OptionD);
            var type = entity.F_Type.Value;
            var answer = (entity.F_Answer ?? string.Empty).Trim();

            if (type == 1)
            {
                //单选：答案只能是 A/B/C/D 其中一个，且对应选项不能为空
                if (answer.Length != 1 || "ABCD".IndexOf(answer, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw new Exception("单选题答案必须为A、B、C、D中的一个！");
                }
                var letter = char.ToUpperInvariant(answer[0]);
                if (string.IsNullOrWhiteSpace(GetOption(entity, letter)))
                {
                    throw new Exception("选项" + letter + "内容不能为空！");
                }
                entity.F_Answer = letter.ToString();
            }
            else if (type == 2)
            {
                //多选：答案必须是2个以上去重后的大写字母，且对应选项都不能为空
                answer = answer.ToUpperInvariant().Replace(" ", string.Empty);
                if (answer.Any(a => a < 'A' || a > 'D'))
                {
                    throw new Exception("多选题答案只能包含A、B、C、D！");
                }
                var letters = answer.Distinct().OrderBy(a => a).ToList();
                if (letters.Count < 2)
                {
                    throw new Exception("多选题答案至少选择两项！");
                }
                foreach (var letter in letters)
                {
                    if (string.IsNullOrWhiteSpace(GetOption(entity, letter)))
                    {
                        throw new Exception("选项" + letter + "内容不能为空！");
                    }
                }
                entity.F_Answer = new string(letters.ToArray());
            }
            else if (type == 3)
            {
                //判断：答案固定为 A(正确) 或 B(错误)
                var letter = answer.ToUpperInvariant();
                if (letter != "A" && letter != "B")
                {
                    throw new Exception("判断题答案只能为正确或错误！");
                }
                entity.F_Answer = letter;
            }
            else
            {
                //简答：参考答案必填
                if (string.IsNullOrWhiteSpace(answer))
                {
                    throw new Exception("请填写简答题参考答案！");
                }
                entity.F_Answer = answer;
            }

            //隐藏的选项不保留历史值（使用空字符串，避免更新时被忽略）
            if (type == 3)
            {
                entity.F_OptionC = string.Empty;
                entity.F_OptionD = string.Empty;
            }
            else if (type == 4)
            {
                entity.F_OptionA = string.Empty;
                entity.F_OptionB = string.Empty;
                entity.F_OptionC = string.Empty;
                entity.F_OptionD = string.Empty;
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
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeleteForm(string keyValue)
        {
            var ids = keyValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length == 0)
            {
                throw new Exception("请选择需要删除的题目！");
            }
            await repository.Update(a => ids.Contains(a.F_Id), a => new QuestionEntity
            {
                F_DeleteMark = true,
                F_DeleteTime = DateTime.Now,
                F_DeleteUserId = currentuser.UserId
            });
        }

        #endregion

        #region 辅助方法

        private static string TrimOption(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string GetOption(QuestionEntity entity, char letter)
        {
            switch (letter)
            {
                case 'A':
                    return entity.F_OptionA;
                case 'B':
                    return entity.F_OptionB;
                case 'C':
                    return entity.F_OptionC;
                case 'D':
                    return entity.F_OptionD;
                default:
                    return null;
            }
        }

        #endregion
    }
}
