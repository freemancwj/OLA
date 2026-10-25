using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Service.InstructionalManagement;

namespace WaterCloud.Web.Areas.InstructionalManagement.Controllers
{
    /// <summary>
    /// 课程资料管理
    /// </summary>
    [Area("InstructionalManagement")]
    public class CourseWareController : BaseController
    {
        public CourseWareService _service { get; set; }

        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(string courseId)
        {
            var data = await _service.GetList(courseId);
            return Success(data.Count, data);
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitForm(WaterCloud.Domain.InstructionalManagement.CourseWareEntity entity, string keyValue)
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

        /// <summary>
        /// 资料下载（附件方式）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(string keyValue)
        {
            var entity = await _service.GetForm(keyValue);
            if (entity == null || string.IsNullOrEmpty(entity.F_FileUrl))
            {
                return Content("文件不存在");
            }
            var path = GlobalContext.HostingEnvironment.WebRootPath + entity.F_FileUrl.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (!System.IO.File.Exists(path))
            {
                return Content("文件已被删除");
            }
            await _service.AddDownCount(keyValue);
            var fileName = entity.F_Name + entity.F_FileExt;
            return File(System.IO.File.ReadAllBytes(path), "application/octet-stream", fileName);
        }
    }
}
