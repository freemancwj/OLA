/*******************************************************************************
 * Copyright © 2020 WaterCloud.Framework 版权所有
 * Author: WaterCloud
 * Description: WaterCloud快速开发平台
 * Website：
*********************************************************************************/
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service.SystemOrganize;

namespace WaterCloud.Web.Areas.FlowManage.Controllers
{
    /// <summary>
    /// 创 建：超级管理员
    /// 日 期：2023-xx-xx xx:xx
    /// 描 述：员工请假管理控制器类
    /// </summary>
    [Area("FlowManage")]
    public class LeaveApplicationController : BaseController
    {
        public LeaveApplicationService _service { get; set; }

     

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<LeaveApplicationEntity> pagination, string keyword)
        {
            if (string.IsNullOrEmpty(pagination.field))
            {
                pagination.field = "F_Id";
                pagination.order = "desc";
            }
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