using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service.SystemOrganize;

namespace WaterCloud.Web.Areas.FlowManage.Controllers
{
    /// <summary>
    /// 描 述：请假管理控制器（教师/学生请假，走工作流审批）
    /// </summary>
    [Area("FlowManage")]
    public class LeaveApplicationController : BaseController
    {
        public LeaveApplicationService _service { get; set; }

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<LeaveApplicationEntity> pagination, string keyword, string leaveType, string status)
        {
            if (string.IsNullOrEmpty(pagination.field))
            {
                pagination.field = "F_Id";
                pagination.order = "desc";
            }
            var data = await _service.GetList(pagination, keyword, leaveType, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #endregion 获取数据

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        public async Task<ActionResult> SubmitForm(LeaveApplicationEntity entity, string keyValue)
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
        public async Task<ActionResult> CancleForm(string keyValue)
        {
            try
            {
                await _service.CancleForm(keyValue);
                return await Success("撤回成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try
            {
                await _service.DeleteForm(keyValue);
                return await Success("操作成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }

        #endregion 提交数据
    }
}
