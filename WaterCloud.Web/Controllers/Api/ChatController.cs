/*******************************************************************************
 * Copyright © 2020 WaterCloud.Framework 版权所有
 * Author: WaterCloud
 * Description: WaterCloud快速开发平台
 * Website：
*********************************************************************************/

using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using WaterCloud.Code;

namespace WaterCloud.Web
{
    /// <summary>
    /// 在线聊天（LayIM）集成接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        /// <summary>
        /// 获取当前登录用户的聊天系统免登录地址（SSO）
        /// </summary>
        [HttpGet]
        public IActionResult SsoUrl()
        {
            string baseUrl = (GlobalContext.Configuration["Chat:BaseUrl"] ?? "").TrimEnd('/');
            string ssoSecret = GlobalContext.Configuration["Chat:SsoSecret"];
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(ssoSecret))
            {
                return Ok(new { url = "", msg = "聊天系统未配置" });
            }

            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            // 聊天账号使用登录账户（唯一），UserName 为姓名，可能重名
            string username = (string.IsNullOrEmpty(user?.UserCode) ? user?.UserName : user.UserCode)?.Trim() ?? "";
            if (string.IsNullOrEmpty(username))
            {
                return Ok(new { url = "", msg = "未登录" });
            }
            // 聊天系统要求用户名长度为 2-20 个字符
            if (username.Length < 2 || username.Length > 20)
            {
                return Ok(new { url = "", msg = "当前账号名长度不符合聊天系统要求（2-20个字符）" });
            }

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string message = $"{username}|{timestamp}";
            string sign;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(ssoSecret)))
            {
                sign = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message))).ToLowerInvariant();
            }
            string ticket = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{message}|{sign}"))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            return Ok(new { url = $"{baseUrl}/api/auth/sso?ticket={Uri.EscapeDataString(ticket)}" });
        }
    }
}
