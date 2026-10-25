using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.InstructionalManagement;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 作业管理（教师发布、学生提交、教师批改）
    /// </summary>
    [Area("InstructionalManagement")]
    public class HomeworkController : BaseController
    {
        public HomeworkService _service { get; set; }

        #region 视图

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 我的作业（学生端）
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult My()
        {
            return View();
        }

        /// <summary>
        /// 发布/编辑作业
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Form()
        {
            return View();
        }

        /// <summary>
        /// 提交情况（已提交/未提交名单）
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Review()
        {
            return View();
        }

        /// <summary>
        /// 在线批改
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Grade()
        {
            return View();
        }

        /// <summary>
        /// 提交作业（学生端）
        /// </summary>
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Submit()
        {
            return View();
        }

        #endregion

        #region 查询

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<HomeworkExtend> pagination, string keyword, string courseId, string classId, string status)
        {
            var data = await _service.GetLookList(pagination, keyword, courseId, classId, status);
            return Content(pagination.setData(data).ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetMyGridJson(SoulPage<HomeworkExtend> pagination, string keyword, string status)
        {
            var data = await _service.GetMyList(pagination, keyword, status);
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
        /// 作业详情 + 本人提交信息（学生端）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetHomeworkDetailJson(string keyValue)
        {
            var data = await _service.GetHomeworkDetail(keyValue);
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

        /// <summary>
        /// 提交名单（status: 空=已提交，0=未提交，2=待批改，3=已批改）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSubmitListJson(SoulPage<HomeworkSubmitExtend> pagination, string homeworkId, string status)
        {
            var data = await _service.GetSubmitList(pagination, homeworkId, status);
            return Content(pagination.setData(data).ToJson());
        }

        /// <summary>
        /// 批改页数据（作业 + 学生作答）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetSubmitJson(string keyValue)
        {
            var data = await _service.GetSubmitDetail(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(HomeworkEntity entity, string keyValue)
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
        [HandlerAuthorize]
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

        /// <summary>
        /// 发布/取消发布（status=1发布，0取消）
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetStatus(string keyValue, int status)
        {
            try
            {
                await _service.SetStatus(keyValue, status);
                return await Success(status == 1 ? "已发布。" : "已取消发布。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        /// <summary>
        /// 学生提交作业（在线作答 + 附件）
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAnswer(string homeworkId, string content, string fileUrl, string fileName, long fileSize)
        {
            try
            {
                await _service.SaveAnswer(homeworkId, content, fileUrl, fileName, fileSize);
                return await Success("作业提交成功。", "", homeworkId);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", homeworkId);
            }
        }

        /// <summary>
        /// 教师批改作业
        /// </summary>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GradeForm(string keyValue, decimal score, string comment)
        {
            try
            {
                await _service.GradeForm(keyValue, score, comment);
                return await Success("批改完成。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion

        #region 上传

        /// <summary>
        /// 作业附件上传（支持图片、文档，最大50MB）
        /// </summary>
        [HttpPost]
        [HandlerLogin]
        [RequestSizeLimit(52428800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> UploadFile()
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    throw new Exception("请选择文件");
                }
                string[] allowExt = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".heif", ".zip", ".rar" };
                var file = files[0];
                var originalName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var extIndex = originalName.LastIndexOf(".", StringComparison.Ordinal);
                if (extIndex < 0)
                {
                    throw new Exception("文件缺少扩展名");
                }
                var ext = originalName.Substring(extIndex).ToLower();
                if (!allowExt.Contains(ext))
                {
                    throw new Exception("不支持的文件格式：" + ext);
                }
                string fileName = Utils.CreateNo() + ext;
                string dir = GlobalContext.HostingEnvironment.WebRootPath + $@"/file/homework/" + DateTime.Now.ToString("yyyyMMdd") + $@"/";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string filePath = $@"/file/homework/" + DateTime.Now.ToString("yyyyMMdd") + $@"/" + fileName;
                using (FileStream fs = System.IO.File.Create(dir + fileName))
                {
                    await file.CopyToAsync(fs);
                }
                return Content(new { code = 0, msg = "", data = new { url = filePath, name = originalName, size = file.Length } }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new { code = 400, msg = "上传失败," + ex.Message, data = (object)null }.ToJson());
            }
        }

        #endregion
    }
}
