using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 课表管理（授课计划、自动排课、课表查询）
    /// </summary>
    [Area("InstructionalManagement")]
    public class TimetableController : BaseController
    {
        public TimetableService _service { get; set; }

        /// <summary>
        /// 课表管理页（授课计划维护 + 自动排课 + 手工调整）
        /// </summary>
        public ActionResult Manage()
        {
            return View();
        }

        #region 基础数据
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetClassListJson()
        {
            var data = await _service.GetClassList();
            return Content(data.Select(a => new { a.F_Id, a.F_FullName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetCourseListJson()
        {
            var data = await _service.GetCourseList();
            return Content(data.Select(a => new { a.F_Id, a.F_ItemName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetTeacherListJson()
        {
            var data = await _service.GetTeacherList();
            return Content(data.Select(a => new { a.F_Id, a.F_RealName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetStudentListJson(string classId)
        {
            var data = await _service.GetStudentList(classId);
            return Content(data.Select(a => new { a.F_Id, a.F_RealName, a.F_Account }).ToList().ToJson());
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

        /// <summary>
        /// 排课参数（每周天数/每天节数）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetScheduleParamJson()
        {
            var data = await _service.GetScheduleParam();
            return Content(new { data.F_DaysPerWeek, data.F_SectionsPerDay }.ToJson());
        }
        #endregion

        #region 授课计划
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetPlanGridJson(string semester, string classId)
        {
            var data = await _service.GetPlanList(semester, classId);
            return Success(data.Count, data);
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetPlanFormJson(string keyValue)
        {
            var data = await _service.GetPlanForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 授课计划中的学期下拉
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSemesterListJson()
        {
            var data = await _service.GetSemesterList();
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitPlanForm(TimetablePlanEntity entity, string keyValue)
        {
            try
            {
                await _service.SubmitPlanForm(entity, keyValue);
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
        public async Task<ActionResult> DeletePlanForm(string keyValue)
        {
            try
            {
                await _service.DeletePlanForm(keyValue);
                return await Success("删除成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }
        #endregion

        #region 自动排课
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AutoGenerate(string semester, string classIds)
        {
            try
            {
                var ids = (classIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                var msg = await _service.AutoGenerate(semester, ids);
                return await Success(msg, "", "");
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", "");
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearTimetable(string semester, string classId)
        {
            try
            {
                await _service.ClearTimetable(semester, classId);
                return await Success("清空成功。", "", "");
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", "");
            }
        }
        #endregion

        #region 课表查询与格子编辑
        /// <summary>
        /// 课表矩阵数据（班级/教师/学生三选一）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetTimetableJson(string semester, string classId, string teacherId, string studentId)
        {
            var data = await _service.GetTimetable(semester, classId, teacherId, studentId);
            return Success(data.Count, data);
        }

        /// <summary>
        /// 保存格子（手工排课/调整）
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCell(TimetableEntity entity)
        {
            try
            {
                await _service.SaveCell(entity);
                return await Success("保存成功。", "", "");
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", "");
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearCell(string keyValue)
        {
            try
            {
                await _service.ClearCell(keyValue);
                return await Success("清除成功。", "", "");
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", "");
            }
        }
        #endregion
    }
}
