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
    /// 选课名单（管理端）
    /// </summary>
    [Area("InstructionalManagement")]
    public class ElectiveRecordController : BaseController
    {
        public ElectiveRecordService _service { get; set; }
        public ElectiveRoundService _roundService { get; set; }
        public ElectiveCourseService _courseService { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ElectiveRecordExtend> pagination, string roundId, string electiveCourseId, string keyword)
        {
            var data = await _service.GetLookList(pagination, roundId, electiveCourseId, keyword);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetRoundListJson()
        {
            var data = await _roundService.GetRoundList();
            return Content(data.Select(a => new { a.F_Id, a.F_Name, a.F_Semester }).ToList().ToJson());
        }

        /// <summary>
        /// 批次下可选课程下拉
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetCourseListJson(string roundId)
        {
            var pagination = new SoulPage<ElectiveCourseExtend> { page = 1, rows = 1000 };
            var data = await _courseService.GetLookList(pagination, roundId, "");
            return Content(data.Select(a => new { a.F_Id, a.F_CourseName, a.F_TeacherName }).ToList().ToJson());
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 移除学生选课
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try
            {
                await _service.DeleteForm(keyValue);
                return await Success("移除成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }

        #endregion
    }
}
