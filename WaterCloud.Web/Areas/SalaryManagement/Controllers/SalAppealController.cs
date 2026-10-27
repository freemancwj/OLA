using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 薪酬申诉处理（人事回复）
    /// </summary>
    [Area("SalaryManagement")]
    public class SalAppealController : BaseController
    {
        public SalAppealService _service { get; set; }

        #region 视图

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        #endregion

        #region 查询

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<SalAppealExtend> pagination, string month, string keyword, string status)
        {
            var data = await _service.GetList(pagination, month, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        #endregion

        #region 处理

        /// <summary>回复申诉（处理完成）</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> Reply(string keyValue, string reply)
        {
            try
            {
                await _service.Reply(keyValue, reply);
                return await Success("已回复。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion
    }
}
