using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 成绩统计
    /// </summary>
    [Area("InstructionalManagement")]
    public class ScoreStatController : BaseController
    {
        public StatService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(string semester, string courseId, string classId)
        {
            var data = await _service.GetScoreStat(semester, courseId, classId);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSemesterListJson()
        {
            var data = await _service.GetSemesterList();
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetCourseListJson()
        {
            var data = await _service.GetCourseList();
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetClassListJson()
        {
            var data = await _service.GetClassList();
            return Success(data.Count, data);
        }
    }
}
