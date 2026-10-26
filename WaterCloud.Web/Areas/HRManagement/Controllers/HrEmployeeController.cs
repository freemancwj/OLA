using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.HRManagement;
using WaterCloud.Service.HRManagement;

namespace WaterCloud.Web.Areas.HRManagement.Controllers
{
    /// <summary>
    /// 员工档案管理
    /// </summary>
    [Area("HRManagement")]
    public class HrEmployeeController : BaseController
    {
        public HrEmployeeService _service { get; set; }

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

        #region 查询

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<HrEmployeeExtend> pagination, string keyword, string organizeId, string education)
        {
            var data = await _service.GetList(pagination, keyword, organizeId, education);
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
        /// 未建档用户列表（新增档案时选择）
        /// </summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetUnarchivedUserListJson()
        {
            var data = await _service.GetUnarchivedUsers();
            return Content(data.Select(a => new { F_Id = a.F_Id, F_Account = a.F_Account, F_RealName = a.F_RealName }).ToList().ToJson());
        }

        #endregion

        #region 提交

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(HrEmployeeEntity entity, string keyValue)
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

        #endregion

        #region 上传相片

        /// <summary>
        /// 员工相片上传
        /// </summary>
        [HttpPost]
        [HandlerLogin]
        [RequestSizeLimit(5242880)]
        [RequestFormLimits(MultipartBodyLengthLimit = 5242880)]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> UploadPhoto()
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files == null || files.Count == 0)
                {
                    throw new Exception("请选择文件");
                }
                string[] allowExt = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
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
                    throw new Exception("不支持的图片格式：" + ext);
                }
                string fileName = Utils.CreateNo() + ext;
                string dir = GlobalContext.HostingEnvironment.WebRootPath + $@"/file/employee/" + DateTime.Now.ToString("yyyyMMdd") + $@"/";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string filePath = $@"/file/employee/" + DateTime.Now.ToString("yyyyMMdd") + $@"/" + fileName;
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
