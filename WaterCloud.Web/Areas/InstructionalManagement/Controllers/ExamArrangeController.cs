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
    /// 考务安排
    /// </summary>
    [Area("InstructionalManagement")]
    public class ExamArrangeController : BaseController
    {
        public ExamArrangeService _service { get; set; }

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
        /// 监考管理
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Invigilator()
        {
            return View();
        }

        #endregion

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ExamArrangeExtend> pagination, string keyword, string classId, string courseId, string examDate)
        {
            var data = await _service.GetLookList(pagination, keyword, classId, courseId, examDate);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetInvigilatorGridJson(SoulPage<ExamArrangeExtend> pagination, string teacherId, string keyword)
        {
            var data = await _service.GetInvigilatorList(pagination, teacherId, keyword);
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
        public async Task<ActionResult> GetCourseListJson()
        {
            var data = await _service.GetCourseList();
            return Content(data.Select(a => new { a.F_Id, a.F_Name }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetClassListJson()
        {
            var data = await _service.GetClassList();
            return Content(data.Select(a => new { a.F_Id, a.F_FullName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetClassroomListJson()
        {
            var data = await _service.GetClassroomList();
            return Content(data.Select(a => new { a.F_Id, ClassroomName = a.F_BuildingName + "-" + a.F_Name + "(容量" + a.F_Capacity + ")" }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetPeriodListJson()
        {
            var data = await _service.GetPeriodList();
            return Content(data.Select(a => new { a.F_Id, PeriodName = a.F_Name + "(" + a.F_StartTime + "-" + a.F_EndTime + ")" }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetTeacherListJson(string keyword)
        {
            var data = await _service.GetTeacherList(keyword);
            return Content(data.Select(a => new { a.F_Id, TeacherName = a.F_RealName + "(" + a.F_Account + ")" }).ToList().ToJson());
        }

        #endregion

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ExamArrangeEntity entity, string keyValue)
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
