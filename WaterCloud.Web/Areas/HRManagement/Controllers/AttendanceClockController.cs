using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Service.HRManagement;

namespace WaterCloud.Web.Areas.HRManagement.Controllers
{
    /// <summary>
    /// 员工打卡
    /// </summary>
    [Area("HRManagement")]
    public class AttendanceClockController : BaseController
    {
        public AttendanceService _service { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 今日打卡状态 + 打卡规则
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetTodayStatusJson()
        {
            var rule = await _service.GetRule();
            var record = await _service.GetTodayRecord();
            return Content(new
            {
                Rule = new
                {
                    rule.F_StartTime,
                    rule.F_EndTime,
                    rule.F_LateGrace,
                    rule.F_EarlyGrace,
                    rule.F_EnabledMark
                },
                Record = record
            }.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClockIn()
        {
            try
            {
                var msg = await _service.ClockIn();
                return await Success(msg, "", null);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "");
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClockOut()
        {
            try
            {
                var msg = await _service.ClockOut();
                return await Success(msg, "", null);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "");
            }
        }

        /// <summary>
        /// 我的打卡记录
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetMyGridJson(SoulPage<AttendanceRecordEntity> pagination, string month)
        {
            var data = await _service.GetMyRecords(pagination, month);
            return Content(pagination.setData(data).ToJson());
        }
    }
}
