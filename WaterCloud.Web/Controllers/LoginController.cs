/*******************************************************************************
 * Copyright © 2020 WaterCloud.Framework 版权所有
 * Author: WaterCloud
 * Description: WaterCloud快速开发平台
 * Website：
*********************************************************************************/

using Jaina.EventBus;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Domain.SystemSecurity;
using WaterCloud.Service.Event;
using WaterCloud.Service.SystemOrganize;
using WaterCloud.Service.SystemSecurity;

namespace WaterCloud.Web.Controllers
{
	public class LoginController : Controller
	{
		/// <summary>
		/// 拼图位置允许的误差（像素）
		/// </summary>
		private const int SliderTolerance = 8;

		public UserService _userService { get; set; }
		public LogService _logService { get; set; }
		public SystemSetService _setService { get; set; }
		public RoleAuthorizeService _roleAuthServuce { get; set; }
		public ISqlSugarClient _context { get; set; }

		[HttpGet]
		public virtual async Task<ActionResult> Index()
		{
			//登录页获取logo和项目名称
			try
			{
				var systemset = await _setService.GetFormByHost("");
				//if (GlobalContext.SystemConfig.Demo)
				//{
					ViewBag.UserName = systemset.F_AdminAccount;
					ViewBag.Password = systemset.F_AdminPassword;
				//}
				ViewBag.SqlMode = GlobalContext.SystemConfig.SqlMode;
				ViewBag.ProjectName = systemset.F_ProjectName;
				ViewBag.LogoIcon = ".." + systemset.F_Logo;
				return View();
			}
			catch (Exception)
			{
				ViewBag.ProjectName = "教学管理系统";
				ViewBag.LogoIcon = "../icon/favicon.ico";
				return View();
			}
		}

		[HttpGet]
		[HandlerAjaxOnly]
		public async Task<ActionResult> GetListJsonByLogin(string keyword)
		{
			var data = await _setService.GetList(keyword);
			data = data.OrderBy(a => a.F_DbNumber).ToList();
			foreach (var item in data)
			{
				item.F_AdminAccount = null;
				item.F_AdminPassword = null;
				item.F_DBProvider = null;
				item.F_DbString = null;
				item.F_PrincipalMan = null;
				item.F_MobilePhone = null;
				item.F_CompanyName = null;
				item.F_LogoCode = null;
			}
			return Content(data.ToJson());
		}

		/// <summary>
		/// 验证码获取（此接口已弃用）
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult GetAuthCode()
		{
			return File(new VerifyCodeHelper().GetVerifyCode(), @"image/Gif");
		}

		[HttpGet]
		public async Task<ActionResult> OutLogin()
		{
			var logEntity = new LogEntity
			{
				F_ModuleName = "系统登录",
				F_Type = DbLogType.Exit.ToString(),
				F_Account = _setService.currentuser.UserCode,
				F_NickName = _setService.currentuser.UserName,
				F_Result = true,
				F_Description = "安全退出系统",
			};
			await GlobalContext.GetService<IEventPublisher>().PublishAsync(new BaseEventSource("Log:create", logEntity, _userService.currentuser));
			await OperatorProvider.Provider.EmptyCurrent("pc_");
			return Content(new AlwaysResult { state = ResultType.success.ToString() }.ToJson());
		}

