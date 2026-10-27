using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;
using WaterCloud.Service.AssetManagement;

namespace WaterCloud.Web.Areas.AssetManagement.Controllers
{
    /// <summary>器材借用归还</summary>
    [Area("AssetManagement")]
    public class AstBorrowController : BaseController
    {
        public AstBorrowService _service { get; set; }

        [HttpGet, HandlerAuthorize] public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize] public override ActionResult Form() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstBorrowEntity> pagination, string keyword, string status)
        {
            var data = await _service.GetList(pagination, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue) => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstBorrowEntity entity, string keyValue)
        {
            try { await _service.SubmitForm(entity, keyValue); return await Success("操作成功。", "", keyValue); }
            catch (Exception ex) { return await Error(ex.Message, "", keyValue); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> ReturnAsset(string keyValue, string damageNote)
        {
            try { await _service.ReturnAsset(keyValue, damageNote); return Success("归还登记成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
    }

    /// <summary>公物报修</summary>
    [Area("AssetManagement")]
    public class AstRepairController : BaseController
    {
        public AstRepairService _service { get; set; }

        [HttpGet, HandlerAuthorize] public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize] public override ActionResult Form() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstRepairEntity> pagination, string keyword, string status)
        {
            var data = await _service.GetList(pagination, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue) => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstRepairEntity entity, string keyValue)
        {
            try { await _service.SubmitForm(entity, keyValue); return await Success("报修已提交。", "", keyValue); }
            catch (Exception ex) { return await Error(ex.Message, "", keyValue); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> AcceptRepair(string keyValue, string repairer)
        {
            try { await _service.AcceptRepair(keyValue, repairer); return Success("已受理派单。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> FinishRepair(string keyValue, decimal cost, string parts, string result)
        {
            try { await _service.FinishRepair(keyValue, cost, parts, result); return Success("维修已完成。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
    }

    /// <summary>设备维保</summary>
    [Area("AssetManagement")]
    public class AstMaintenanceController : BaseController
    {
        public AstMaintenanceService _service { get; set; }

        [HttpGet, HandlerAuthorize] public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize] public override ActionResult Form() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstMaintenanceEntity> pagination, string assetId)
        {
            var data = await _service.GetList(pagination, assetId);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue) => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstMaintenanceEntity entity, string keyValue)
        {
            try { await _service.SubmitForm(entity, keyValue); return await Success("操作成功。", "", keyValue); }
            catch (Exception ex) { return await Error(ex.Message, "", keyValue); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try { await _service.DeleteForm(keyValue); return await Success("删除成功。", "", keyValue, DbLogType.Delete); }
            catch (Exception ex) { return await Error(ex.Message, "", keyValue, DbLogType.Delete); }
        }
    }
}
