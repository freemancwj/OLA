using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 成绩管理
    /// </summary>
    [Area("InstructionalManagement")]
    public class ScoresController : BaseController
    {
        public ScoresService _service { get; set; }

        #region 视图

        [HttpGet]
        public ActionResult BatchForm()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AuditForm()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Statistics()
        {
            return View();
        }

        #endregion

        #region 列表

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ScoresExtend> pagination, string keyword, string classId, string courseId, string semester, string scoreType, string auditStatus, string minScore, string maxScore)
        {
            if (string.IsNullOrEmpty(pagination.field))
            {
                pagination.field = "F_ExamTime";
                pagination.order = "desc";
            }
            var data = await _service.GetLookList(pagination, keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetLookForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 当前用户成绩管理角色，用于页面按钮控制
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetRoleJson()
        {
            return Content(_service.RoleInfo.ToJson());
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
        public async Task<ActionResult> GetCourseListJson()
        {
            var data = await _service.GetCourseList();
            return Content(data.Select(a => new { a.F_Id, a.F_ItemCode, a.F_ItemName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetScoreTypeListJson()
        {
            var data = await _service.GetScoreTypeList();
            return Content(data.Select(a => new { a.F_ItemCode, a.F_ItemName }).ToList().ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetStudentListJson(string classId)
        {
            var data = await _service.GetStudentList(classId);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetBatchStudentsJson(string classId, string courseId, string scoreType, string semester)
        {
            var data = await _service.GetBatchStudents(classId, courseId, scoreType, semester);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSummaryJson(string keyword, string classId, string courseId, string semester, string scoreType, string auditStatus, string minScore, string maxScore)
        {
            var data = await _service.GetSummary(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetStatisticsJson(string keyword, string classId, string courseId, string semester, string scoreType, string auditStatus, string minScore, string maxScore)
        {
            var data = await _service.GetStatistics(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            return Content(data.ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ScoresEntity entity, string keyValue)
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
        public async Task<ActionResult> BatchSubmitForm(string rowsJson, string classId, string courseId, string scoreType, string semester, string examTime)
        {
            try
            {
                var count = await _service.BatchSubmitForm(rowsJson, classId, courseId, scoreType, semester, examTime);
                return await Success("成功保存" + count + "条成绩。", "", "");
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", "");
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AuditForm(string keyValue, int auditStatus, string remark)
        {
            try
            {
                await _service.AuditForm(keyValue, auditStatus, remark);
                return await Success("审核完成。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteForm(string keyValue)
        {
            try
            {
                await _service.DeleteForm(keyValue);
                return await Success("删除成功。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion

        #region 导出

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string keyword, string classId, string courseId, string semester, string scoreType, string auditStatus, string minScore, string maxScore)
        {
            var data = await _service.GetLookListAll(keyword, classId, courseId, semester, scoreType, auditStatus, minScore, maxScore);
            var rows = data.Select(a => new ScoresExportRow
            {
                学号 = a.F_Account,
                姓名 = a.F_RealName,
                班级 = a.F_ClassName,
                课程 = a.F_CourseName,
                成绩类型 = a.F_ScoreTypeName,
                分数 = a.F_Score.HasValue ? a.F_Score.Value.ToString("0.##") : "",
                等级 = a.F_ScoreLevel,
                学期 = a.F_Semester,
                考试时间 = a.F_ExamTime.HasValue ? a.F_ExamTime.Value.ToString("yyyy-MM-dd") : "",
                审核状态 = GetAuditStatusName(a.F_AuditStatus),
                录入人 = a.F_CreatorUserName,
                备注 = a.F_Remak
            }).ToList();
            using var ms = new MemoryStream();
            ms.SaveAs(rows, true, "成绩数据");
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "成绩数据_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
        }

        private static string GetAuditStatusName(int? status)
        {
            switch (status)
            {
                case 1:
                    return "已通过";
                case 2:
                    return "已退回";
                default:
                    return "待审核";
            }
        }

        #endregion
    }
}
