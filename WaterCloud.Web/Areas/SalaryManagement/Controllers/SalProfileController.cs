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
    /// 教职工薪酬档案
    /// </summary>
    [Area("SalaryManagement")]
    public class SalProfileController : BaseController
    {
        public SalProfileService _service { get; set; }

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
        public async Task<ActionResult> GetGridJson(SoulPage<SalProfileExtend> pagination, string keyword, string postType, string titleLevel)
        {
            var data = await _service.GetList(pagination, keyword, postType, titleLevel);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>未建薪酬档案的用户（新增时选择）</summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetUnarchivedUserListJson()
        {
            var data = await _service.GetUnarchivedUsers();
            return Content(data.Select(a => new { F_Id = a.F_Id, F_Account = a.F_Account, F_RealName = a.F_RealName }).ToList().ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(SalProfileEntity entity, string keyValue)
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
