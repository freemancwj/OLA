using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 调课申请与审批
    /// </summary>
    [Area("InstructionalManagement")]
    public class ScheduleChangeController : BaseController
    {
        public ScheduleChangeService _service { get; set; }

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

        /// <summary>
        /// 审批页
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult AuditForm()
        {
            return View();
        }

        #endregion

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ScheduleChangeExtend> pagination, string keyword, int? status)
        {
            var data = await _service.GetLookList(pagination, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetFormDetail(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 可申请调课的课表格子（管理员全部，教师仅本人授课）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetMyTimetableJson(string semester)
        {
            var data = await _service.GetMyTimetableList(semester);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 教室下拉
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetClassroomListJson()
        {
            var data = await _service.GetClassroomList();
            return Content(data.Select(a => new { a.F_Id, a.F_Name }).ToList().ToJson());
        }

        #endregion

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ScheduleChangeEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitForm(entity, keyValue);
                return await Success("申请已提交，等待审批。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        /// <summary>
        /// 审批（status 1通过 2拒绝）
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Audit(string keyValue, int status, string remark)
        {
            try
            {
                await _service.Audit(keyValue, status, remark);
                return await Success("审批完成。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
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
