using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 章节与课时管理
    /// </summary>
    [Area("InstructionalManagement")]
    public class CourseLessonController : BaseController
    {
        public CourseLessonService _service { get; set; }

        #region 章节

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetChapterGridJson(string courseId)
        {
            var data = await _service.GetChapterList(courseId);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetChapterFormJson(string keyValue)
        {
            var data = await _service.GetChapterForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitChapterForm(CourseChapterEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitChapterForm(entity, keyValue);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteChapterForm(string keyValue)
        {
            try
            {
                await _service.DeleteChapterForm(keyValue);
                return await Success("删除成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveChapter(string keyValue, string direction)
        {
            try
            {
                await _service.MoveChapter(keyValue, direction);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion

        #region 课时

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetLessonGridJson(string chapterId)
        {
            var data = await _service.GetLessonList(chapterId);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetLessonFormJson(string keyValue)
        {
            var data = await _service.GetLessonForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitLessonForm(CourseLessonEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitLessonForm(entity, keyValue);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteLessonForm(string keyValue)
        {
            try
            {
                await _service.DeleteLessonForm(keyValue);
                return await Success("删除成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveLesson(string keyValue, string direction)
        {
            try
            {
                await _service.MoveLesson(keyValue, direction);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion
    }
}
