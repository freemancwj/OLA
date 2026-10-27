using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.AssetManagement;
using WaterCloud.Service.AssetManagement;

namespace WaterCloud.Web.Areas.AssetManagement.Controllers
{
    [Area("AssetManagement")]
    public abstract class AstBasicController<TEntity, TService> : BaseController
        where TEntity : class, new()
        where TService : AstBasicService<TEntity>
    {
        public TService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index() => View();

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Form() => View();

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<TEntity> pagination, string keyword)
        {
            var data = await _service.GetList(pagination, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(TEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitForm(entity, keyValue);
                return await Success("操作成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        [HandlerAuthorize]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try
            {
                await _service.DeleteForm(keyValue);
                return await Success("删除成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }
    }

    public class AstGradeController : AstBasicController<AstGradeEntity, AstGradeService> { }
    public class AstClassController : AstBasicController<AstClassEntity, AstClassService> { }
    public class AstRoomController : AstBasicController<AstRoomEntity, AstRoomService> { }
    public class AstCategoryController : AstBasicController<AstCategoryEntity, AstCategoryService> { }
    public class AstSupplierController : AstBasicController<AstSupplierEntity, AstSupplierService> { }
}
