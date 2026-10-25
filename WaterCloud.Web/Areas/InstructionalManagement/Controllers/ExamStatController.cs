using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 补考清考统计
    /// </summary>
    [Area("InstructionalManagement")]
    public class ExamStatController : BaseController
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
        public async Task<ActionResult> GetGridJson(string semester)
        {
            var data = await _service.GetExamStat(semester);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSemesterListJson()
        {
            var data = await _service.GetSemesterList();
            return Success(data.Count, data);
        }
    }
}
