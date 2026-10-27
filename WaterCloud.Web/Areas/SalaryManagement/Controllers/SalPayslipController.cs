using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 工资单管理：自动算薪、审批流转、归档锁定、审计日志
    /// </summary>
    [Area("SalaryManagement")]
    public class SalPayslipController : BaseController
    {
        public SalPayslipService _service { get; set; }

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
        public async Task<ActionResult> GetGridJson(SoulPage<SalPayslipExtend> pagination, string month, string keyword, string status)
        {
            var data = await _service.GetList(pagination, month, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetDetailsJson(string keyValue)
        {
            var data = await _service.GetDetails(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetAuditLogJson(SoulPage<SalAuditLogExtend> pagination, string month, string keyword)
        {
            var data = await _service.GetAuditLogs(pagination, month, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        #endregion

        #region 算薪与审批

        /// <summary>对某月执行自动算薪</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> CalculateMonth(string month)
        {
            try
            {
                var count = await _service.CalculateMonth(month);
                return await Success($"算薪完成，共生成/更新 {count} 人工资单。", "", month);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", month);
            }
        }

        /// <summary>人事提交审批</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> SubmitApprove(string keyValue)
        {
            try
            {
                await _service.SubmitApprove(keyValue);
                return await Success("已提交审批。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        /// <summary>逐级审批（教务→财务→校长）</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> Approve(string keyValue, string remark)
        {
            try
            {
                await _service.Approve(keyValue, remark);
                return await Success("审批通过。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        /// <summary>驳回到草稿</summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> Reject(string keyValue, string remark)
        {
            try
            {
                await _service.Reject(keyValue, remark);
                return await Success("已驳回。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion
    }
}
