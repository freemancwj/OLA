using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Service.HRManagement;

namespace WaterCloud.Web.Areas.HRManagement.Controllers
{
    /// <summary>
    /// 打卡记录查询（管理员）
    /// </summary>
    [Area("HRManagement")]
    public class AttendanceRecordController : BaseController
    {
        public AttendanceService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AttendanceRecordExtend> pagination, string keyword, string date)
        {
            var data = await _service.GetRecordList(pagination, keyword, date);
            return Content(pagination.setData(data).ToJson());
        }
    }
}
