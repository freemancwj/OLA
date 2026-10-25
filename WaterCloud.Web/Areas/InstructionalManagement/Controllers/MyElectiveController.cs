using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 学生选课（学生端）
    /// </summary>
    [Area("InstructionalManagement")]
    public class MyElectiveController : BaseController
    {
        public ElectiveRecordService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 当前开放批次信息
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetOpenRoundJson()
        {
            var data = await _service.GetOpenRound();
            return Content(data == null ? "{}" : data.ToJson());
        }

        /// <summary>
        /// 可选课程列表
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetAvailableJson(string roundId)
        {
            var data = await _service.GetAvailableList(roundId);
            return Success(data.Count, data);
        }

        /// <summary>
        /// 我的选课
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetMineJson(string roundId)
        {
            var data = await _service.GetMyList(roundId);
            return Success(data.Count, data);
        }

        /// <summary>
        /// 选课
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Select(string keyValue)
        {
            try
            {
                await _service.SelectCourse(keyValue);
                return await Success("选课成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        /// <summary>
        /// 退课
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Drop(string keyValue)
        {
            try
            {
                await _service.DropCourse(keyValue);
                return await Success("退课成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }
    }
}
