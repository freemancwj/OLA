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
    /// 学习进度服务
    /// </summary>
    public class CourseProgressService : BaseService<CourseProgressEntity>, IDenpendency
    {
        public CourseProgressService(ISqlSugarClient context) : base(context)
        {
        }

        /// <summary>
        /// 保存进度（只前进不后退）
        /// </summary>
        public async Task SaveProgress(string lessonId, int position, int duration)
        {
            var userId = currentuser.UserId;
            if (string.IsNullOrEmpty(userId)) throw new Exception("未登录");
            if (position < 0) position = 0;
            var existing = await repository.Db.Queryable<CourseProgressEntity>()
                .FirstAsync(a => a.F_LessonId == lessonId && a.F_UserId == userId);
            if (existing == null)
            {
                var entity = new CourseProgressEntity
                {
                    F_LessonId = lessonId,
                    F_UserId = userId,
                    F_Position = position,
                    F_Duration = duration,
                    F_LastTime = DateTime.Now
                };
                entity.Create();
                await repository.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                // 进度只前进不后退
                if (position > (existing.F_Position ?? 0))
                {
                    existing.F_Position = position;
                }
                if (duration > 0)
                {
                    existing.F_Duration = duration;
                }
                existing.F_LastTime = DateTime.Now;
                existing.F_LastModifyTime = DateTime.Now;
                existing.F_LastModifyUserId = userId;
                await repository.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 获取某课时进度
        /// </summary>
        public async Task<CourseProgressEntity> GetProgress(string lessonId)
        {
            var userId = currentuser.UserId;
            return await repository.Db.Queryable<CourseProgressEntity>()
                .FirstAsync(a => a.F_LessonId == lessonId && a.F_UserId == userId);
        }
    }
}
