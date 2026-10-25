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
    /// 课程管理
    /// </summary>
    [Area("InstructionalManagement")]
    public class CourseController : BaseController
    {
        public CourseService _service { get; set; }

        #region 视图

        [HttpGet]
        public ActionResult Chapter()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Ware()
        {
            return View();
        }

        [HttpGet]
        public new ActionResult Details()
        {
            return View();
        }

        #endregion

        #region 列表

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<CourseExtend> pagination, string keyword, string categoryId, string status)
        {
            var data = await _service.GetLookList(pagination, keyword, categoryId, status);
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
        public async Task<ActionResult> GetTeacherListJson()
        {
            var data = await _service.GetTeacherList();
            return Content(data.Select(a => new { a.F_Id, a.F_RealName, a.F_Account }).ToList().ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(CourseEntity entity, string keyValue)
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
                return await Success(status == 1 ? "已上架。" : "已下架。", "", keyValue);
            }
            catch (Exception ex)
            {
                return await Error(ex.Message, "", keyValue);
            }
        }

        #endregion

        #region 上传

        /// <summary>
        /// 课程文件上传（视频/课件/文档/图片），最大500MB
        /// </summary>
        [HttpPost]
        [HandlerLogin]
        [RequestSizeLimit(524288000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
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
                string[] allowExt = { ".mp4", ".webm", ".mov", ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
                long size = files.Sum(f => f.Length);
                var list = new System.Collections.Generic.List<object>();
                foreach (var file in files)
                {
                    var originalName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var ext = originalName.Substring(originalName.LastIndexOf(".")).ToLower();
                    if (!allowExt.Contains(ext))
                    {
                        throw new Exception("不支持的文件格式：" + ext);
                    }
                    string fileName = Utils.CreateNo() + ext;
                    string dir = GlobalContext.HostingEnvironment.WebRootPath + $@"/file/course/" + DateTime.Now.ToString("yyyyMMdd") + $@"/";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string filePath = $@"/file/course/" + DateTime.Now.ToString("yyyyMMdd") + $@"/" + fileName;
                    using (FileStream fs = System.IO.File.Create(dir + fileName))
                    {
                        await file.CopyToAsync(fs);
                    }
                    list.Add(new { src = filePath, title = originalName, size = file.Length });
                }
                return Content(new { code = 0, msg = "上传成功", data = list }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new { code = 400, msg = "上传失败," + ex.Message }.ToJson());
            }
        }

        #endregion
    }
}
