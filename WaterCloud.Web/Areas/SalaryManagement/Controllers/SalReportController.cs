using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SalaryManagement;
using WaterCloud.Service.SalaryManagement;

namespace WaterCloud.Web.Areas.SalaryManagement.Controllers
{
    /// <summary>
    /// 薪酬报表中心（Excel 导出，对接财务/财政对账）
    /// </summary>
    [Area("SalaryManagement")]
    public class SalReportController : BaseController
    {
        public SalPayslipService _payslipService { get; set; }

        [HttpGet]
        [HandlerAuthorize]
        public override ActionResult Index()
        {
            return View();
        }

        /// <summary>报表预览数据</summary>
        [HttpGet]
        [HandlerAjaxOnly]
        public async Task<ActionResult> GetGridJson(SoulPage<SalPayslipExtend> pagination, string month)
        {
            if (string.IsNullOrEmpty(month)) month = DateTime.Now.ToString("yyyy-MM");
            var queryable = (await _payslipService.GetAllByMonth(month));
            pagination.count = queryable.Count;
            var data = queryable.Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows).ToList();
            return Content(pagination.setData(data).ToJson());
        }

        /// <summary>导出 Excel：summary总表/period课时费/headteacher班主任/afterschool课后服务/taxsocial个税社保</summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string type, string month)
        {
            if (string.IsNullOrEmpty(month)) month = DateTime.Now.ToString("yyyy-MM");
            using var ms = new MemoryStream();
            string fileName;
            switch (type)
            {
                case "period":
                    var periodRows = (await _payslipService.GetDetailsByMonthCategory(month, "课时")).Select(a => new
                    {
                        月份 = a.F_Month,
                        账号 = a.F_Account,
                        姓名 = a.F_RealName,
                        项目 = a.F_ItemName,
                        数量 = a.F_Quantity,
                        单价 = a.F_UnitPrice,
                        金额 = a.F_Amount
                    }).ToList();
                    ms.SaveAs(periodRows, true, "课时费明细");
                    fileName = "课时费发放明细_" + month;
                    break;
                case "headteacher":
                    var headRows = (await _payslipService.GetDetailsByMonthCategory(month, "班主任")).Select(a => new
                    {
                        月份 = a.F_Month,
                        账号 = a.F_Account,
                        姓名 = a.F_RealName,
                        项目 = a.F_ItemName,
                        金额 = a.F_Amount
                    }).ToList();
                    ms.SaveAs(headRows, true, "班主任津贴");
                    fileName = "班主任津贴台账_" + month;
                    break;
                case "afterschool":
                    var afterRows = (await _payslipService.GetDetailsByMonthCategory(month, "课后服务")).Select(a => new
                    {
                        月份 = a.F_Month,
                        账号 = a.F_Account,
                        姓名 = a.F_RealName,
                        项目 = a.F_ItemName,
                        数量 = a.F_Quantity,
                        单价 = a.F_UnitPrice,
                        金额 = a.F_Amount
                    }).ToList();
                    ms.SaveAs(afterRows, true, "课后服务费");
                    fileName = "课后服务费专项汇总_" + month;
                    break;
                case "taxsocial":
                    var taxRows = (await _payslipService.GetAllByMonth(month)).Select(a => new
                    {
                        月份 = a.F_Month,
                        账号 = a.F_Account,
                        姓名 = a.F_RealName,
                        应发工资 = a.F_GrossPay,
                        社保公积金个人 = a.F_SocialPersonal,
                        工会会费 = a.F_UnionFee,
                        个人所得税 = a.F_Tax,
                        实发工资 = a.F_NetPay
                    }).ToList();
                    ms.SaveAs(taxRows, true, "个税社保汇总");
                    fileName = "个税社保汇总_" + month;
                    break;
                default:
                    var rows = (await _payslipService.GetAllByMonth(month)).Select(a => new
                    {
                        月份 = a.F_Month,
                        账号 = a.F_Account,
                        姓名 = a.F_RealName,
                        固定部分 = a.F_FixedTotal,
                        课时费 = a.F_PeriodTotal,
                        班主任津贴 = a.F_HeadTeacherTotal,
                        课后服务费 = a.F_AfterSchoolTotal,
                        专项津贴 = a.F_SpecialTotal,
                        调整项 = a.F_AdjustmentTotal,
                        考勤扣款 = a.F_AttendanceDeduct,
                        社保公积金个人 = a.F_SocialPersonal,
                        工会会费 = a.F_UnionFee,
                        个人所得税 = a.F_Tax,
                        应发工资 = a.F_GrossPay,
                        实发工资 = a.F_NetPay,
                        状态 = StatusName(a.F_Status)
                    }).ToList();
                    ms.SaveAs(rows, true, "月度薪酬总表");
                    fileName = "全校月度薪酬总表_" + month;
                    break;
            }
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + "_" + DateTime.Now.ToString("HHmmss") + ".xlsx");
        }

        private static string StatusName(int? status)
        {
            switch (status)
            {
                case 1: return "人事已提交";
                case 2: return "教务已核对";
                case 3: return "财务已核对";
                case 4: return "已归档锁定";
                case 5: return "已驳回";
                default: return "草稿";
            }
        }
    }
}
