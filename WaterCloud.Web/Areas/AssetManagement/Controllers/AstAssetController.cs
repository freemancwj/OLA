using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;
using WaterCloud.Service.AssetManagement;

namespace WaterCloud.Web.Areas.AssetManagement.Controllers
{
    /// <summary>资产卡片（一物一码）</summary>
    [Area("AssetManagement")]
    public class AstAssetController : BaseController
    {
        public AstAssetService _service { get; set; }

        [HttpGet, HandlerAuthorize]
        public override ActionResult Index() => View();
        [HttpGet, HandlerAuthorize]
        public override ActionResult Form() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<AstAssetEntity> pagination, string keyword, string categoryId, string status)
        {
            var data = await _service.GetList(pagination, keyword, categoryId, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
            => Content((await _service.GetForm(keyValue)).ToJson());

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(AstAssetEntity entity, string keyValue)
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

        #region 班级资产绑定
        [HttpGet, HandlerAuthorize]
        public ActionResult ClassRelation() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetClassRelationJson(SoulPage<AstClassRelationEntity> pagination, string classId, string term)
        {
            var data = await _service.GetClassRelationList(pagination, classId, term);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> BindClassAsset(AstClassRelationEntity entity)
        {
            try { await _service.BindClassAsset(entity); return Success("绑定成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> UnbindClassAsset(string keyValue)
        {
            try { await _service.UnbindClassAsset(keyValue); return Success("解绑成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
        #endregion

        #region 调拨
        [HttpGet, HandlerAuthorize]
        public ActionResult Transfer() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetTransferJson(SoulPage<AstTransferEntity> pagination, string keyword)
        {
            var data = await _service.GetTransferList(pagination, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferAsset(AstTransferEntity entity)
        {
            try { await _service.TransferAsset(entity); return Success("调拨成功。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
        #endregion

        #region 报废
        [HttpGet, HandlerAuthorize]
        public ActionResult Scrap() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetScrapJson(SoulPage<AstScrapEntity> pagination, string status)
        {
            var data = await _service.GetScrapList(pagination, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyScrap(AstScrapEntity entity)
        {
            try { await _service.ApplyScrap(entity); return Success("报废申请已提交。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }

        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> ApproveScrap(string keyValue, bool approve, string remark)
        {
            try { await _service.ApproveScrap(keyValue, approve, remark); return Success(approve ? "已批准报废。" : "已驳回。"); }
            catch (Exception ex) { return Error(ex.Message); }
        }
        #endregion

        #region 折旧
        [HttpPost, HandlerAjaxOnly, ValidateAntiForgeryToken, HandlerAuthorize]
        public async Task<ActionResult> CalcDepreciation(string period)
        {
            try { var msg = await _service.CalcDepreciation(period); return Success(msg); }
            catch (Exception ex) { return Error(ex.Message); }
        }
        #endregion
    }
}
