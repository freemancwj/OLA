using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Service.HRManagement;

namespace WaterCloud.Web.Areas.HRManagement.Controllers
{
    /// <summary>
    /// 打卡规则设置
    /// </summary>
    [Area("HRManagement")]
    public class AttendanceRuleController : BaseController
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
        public async Task<ActionResult> GetFormJson()
        {
            var data = await _service.GetRule();
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AttendanceRuleEntity entity)
        {
            try
            {
                await _service.SaveRule(entity);
                return await Success("保存成功。", "", null);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "");
            }
        }
    }
}
