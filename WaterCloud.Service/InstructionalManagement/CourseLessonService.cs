using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using WaterCloud.Code;
using WaterCloud.DataBase;
using WaterCloud.Domain.InstructionalManagement;

namespace WaterCloud.Service.InstructionalManagement
{
    /// <summary>
    /// 章节与课时服务
    /// </summary>
    public class CourseLessonService : BaseService<CourseChapterEntity>, IDenpendency
    {
        public CourseLessonService(ISqlSugarClient context) : base(context)
        {
        }

        #region 章节

        public async Task<List<CourseChapterEntity>> GetChapterList(string courseId)
        {
            return await repository.IQueryable()
                .Where(a => a.F_CourseId == courseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        public async Task<CourseChapterEntity> GetChapterForm(string keyValue)
        {
            return await repository.FindEntity(keyValue);
        }

        public async Task SubmitChapterForm(CourseChapterEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                if (entity.F_SortCode == null)
                {
                    var max = await repository.Db.Queryable<CourseChapterEntity>()
                        .Where(a => a.F_CourseId == entity.F_CourseId && a.F_DeleteMark == false)
                        .MaxAsync(a => (int?)a.F_SortCode);
                    entity.F_SortCode = (max ?? 0) + 1;
                }
                entity.Create();
                await repository.Insert(entity);
            }
            else
            {
                entity.Modify(keyValue);
                await repository.Update(entity);
            }
        }

        public async Task DeleteChapterForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            if (await repository.Db.Queryable<CourseLessonEntity>().AnyAsync(a => ids.Contains(a.F_ChapterId) && a.F_DeleteMark == false))
            {
                throw new Exception("章节下存在课时，请先删除课时");
            }
            await repository.Delete(a => ids.Contains(a.F_Id));
        }

        /// <summary>
        /// 章节上移/下移
        /// </summary>
        public async Task MoveChapter(string keyValue, string direction)
        {
            var current = await repository.FindEntity(keyValue);
            if (current == null) throw new Exception("章节不存在");
            var siblings = await repository.Db.Queryable<CourseChapterEntity>()
                .Where(a => a.F_CourseId == current.F_CourseId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
            var index = siblings.FindIndex(a => a.F_Id == current.F_Id);
            int targetIndex = direction == "up" ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
            {
                throw new Exception("无法移动");
            }
            var target = siblings[targetIndex];
            var tmpSort = current.F_SortCode;
            current.F_SortCode = target.F_SortCode;
            target.F_SortCode = tmpSort;
            try
            {
                repository.Db.Ado.BeginTran();
                await repository.Db.Updateable(current).ExecuteCommandAsync();
                await repository.Db.Updateable(target).ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        #endregion

        #region 课时

        public async Task<List<CourseLessonEntity>> GetLessonList(string chapterId)
        {
            return await repository.Db.Queryable<CourseLessonEntity>()
                .Where(a => a.F_ChapterId == chapterId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
        }

        public async Task<CourseLessonEntity> GetLessonForm(string keyValue)
        {
            return await repository.Db.Queryable<CourseLessonEntity>().FirstAsync(a => a.F_Id == keyValue);
        }

        public async Task SubmitLessonForm(CourseLessonEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_DeleteMark = false;
                entity.F_EnabledMark = true;
                if (entity.F_SortCode == null)
                {
                    var max = await repository.Db.Queryable<CourseLessonEntity>()
                        .Where(a => a.F_ChapterId == entity.F_ChapterId && a.F_DeleteMark == false)
                        .MaxAsync(a => (int?)a.F_SortCode);
                    entity.F_SortCode = (max ?? 0) + 1;
                }
                entity.Create();
                await repository.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                entity.Modify(keyValue);
                await repository.Db.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
            }
        }

        public async Task DeleteLessonForm(string keyValue)
        {
            var ids = keyValue.Split(',');
            // 物理删除课时及其学习进度
            await repository.Db.Deleteable<CourseLessonEntity>(a => ids.Contains(a.F_Id)).ExecuteCommandAsync();
            await repository.Db.Deleteable<CourseProgressEntity>(a => ids.Contains(a.F_LessonId)).ExecuteCommandAsync();
        }

        /// <summary>
        /// 课时上移/下移
        /// </summary>
        public async Task MoveLesson(string keyValue, string direction)
        {
            var current = await repository.Db.Queryable<CourseLessonEntity>().FirstAsync(a => a.F_Id == keyValue);
            if (current == null) throw new Exception("课时不存在");
            var siblings = await repository.Db.Queryable<CourseLessonEntity>()
                .Where(a => a.F_ChapterId == current.F_ChapterId && a.F_DeleteMark == false)
                .OrderBy(a => a.F_SortCode)
                .ToListAsync();
            var index = siblings.FindIndex(a => a.F_Id == current.F_Id);
            int targetIndex = direction == "up" ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
            {
                throw new Exception("无法移动");
            }
            var target = siblings[targetIndex];
            var tmpSort = current.F_SortCode;
            current.F_SortCode = target.F_SortCode;
            target.F_SortCode = tmpSort;
            try
            {
                repository.Db.Ado.BeginTran();
                await repository.Db.Updateable(current).ExecuteCommandAsync();
                await repository.Db.Updateable(target).ExecuteCommandAsync();
                repository.Db.Ado.CommitTran();
            }
            catch
            {
                repository.Db.Ado.RollbackTran();
                throw;
            }
        }

        #endregion
    }
}
