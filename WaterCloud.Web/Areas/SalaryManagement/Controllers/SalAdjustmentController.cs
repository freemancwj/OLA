using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 薪酬手动调整项（临时加发/扣款，留痕）
    /// </summary>
    [Area("SalaryManagement")]
    public class SalAdjustmentController : BaseController
    {
        public SalAdjustmentService _service { get; set; }
        public SalProfileService _profileService { get; set; }

        #region 视图

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Form()
        {
            return View();
        }

        #endregion

        #region 查询

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<SalAdjustmentExtend> pagination, string month, string keyword)
        {
            var data = await _service.GetList(pagination, month, keyword);
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
        public async Task<ActionResult> GetProfileUserListJson()
        {
            var data = await _profileService.GetProfileUsers();
            return Content(data.Select(a => new { F_Id = a.F_Id, F_Account = a.F_Account, F_RealName = a.F_RealName }).ToList().ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(SalAdjustmentEntity entity, string keyValue)
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

        #endregion
    }
}
