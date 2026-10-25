using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 在线学习（学生端）
    /// </summary>
    [Area("InstructionalManagement")]
    public class CourseStudyController : BaseController
    {
        public CourseService _courseService { get; set; }
        public CourseProgressService _progressService { get; set; }

        #region 视图

        [HttpGet]
        public ActionResult Play()
        {
            return View();
        }

        #endregion

        #region 数据

        /// <summary>
        /// 已上架课程卡片
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetCardsJson(string keyword, string categoryId)
        {
            var data = await _courseService.GetStudyCards(keyword, categoryId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 课程学习详情（章节+课时+资料+进度）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetStudyInfoJson(string courseId)
        {
            try
            {
                var data = await _courseService.GetStudyInfo(courseId);
                return Content(new AlwaysResult<object> { state = ResultType.success.ToString(), message = "", data = data }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new AlwaysResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        /// <summary>
        /// 保存学习进度
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> SaveProgress(string lessonId, int position, int duration)
        {
            try
            {
                await _progressService.SaveProgress(lessonId, position, duration);
                return Content(new AlwaysResult { state = ResultType.success.ToString(), message = "" }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new AlwaysResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        /// <summary>
        /// 获取某课时进度
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetProgress(string lessonId)
        {
            var data = await _progressService.GetProgress(lessonId);
            return Content(data.ToJson());
        }

        #endregion
    }
}
