using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 选课统计
    /// </summary>
    [Area("InstructionalManagement")]
    public class ElectiveStatController : BaseController
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
        public async Task<ActionResult> GetGridJson(string roundId)
        {
            var data = await _service.GetElectiveStat(roundId);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetRoundListJson()
        {
            var data = await _service.GetRoundList();
            return Success(data.Count, data);
        }
    }
}
