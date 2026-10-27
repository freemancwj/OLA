using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 我的工资条（教职工自助查询，仅本人数据）
    /// </summary>
    [Area("SalaryManagement")]
    public class SalMyPayslipController : BaseController
    {
        public SalPayslipService _service { get; set; }
        public SalAppealService _appealService { get; set; }

        #region 视图

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Details()
        {
            return View();
        }

        #endregion

        #region 查询

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<SalPayslipEntity> pagination)
        {
            var data = await _service.GetMyPayslips(pagination);
            return Content(pagination.setData(data).ToJson());
        }

        /// <summary>工资明细（校验本人）</summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetDetailsJson(string keyValue)
        {
            var slip = await _service.GetForm(keyValue);
            if (slip == null || slip.F_UserId != WaterCloud.Code.OperatorProvider.Provider.GetCurrent().UserId)
            {
                throw new Exception("只能查看本人工资条");
            }
            var data = await _service.GetDetails(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 申诉

        /// <summary>对本人工资单提交申诉</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitAppeal(string payslipId, string content)
        {
            try
            {
                await _appealService.SubmitAppeal(payslipId, content);
                return await Success("申诉已提交，人事将尽快核对。", "", payslipId);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", payslipId);
            }
        }

        #endregion
    }
}
