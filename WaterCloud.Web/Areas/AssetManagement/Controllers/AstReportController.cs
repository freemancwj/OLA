using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Service.AssetManagement;

namespace WaterCloud.Web.Areas.AssetManagement.Controllers
{
    /// <summary>资产统计报表中心</summary>
    [Area("AssetManagement")]
    public class AstReportController : BaseController
    {
        public AstReportService _service { get; set; }

        [HttpGet, HandlerAuthorize]
        public override ActionResult Index() => View();

        [HttpGet, HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(string type, string keyword, string categoryId, string status, string period)
        {
            object data;
            switch (type)
            {
                case "category": data = await _service.GetCategorySummary(); break;
                case "damage": data = await _service.GetClassDamageSummary(); break;
                case "idle": data = await _service.GetIdleAssets(); break;
                case "depreciation": data = await _service.GetDepreciationLedger(period); break;
                default: data = await _service.GetAssetLedger(keyword, categoryId, status); break;
            }
            return Content(new { rows = data }.ToJson());
        }

        /// <summary>导出 Excel</summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string type, string keyword, string categoryId, string status, string period)
        {
            using var ms = new MemoryStream();
            string fileName;
            switch (type)
            {
                case "category":
                    var catRows = (await _service.GetCategorySummary()) as System.Collections.IEnumerable;
                    ms.SaveAs(catRows, true, "分类汇总");
                    fileName = "资产分类汇总";
                    break;
                case "damage":
                    var dmgRows = (await _service.GetClassDamageSummary()) as System.Collections.IEnumerable;
                    ms.SaveAs(dmgRows, true, "班级损坏统计");
                    fileName = "各班公物损坏统计";
                    break;
                case "idle":
                    var idleRows = (await _service.GetIdleAssets()).Select(a => new
                    {
                        资产编号 = a.F_AssetCode,
                        资产名称 = a.F_AssetName,
                        规格型号 = a.F_SpecModel,
                        购置日期 = a.F_PurchaseDate,
                        原值 = a.F_PurchasePrice,
                        净值 = a.F_NetValue,
                        存放位置 = a.F_Location
                    }).ToList();
                    ms.SaveAs(idleRows, true, "闲置资产");
                    fileName = "闲置资产统计";
                    break;
                case "depreciation":
                    var depRows = (await _service.GetDepreciationLedger(period)).Select(a => new
                    {
                        期间 = a.F_Period,
                        资产Id = a.F_AssetId,
                        月折旧 = a.F_MonthlyDepreciation,
                        累计折旧 = a.F_AccumulatedDepreciation,
                        净值 = a.F_NetValue
                    }).ToList();
                    ms.SaveAs(depRows, true, "折旧台账");
                    fileName = "资产折旧台账_" + (period ?? "全部");
                    break;
                default:
                    var rows = (await _service.GetAssetLedger(keyword, categoryId, status)).Select(a => new
                    {
                        资产编号 = a.F_AssetCode,
                        资产名称 = a.F_AssetName,
                        规格型号 = a.F_SpecModel,
                        序列号 = a.F_SerialNumber,
                        购置日期 = a.F_PurchaseDate,
                        购置来源 = a.F_PurchaseSource,
                        原值 = a.F_PurchasePrice,
                        净值 = a.F_NetValue,
                        状态 = a.F_Status,
                        存放位置 = a.F_Location,
                        损坏次数 = a.F_DamageCount
                    }).ToList();
                    ms.SaveAs(rows, true, "资产总台账");
                    fileName = "全校资产总台账";
                    break;
            }
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + "_" + DateTime.Now.ToString("HHmmss") + ".xlsx");
        }
    }
}
