using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 薪酬规则参数配置（单行配置）
    /// </summary>
    [Area("SalaryManagement")]
    public class SalRuleController : BaseController
    {
        public SalRuleService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetRuleJson()
        {
            var data = await _service.GetRule();
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveRule(SalRuleEntity entity)
        {
            try
            {
                await _service.SaveRule(entity);
                return await Success("保存成功。", "", entity.F_Id);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", entity.F_Id);
            }
        }
    }
}
