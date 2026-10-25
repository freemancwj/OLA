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
    /// 在线考试管理
    /// </summary>
    [Area("InstructionalManagement")]
    public class ExamController : BaseController
    {
        public ExamService _service { get; set; }

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

        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Record()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Grade()
        {
            return View();
        }

        /// <summary>
        /// 学生端：我的考试（视图由学生端负责）
        /// </summary>
        [HttpGet]
        public ActionResult My()
        {
            return View();
        }

        /// <summary>
        /// 学生端：在线答题（视图由学生端负责）
        /// </summary>
        [HttpGet]
        public ActionResult Take()
        {
            return View();
        }

        /// <summary>
        /// 学生端：成绩详情（视图由学生端负责）
        /// </summary>
        [HttpGet]
        public ActionResult Result()
        {
            return View();
        }

        /// <summary>
        /// 学生端：错题记录（视图由学生端负责）
        /// </summary>
        [HttpGet]
        public ActionResult Wrong()
        {
            return View();
        }

        #endregion

        #region 考试列表

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ExamExtend> pagination, string keyword, string courseId, string classId, string type, string status)
        {
            var data = await _service.GetLookList(pagination, keyword, courseId, classId, type, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetMyGridJson(SoulPage<ExamExtend> pagination, string keyword, string type, int limit = 0)
        {
            //学生端视图使用 limit 作为分页大小参数名，这里做一次兼容
            if (limit > 0)
            {
                pagination.rows = limit;
            }
            var data = await _service.GetMyExamList(pagination, keyword, type);
            //学生端表格同时兼容 code/msg 与 state/message 两种解析方式
            return Content(new
            {
                state = pagination.state,
                message = pagination.message,
                code = pagination.state,
                msg = pagination.message,
                count = pagination.count,
                data = data
            }.ToJson());
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
        public async Task<ActionResult> GetQuestionListJson(string courseId, string type, string keyword)
        {
            var data = await _service.GetQuestionList(courseId, type, keyword);
            return Content(data.ToJson());
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
        public async Task<ActionResult> GetClassListJson()
        {
            var data = await _service.GetClassList();
            return Content(data.Select(a => new { a.F_Id, a.F_FullName }).ToList().ToJson());
        }

        #endregion

        #region 成绩与批改

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetRecordListJson(SoulPage<ExamRecordExtend> pagination, string examId, string keyword, string status)
        {
            var data = await _service.GetRecordList(pagination, examId, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetRecordFormJson(string keyValue)
        {
            var data = await _service.GetRecordForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetResultJson(string recordId)
        {
            try
            {
                var data = await _service.GetResult(recordId);
                var answers = data.Answers;
                data.Answers = null;
                return Content(new
                {
                    state = "success",
                    msg = "",
                    data = new { record = data, answers = answers }
                }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new { state = "error", msg = ex.Message, data = (object)null }.ToJson());
            }
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetWrongListJson(SoulPage<WrongQuestionExtend> pagination, string keyword, string courseId, int limit = 0)
        {
            //学生端视图使用 limit 作为分页大小参数名，这里做一次兼容
            if (limit > 0)
            {
                pagination.rows = limit;
            }
            var data = await _service.GetWrongList(pagination, keyword, courseId);
            //学生端表格同时兼容 code/msg 与 state/message 两种解析方式
            return Content(new
            {
                state = pagination.state,
                message = pagination.message,
                code = pagination.state,
                msg = pagination.message,
                count = pagination.count,
                data = data
            }.ToJson());
        }

        #endregion

        #region 学生答题

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> StartExamJson(string examId)
        {
            try
            {
                var data = await _service.StartExam(examId);
                return Content(new { state = "success", msg = "", data = data }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new { state = "error", msg = ex.Message, data = (object)null }.ToJson());
            }
        }

        /// <summary>
        /// 学生交卷（学生答题页使用 common.ajax 直接 POST，未携带防伪令牌请求头）
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> SubmitExam(string recordId, string answersJson)
        {
            try
            {
                await _service.SubmitExam(recordId, answersJson);
                return await Success("交卷成功。", "", recordId);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", recordId);
            }
        }

        #endregion

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ExamFormModel exam, string questionsJson)
        {
            try
            {
                await _service.SubmitForm(exam, questionsJson);
                return await Success("操作成功。", "", exam?.F_Id);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", exam?.F_Id);
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
                return await Success("删除成功。", "", keyValue, DbLogType.Delete);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue, DbLogType.Delete);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetStatus(string keyValue, int status)
        {
            try
            {
                await _service.SetStatus(keyValue, status);
                var message = status == 1 ? "已发布。" : (status == 2 ? "已结束。" : "已转为草稿。");
                return await Success(message, "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GradeForm(string recordId, string answersJson)
        {
            try
            {
                await _service.GradeRecord(recordId, answersJson);
                return await Success("批改完成。", "", recordId);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", recordId);
            }
        }

        #endregion
    }
}
