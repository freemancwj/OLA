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
    /// 成绩修改申请
    /// </summary>
    [Area("InstructionalManagement")]
    public class ScoreChangeController : BaseController
    {
        public ScoreChangeService _service { get; set; }

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
        public ActionResult AuditForm()
        {
            return View();
        }

        #endregion

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ScoreChangeExtend> pagination, string keyword, string status)
        {
            var data = await _service.GetLookList(pagination, keyword, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetFormJson(string keyValue)
        {
            var data = await _service.GetForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 可申请修改的成绩记录下拉
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetScoreListJson(string keyword)
        {
            var data = await _service.GetScoreList(keyword);
            return Content(data.Select(a => new
            {
                a.F_Id,
                Text = a.F_RealName + "(" + a.F_Account + ")-" + a.F_ClassName + "-" + a.F_CourseName + "-" + a.F_ScoreTypeName + "-" + a.F_Score,
                a.F_StudentId,
                a.F_CourseId,
                a.F_Score
            }).ToList().ToJson());
        }

        #endregion

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ScoreChangeEntity entity, string keyValue)
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
        public async Task<ActionResult> Audit(string keyValue, int status, string remark)
        {
            try
            {
                await _service.Audit(keyValue, status, remark);
                return await Success("审批成功。", "", keyValue);
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
