using LayimChat.Data;
using LayimChat.Hubs;
using LayimChat.Models;
using LayimChat.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Controllers;

/// <summary>
/// 第三方系统（如公司管理系统）集成接口。
/// 全部接口需在请求头携带 X-Api-Key（与配置 Integration:ApiKey 一致）。
/// 注意：用户与群组直接读主系统 sys_user / sys_organize，聊天系统不再开户、建组。
/// </summary>
[ApiController]
[Route("api/integration")]
[ApiKey]
public class IntegrationController : ControllerBase
{
    private readonly LayimDbContext _db;
    private readonly IHubContext<ChatHub> _hub;

    public IntegrationController(LayimDbContext db, IHubContext<ChatHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    /// <summary>用户列表（主系统 sys_user）</summary>
    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .Where(u => u.Deleted != 1)
            .OrderBy(u => u.Username)
            .Select(u => new { u.Id, u.Username, u.Sign, u.Avatar })
            .ToListAsync();
        var data = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Sign,
            u.Avatar,
            Status = ChatHub.IsOnline(u.Id) ? "online" : "offline"
        });
        return Ok(new { code = 0, msg = "", data });
    }

    public record SyncUserRequest(string Username, string? Password, string? Sign, string? Avatar, string[]? Groups);

    /// <summary>
    /// 更新用户聊天资料（签名/头像）。用户由主系统 sys_user 创建，此处不再开户；
    /// 群组成员由组织架构（F_OrganizeId）推导，Groups 参数已忽略。
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> SyncUser([FromBody] SyncUserRequest req)
    {
        var username = (req.Username ?? "").Trim();
        if (username.Length == 0)
        {
            return Ok(new { code = 1, msg = "用户名不能为空" });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.Deleted != 1);
        if (user == null)
        {
            return Ok(new { code = 1, msg = $"用户不存在：{username}，请先在管理系统中创建账号" });
        }

        if (req.Sign != null) user.Sign = req.Sign;
        if (req.Avatar != null) user.Avatar = req.Avatar;
        await _db.SaveChangesAsync();

        return Ok(new { code = 0, msg = "", data = new { id = user.Id, username = user.Username, created = false } });
    }

    /// <summary>删除用户：仅清理该用户的聊天数据（好友关系、群成员、消息），不删除主系统 sys_user</summary>
    [HttpDelete("users/{user}")]
    public async Task<IActionResult> DeleteUser(string user)
    {
        var target = await UserDirectory.FindByAccountAsync(_db, user)
            ?? await UserDirectory.FindAsync(_db, user);
        if (target == null)
        {
            return Ok(new { code = 1, msg = $"用户不存在：{user}" });
        }

        await _db.Friendships
            .Where(f => f.OwnerId == target.Id || f.FriendId == target.Id)
            .ExecuteDeleteAsync();
        await _db.GroupMembers.Where(m => m.UserId == target.Id).ExecuteDeleteAsync();
        await _db.Messages
            .Where(m => m.FromUserId == target.Id || (m.ToType == "friend" && m.ToId == target.Id))
            .ExecuteDeleteAsync();
        await _db.UserStates.Where(s => s.UserId == target.Id).ExecuteDeleteAsync();

        return Ok(new { code = 0, msg = "已清理该用户的聊天数据（主系统用户未删除）" });
    }

    // ==================== 群组（即主系统组织机构 sys_organize） ====================

    /// <summary>群组列表（含成员），供办公系统对账</summary>
    [HttpGet("groups")]
    public async Task<IActionResult> Groups()
    {
        var groups = await _db.ChatGroups
            .Where(g => g.Deleted != 1 && g.Enabled != 0)
            .OrderBy(g => g.GroupName)
            .ToListAsync();
        var members = await _db.GroupMembers.ToListAsync();
        var nameMap = await _db.Users
            .Where(u => u.Deleted != 1)
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        var data = groups.Select(g => new
        {
            id = g.Id,
            name = g.GroupName,
            ownerId = g.OwnerId,
            members = members
                .Where(m => m.GroupId == g.Id && nameMap.ContainsKey(m.UserId))
                .Select(m => new { id = m.UserId, username = nameMap[m.UserId] })
                .ToList()
        });

        return Ok(new { code = 0, msg = "", data });
    }

    public record SyncGroupRequest(string? Id, string? Name);

    /// <summary>群组即组织架构，由主系统维护；此处不再创建/改名</summary>
    [HttpPost("groups")]
    public IActionResult SyncGroup([FromBody] SyncGroupRequest req)
        => Ok(new { code = 1, msg = "群组即主系统组织架构，请在管理系统中维护" });

    /// <summary>群组即组织架构，由主系统维护；此处不再删除</summary>
    [HttpDelete("groups/{id}")]
    public IActionResult DeleteGroup(string id)
        => Ok(new { code = 1, msg = "群组即主系统组织架构，请在管理系统中维护" });

    public record MemberRequest(string? User);

    /// <summary>群成员由用户所属部门（sys_user.F_OrganizeId）决定，改部门请在管理系统中操作</summary>
    [HttpPost("groups/{id}/members")]
    public IActionResult AddMember(string id, [FromBody] MemberRequest req)
        => Ok(new { code = 1, msg = "群成员由用户所属部门决定，请在管理系统中调整用户部门" });

    /// <summary>群成员由用户所属部门决定，改部门请在管理系统中操作</summary>
    [HttpDelete("groups/{id}/members/{user}")]
    public IActionResult RemoveMember(string id, string user)
        => Ok(new { code = 1, msg = "群成员由用户所属部门决定，请在管理系统中调整用户部门" });

    public record SendMessageRequest(string FromUser, string ToType, string ToUser, string Content);

    /// <summary>
    /// 发送消息（系统通知 / 业务提醒推送）。
    /// FromUser / ToUser 支持传用户ID或用户名；ToType 为 friend 或 group。
    /// 消息会落库，且接收方在线时实时推送到聊天窗口。
    /// </summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
    {
        var toType = (req.ToType ?? "").Trim().ToLower();
        if (toType is not ("friend" or "group"))
        {
            return Ok(new { code = 1, msg = "ToType 必须是 friend 或 group" });
        }
        if (string.IsNullOrWhiteSpace(req.Content))
        {
            return Ok(new { code = 1, msg = "消息内容不能为空" });
        }

        var from = await ResolveUser(req.FromUser);
        if (from == null)
        {
            return Ok(new { code = 1, msg = $"发送者不存在：{req.FromUser}" });
        }

        string toId;
        if (toType == "group")
        {
            if (!await _db.ChatGroups.AnyAsync(g => g.Id == req.ToUser.Trim()))
            {
                return Ok(new { code = 1, msg = $"群组不存在：{req.ToUser}" });
            }
            toId = req.ToUser.Trim();
        }
        else
        {
            var to = await ResolveUser(req.ToUser);
            if (to == null)
            {
                return Ok(new { code = 1, msg = $"接收者不存在：{req.ToUser}" });
            }
            toId = to.Id;
        }

        var message = new Message
        {
            FromUserId = from.Id,
            ToType = toType,
            ToId = toId,
            Content = req.Content,
            CreatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        // 推送规则与 ChatHub 保持一致：好友消息 id=发送者ID，群消息 id=群ID
        var payload = new
        {
            username = from.Username,
            avatar = Avatars.OrDefault(from.Avatar),
            id = toType == "group" ? toId : from.Id,
            type = toType,
            content = req.Content,
            timestamp = new DateTimeOffset(DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds()
        };

        if (toType == "group")
        {
            await _hub.Clients.Group(ChatHub.GroupName(toId)).SendAsync("ReceiveMessage", payload);
        }
        else
        {
            await _hub.Clients.User(toId).SendAsync("ReceiveMessage", payload);
        }

        return Ok(new { code = 0, msg = "", data = new { id = message.Id } });
    }

    /// <summary>按ID或账号解析用户（主系统用户 + 客服/访客影子用户）</summary>
    private async Task<User?> ResolveUser(string? idOrName)
    {
        var key = (idOrName ?? "").Trim();
        if (key.Length == 0) return null;
        return await UserDirectory.FindByAccountAsync(_db, key)
            ?? await UserDirectory.FindAsync(_db, key);
    }
}