		/// <summary>
		/// 验证登录状态请求接口
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[HandlerAjaxOnly]
		[IgnoreAntiforgeryToken]
		public async Task<ActionResult> CheckLoginState()
		{
			try
			{
				if (_setService.currentuser.UserId == null)
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString() }.ToJson());
				}
				//登录检测
				if ((await OperatorProvider.Provider.IsOnLine("pc_")).stateCode <= 0)
				{
					await OperatorProvider.Provider.EmptyCurrent("pc_");
					return Content(new AlwaysResult { state = ResultType.error.ToString() }.ToJson());
				}
				else
				{
					//验证回退路由是否有权限，没有就删除
					await CheckReturnUrl(_setService.currentuser.UserId);
					return Content(new AlwaysResult { state = ResultType.success.ToString() }.ToJson());
				}
			}
			catch (Exception)
			{
				return Content(new AlwaysResult { state = ResultType.error.ToString() }.ToJson());
			}
		}

		/// <summary>
		/// 获取滑块拼图验证码
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[HandlerAjaxOnly]
		public ActionResult GetSliderCaptcha()
		{
			try
			{
				SliderCaptchaResult captcha = new SliderVerifyHelper().Create(GlobalContext.HostingEnvironment.WebRootPath);
				string token = Guid.NewGuid().ToString("N");
				// 拼图目标位置只保存在服务端，有效期5分钟
				CacheHelper.SetBySecond("slider_captcha_" + token, captcha.PieceOffsetX.ToString(), 300);
				return Content(new
				{
					state = ResultType.success.ToString(),
					token = token,
					bgImage = "data:image/png;base64," + captcha.BackgroundImage,
					pieceImage = "data:image/png;base64," + captcha.PieceImage,
					pieceY = captcha.PieceY,
					pieceBoxSize = captcha.PieceBoxSize,
					width = captcha.Width,
					height = captcha.Height
				}.ToJson());
			}
			catch (Exception)
			{
				return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "验证码加载失败，请刷新重试" }.ToJson());
			}
		}

		/// <summary>
		/// 验证滑块拼图位置
		/// </summary>
		/// <param name="token">验证码token</param>
		/// <param name="sliderX">拼图拖动后的横向位置</param>
		/// <returns></returns>
		[HttpPost]
		[HandlerAjaxOnly]
		[IgnoreAntiforgeryToken]
		public ActionResult VerifySlider(string token, int sliderX)
		{
			try
			{
				if (string.IsNullOrEmpty(token))
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "验证失败，请刷新验证码" }.ToJson());
				}
				string cacheKey = "slider_captcha_" + token;
				string targetX = CacheHelper.Get<string>(cacheKey);
				// 每个验证码只能使用一次
				CacheHelper.Remove(cacheKey);
				if (string.IsNullOrEmpty(targetX))
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "验证码已过期，请刷新重试" }.ToJson());
				}
				if (!int.TryParse(targetX, out int target) || Math.Abs(sliderX - target) > SliderTolerance)
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "拼图未对齐，验证失败" }.ToJson());
				}
				string verifyToken = Guid.NewGuid().ToString("N");
				// 验证通过凭证，登录时校验
				CacheHelper.SetBySecond("slider_verify_" + verifyToken, verifyToken, 300);
				return Content(new { state = ResultType.success.ToString(), message = "验证通过", verifyToken = verifyToken }.ToJson());
			}
			catch (Exception)
			{
				return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "验证失败，请刷新重试" }.ToJson());
			}
		}

		/// <summary>
		/// 登录验证
		/// </summary>
		/// <param name="username">用户</param>
		/// <param name="password">密码</param>
		/// <param name="localurl">域名</param>
		/// <param name="sliderToken">滑块验证token</param>
		/// <returns></returns>
		[HttpPost]
		[HandlerAjaxOnly]
		[IgnoreAntiforgeryToken]
		public async Task<ActionResult> CheckLogin(string username, string password, string localurl, string sliderToken)
		{
			//根据域名判断租户
			LogEntity logEntity = new LogEntity();
			logEntity.F_ModuleName = "系统登录";
			logEntity.F_Type = DbLogType.Login.ToString();
			if (GlobalContext.SystemConfig.SqlMode == Define.SQL_MORE)
			{
				localurl = "";
			}
			try
			{
				if (string.IsNullOrEmpty(sliderToken))
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "请先完成滑块验证" }.ToJson());
				}
				
				var cacheToken = CacheHelper.Get<string>("slider_verify_" + sliderToken);
				if (cacheToken == null)
				{
					return Content(new AlwaysResult { state = ResultType.error.ToString(), message = "滑块验证已过期，请重新验证" }.ToJson());
				}
				
				CacheHelper.Remove("slider_verify_" + sliderToken);
				
				UserEntity userEntity = await _userService.CheckLogin(username, password, localurl);
				OperatorModel operatorModel = new OperatorModel();
				operatorModel.UserId = userEntity.F_Id;
				operatorModel.UserCode = userEntity.F_Account;
				operatorModel.UserName = userEntity.F_RealName;
				operatorModel.CompanyId = userEntity.F_CompanyId;
				operatorModel.OrganizeId = userEntity.F_OrganizeId;
				operatorModel.RoleId = userEntity.F_RoleId;
				operatorModel.LoginIPAddress = WebHelper.Ip;
				if (GlobalContext.SystemConfig.LocalLAN != false)
				{
					operatorModel.LoginIPAddressName = "本地局域网";
				}
				else
				{
					operatorModel.LoginIPAddressName = WebHelper.GetIpLocation(operatorModel.LoginIPAddress);
				}
				operatorModel.LoginTime = DateTime.Now;
				operatorModel.DdUserId = userEntity.F_DingTalkUserId;
				operatorModel.WxOpenId = userEntity.F_WxOpenId;
				//各租户的管理员也是当前数据库的全部权限
				operatorModel.IsSuperAdmin = userEntity.F_IsAdmin ?? false;
				operatorModel.IsAdmin = userEntity.F_IsAdmin ?? false;
				operatorModel.IsBoss = userEntity.F_IsBoss ?? false;
				operatorModel.IsLeaderInDepts = userEntity.F_IsLeaderInDepts ?? false;
				operatorModel.IsSenior = userEntity.F_IsSenior ?? false;
				SystemSetEntity setEntity = await _setService.GetForm(userEntity.F_CompanyId);
				operatorModel.DbNumber = setEntity.F_DbNumber;
				if (operatorModel.IsAdmin && operatorModel.DbNumber == GlobalContext.SystemConfig.MainDbNumber)
				{
					operatorModel.IsSuperAdmin = true;
				}
				else
				{
					operatorModel.IsSuperAdmin = false;
				}
				//缓存保存用户信息
				await OperatorProvider.Provider.AddLoginUser(operatorModel, "", "pc_");
				//防重复token
				string token = Utils.GuId();
				HttpContext.Response.Cookies.Append("pc_" + GlobalContext.SystemConfig.TokenName, token);
				await CacheHelper.SetAsync("pc_" + GlobalContext.SystemConfig.TokenName + "_" + operatorModel.UserId + "_" + operatorModel.LoginTime, token, GlobalContext.SystemConfig.LoginExpire, true);
				logEntity.F_Account = userEntity.F_Account;
				logEntity.F_NickName = userEntity.F_RealName;
				logEntity.F_Result = true;
				logEntity.F_Description = "登录成功";
				await GlobalContext.GetService<IEventPublisher>().PublishAsync(new BaseEventSource("Log:create", logEntity, operatorModel));
				//验证回退路由是否有权限，没有就删除
				await CheckReturnUrl(operatorModel.UserId);
				return Content(new AlwaysResult { state = ResultType.success.ToString(), message = "登录成功。" }.ToJson());
			}
			catch (Exception ex)
			{
				//记录完整异常堆栈，便于定位登录失败原因
				LogHelper.WriteWithTime(ex);
				logEntity.F_Account = username;
				logEntity.F_NickName = username;
				logEntity.F_Result = false;
				logEntity.F_Description = "登录失败，" + ex.Message;
				await GlobalContext.GetService<IEventPublisher>().PublishAsync(new BaseEventSource("Log:create", logEntity));
				return Content(new AlwaysResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
			}
		}

		private async Task CheckReturnUrl(string userId)
		{
			var realurl = WebHelper.GetCookie("wc_realreturnurl");
			var url = WebHelper.GetCookie("wc_returnurl");
			if (!string.IsNullOrEmpty(realurl) && !await _roleAuthServuce.CheckReturnUrl(userId, realurl))
			{
				WebHelper.RemoveCookie("wc_realreturnurl");
			}
			if (!string.IsNullOrEmpty(url) && !await _roleAuthServuce.CheckReturnUrl(userId, url))
			{
				WebHelper.RemoveCookie("wc_returnurl");
			}
		}
	}
}