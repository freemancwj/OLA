using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.LogisticsManagement;
using WaterCloud.Service.LogisticsManagement;

namespace WaterCloud.Web.Areas.LogisticsManagement.Controllers
{
    /// <summary>
    /// 合同管理
    /// </summary>
    [Area("LogisticsManagement")]
    public class ContractController : BaseController
    {
        public ContractService _service { get; set; }

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

        #endregion

        #region 获取数据

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<ContractEntity> pagination, string keyword, string status)
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

        #endregion

        #region 提交数据

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(ContractEntity entity, string keyValue)
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

        #region 上传

        /// <summary>
        /// 合同附件上传（最大50MB）
        /// </summary>
        [HttpPost]
        [HandlerLogin]
        [RequestSizeLimit(52428800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        [IgnoreAntiforgeryToken]
        public ActionResult UploadFile()
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    throw new Exception("请选择文件");
                }
                string[] allowExt = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
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
                string dir = GlobalContext.HostingEnvironment.WebRootPath + $@"/file/contract/" + DateTime.Now.ToString("yyyyMMdd") + $@"/";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string filePath = $@"/file/contract/" + DateTime.Now.ToString("yyyyMMdd") + $@"/" + fileName;
                using (FileStream fs = System.IO.File.Create(dir + fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                return Content(new { code = 0, msg = "上传成功", data = new { url = filePath, name = originalName, size = file.Length } }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new { code = 400, msg = "上传失败," + ex.Message }.ToJson());
            }
        }

        #endregion
    }
}
