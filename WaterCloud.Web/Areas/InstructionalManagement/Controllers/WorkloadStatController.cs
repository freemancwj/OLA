using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 教师工作量统计
    /// </summary>
    [Area("InstructionalManagement")]
    public class WorkloadStatController : BaseController
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
            var data = await _service.GetWorkloadStat(semester);
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
