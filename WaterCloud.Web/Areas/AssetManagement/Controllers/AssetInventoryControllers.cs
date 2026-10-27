using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;
using WaterCloud.Service.AssetManagement;

namespace WaterCloud.Web.Areas.AssetManagement.Controllers
{
    /// <summary>低值易耗品库存 + 出入库</summary>
    [Area("AssetManagement")]
    public class AstConsumableController : BaseController
    {
        public AstConsumableService _service { get; set; }

        [HttpGet, HandlerAuthorize] public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize] public override ActionResult Form() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstConsumableEntity> pagination, string keyword)
        {
            var data = await _service.GetList(pagination, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue) => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstConsumableEntity entity, string keyValue)
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

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> OperateStock(AstConsumableRecordEntity record)
        {
            try { await _service.OperateStock(record); return Success("操作成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetRecordJson(SoulPage<AstConsumableRecordEntity> pagination, string consumableId)
        {
            var data = await _service.GetRecordList(pagination, consumableId);
            return Content(pagination.setData(data).ToJson());
        }
    }

    /// <summary>学期盘点</summary>
    [Area("AssetManagement")]
    public class AstInventoryController : BaseController
    {
        public AstInventoryService _service { get; set; }

        [HttpGet, HandlerAuthorize] public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize] public override ActionResult Form() => View();
        [HttpGet, HandlerAuthorize] public ActionResult Detail() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstInventoryEntity> pagination, string keyword)
        {
            var data = await _service.GetList(pagination, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue) => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstInventoryEntity entity, string keyValue)
        {
            try { await _service.SubmitForm(entity, keyValue); return await Success("操作成功。", "", keyValue); }
            catch (Exception ex) { return await Error(ex.Message, "", keyValue); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> GenerateItems(string inventoryId, string gradeId, string classId)
        {
            try { var msg = await _service.GenerateItems(inventoryId, gradeId, classId); return Success(msg); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetItemsJson(string inventoryId)
            => Content((await _service.GetItems(inventoryId)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmItem(string itemId, string actualStatus, string remark)
        {
            try { await _service.ConfirmItem(itemId, actualStatus, remark); return Success("确认成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> FinishInventory(string inventoryId)
        {
            try { await _service.FinishInventory(inventoryId); return Success("盘点已完成。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
    }
}
