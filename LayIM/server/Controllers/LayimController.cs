using System.Security.Claims;
using LayimChat.Data;
using LayimChat.Hubs;
using LayimChat.Models;
using LayimChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Controllers;

[Authorize]
[ApiController]
[Route("api/layim")]
public class LayimController : ControllerBase
{
    private readonly LayimDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<ChatHub> _hub;

    public LayimController(LayimDbContext db, IWebHostEnvironment env, IHubContext<ChatHub> hub)
    {
        _db = db;
        _env = env;
        _hub = hub;
    }

    private string MyId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>「不属于任何群组的用户」占位分组名：前端据此隐藏分组标题，把这些用户平铺在群组下方</summary>
    public const string NoGroupName = "__nogroup__";

    /// <summary>把 yyyy-MM-dd 的本地日期转换为 UTC 边界。
    /// isEnd=true 时返回「次日 00:00（本地）」对应的 UTC，供 &lt; 比较实现整日包含；解析失败返回 null。</summary>
    private static DateTime? ParseLocalDate(string? text, bool isEnd)
    {
        if (string.IsNullOrWhiteSpace(text)) { return null; }
        if (!DateTime.TryParseExact(text.Trim(), "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var day))
        {
            return null;
        }
        if (isEnd) { day = day.AddDays(1); }
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(day, DateTimeKind.Unspecified), TimeZoneInfo.Local);
    }

    /// <summary>LayIM 初始化数据（好友列表 + 我的群组 + 我的资料）</summary>
    [HttpGet("init")]
    public async Task<IActionResult> Init()
    {
        var me = await UserDirectory.FindAsync(_db, MyId);
        if (me == null)
        {
            return Unauthorized();
        }

        // 网站访客（免登录临时用户）：通讯录只有固定客服一个联系人，没有任何群组
        if (ServiceAgent.IsGuestName(me.Username))
        {
            var agent = await _db.ShadowUsers.FindAsync(ServiceAgent.Id);
            if (agent == null)
            {
                return Ok(new { code = 1, msg = "客服账号不存在" });
            }
            return Ok(new
            {
                code = 0,
                msg = "",
                data = new
                {
                    mine = new
                    {
                        username = me.Username,
                        id = me.Id,
                        status = ChatHub.IsOnline(me.Id) ? "online" : "offline",
                        sign = me.Sign,
                        avatar = Avatars.OrDefault(me.Avatar),
                        admin = false,
                        guest = true // 前端据此进入访客模式（精简界面、自动打开客服会话）
                    },
                    friend = new[]
                    {
                        new
                        {
                            groupname = "在线客服",
                            id = "kefu",
                            list = new[]
                            {
                                new
                                {
                                    username = ServiceAgent.DisplayName,
                                    id = agent.Id,
                                    avatar = Avatars.OrDefault(agent.Avatar),
                                    sign = agent.Sign,
                                    status = "online" // 客服对访客恒在线
                                }
                            }
                        }
                    },
                    group = Array.Empty<object>()
                }
            });
        }

        // 网站客服：联系人是全部访客（最近有消息的排在前面），没有群组
        if (me.Id == ServiceAgent.Id)
        {
            var guests = await _db.ShadowUsers
                .Where(u => u.Username.StartsWith(ServiceAgent.GuestPrefix))
                .ToListAsync();

            // 按「最近一条消息时间」排序访客，方便客服优先处理新会话
            var recent = await _db.Messages
                .Where(m => m.ToType == "friend"
                    && (m.FromUserId == ServiceAgent.Id || m.ToId == ServiceAgent.Id))
                .Select(m => new { m.FromUserId, m.ToId, m.CreatedAt })
                .ToListAsync();
            var lastTimes = recent
                .Select(m => new
                {
                    Partner = m.FromUserId == ServiceAgent.Id ? m.ToId : m.FromUserId,
                    m.CreatedAt
                })
                .GroupBy(x => x.Partner)
                .ToDictionary(g => g.Key, g => g.Max(x => x.CreatedAt));

            var guestList = guests
                .OrderByDescending(u => lastTimes.TryGetValue(u.Id, out var t) ? t : DateTime.MinValue)
                .ThenBy(u => u.Username)
                .Select(u => new
                {
                    username = u.Username,
                    id = u.Id,
                    avatar = Avatars.OrDefault(u.Avatar),
                    sign = u.Sign,
                    status = ChatHub.IsOnline(u.Id) ? "online" : "offline"
                })
                .ToList();

            return Ok(new
            {
                code = 0,
                msg = "",
                data = new
                {
                    mine = new
                    {
                        username = me.Username,
                        id = me.Id,
                        status = ChatHub.IsOnline(me.Id) ? "online" : "offline",
                        sign = me.Sign,
                        avatar = Avatars.OrDefault(me.Avatar),
                        admin = false,
                        kefu = true // 前端据此进入客服工作台模式（无群组）
                    },
                    friend = new[]
                    {
                        new { groupname = "访客", id = "guests", list = guestList }
                    },
                    group = Array.Empty<object>()
                }
            });
        }

        // 群成员关系按主系统组织架构（sys_user.F_OrganizeId）实时对齐
        await GroupMembershipSync.SyncAsync(_db);

        // 通讯录按群组（= 主系统组织机构）组织：第一级为群组，展开显示该群组下的所有成员
        var allGroups = await _db.ChatGroups
            .Where(g => g.Deleted != 1 && g.Enabled != 0)
            .OrderBy(g => g.GroupName)
            .ToListAsync();
        var groupIds = allGroups.Select(g => g.Id).ToList();

        var memberships = await _db.GroupMembers
            .Where(m => groupIds.Contains(m.GroupId))
            .ToListAsync();

        // 一次性取所有成员用户信息，避免 N+1 查询
        var memberIds = memberships.Select(m => m.UserId).Distinct().ToList();
        var memberUsers = await _db.Users
            .Where(u => memberIds.Contains(u.Id) && u.Deleted != 1)
            .ToDictionaryAsync(u => u.Id);

        var memberGroupIds = memberships.Select(m => m.GroupId).ToHashSet();
        var myGroups = allGroups.Where(g => memberGroupIds.Contains(g.Id) && memberships.Any(m => m.GroupId == g.Id && m.UserId == MyId)).ToList();

        // 不属于任何群组的主系统用户：作为最后一个分组追加到联系人列表末尾
        // （前端隐藏该分组标题，直接平铺在所有群组下方）
        // 注意：sys_user 与 chat_group_member 的列排序规则不同，不能做列间比较，先取回内存再过滤
        var groupedUserIds = memberships.Select(m => m.UserId).Distinct().ToList();
        var noGroupUsers = (await _db.Users
                .Where(u => u.Deleted != 1)
                .ToListAsync())
            .Where(u => u.Id != MyId && !groupedUserIds.Contains(u.Id))
            .ToList();

        var friendGroups = allGroups.Where(g => memberGroupIds.Contains(g.Id)).Select(g => (object)new
        {
            groupname = g.GroupName ?? "",
            id = g.Id,
            list = memberships
                .Where(m => m.GroupId == g.Id && m.UserId != MyId && memberUsers.ContainsKey(m.UserId))
                .Select(m =>
                {
                    var u = memberUsers[m.UserId];
                    return new
                    {
                        username = u.Username,
                        id = u.Id,
                        avatar = Avatars.OrDefault(u.Avatar),
                        sign = u.Sign,
                        // online/offline：前端据此渲染在线标识，离线好友头像置灰
                        status = ChatHub.IsOnline(u.Id) ? "online" : "offline"
                    };
                })
                // 在线用户排在所有离线用户之前
                .OrderByDescending(m => m.status == "online")
                .ThenBy(m => m.username)
        }).ToList();

        if (noGroupUsers.Count > 0)
        {
            friendGroups.Add(new
            {
                // 占位分组名：前端据此隐藏分组标题，让这些用户直接平铺在群组下方
                groupname = NoGroupName,
                id = NoGroupName,
                list = noGroupUsers
                    .Select(u => new
                    {
                        username = u.Username,
                        id = u.Id,
                        avatar = Avatars.OrDefault(u.Avatar),
                        sign = u.Sign,
                        status = ChatHub.IsOnline(u.Id) ? "online" : "offline"
                    })
                    .OrderByDescending(m => m.status == "online")
                    .ThenBy(m => m.username)
            });
        }

        return Ok(new
        {
            code = 0,
            msg = "",
            data = new
            {
                mine = new
                {
                    username = me.Username,
                    id = me.Id,
                    status = ChatHub.IsOnline(me.Id) ? "online" : "offline",
                    sign = me.Sign,
                    avatar = Avatars.OrDefault(me.Avatar),
                    // 群组即主系统组织机构，聊天端不提供群组管理入口
                    admin = false
                },
                friend = friendGroups,
                group = myGroups.Select(g => new { groupname = g.GroupName ?? "", id = g.Id, avatar = Avatars.OrDefault(g.Avatar) })
            }
        });
    }

    /// <summary>搜索用户：按账号模糊匹配主系统用户，排除自己</summary>
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string? keyword)
    {
        var kw = (keyword ?? "").Trim();
        if (kw.Length == 0)
        {
            return Ok(new { code = 0, msg = "", data = Array.Empty<object>() });
        }
        if (kw.Length > 30)
        {
            kw = kw[..30];
        }

        var users = await _db.Users
            .Where(u => u.Username.Contains(kw) && u.Id != MyId && u.Deleted != 1)
            .OrderBy(u => u.Username)
            .Take(20)
            .ToListAsync();

        var data = users.Select(u => new
        {
            username = u.Username,
            id = u.Id,
            avatar = Avatars.OrDefault(u.Avatar),
            sign = u.Sign,
            status = ChatHub.IsOnline(u.Id) ? "online" : "offline"
        });
        return Ok(new { code = 0, msg = "", data });
    }

    /// <summary>群成员列表</summary>
    [HttpGet("members")]
    public async Task<IActionResult> Members([FromQuery] string id)
    {
        // 同理不能与 sys_user 做 SQL 连接（排序规则不同），先取成员ID再查用户
        var memberIds = await _db.GroupMembers
            .Where(m => m.GroupId == id)
            .Select(m => m.UserId)
            .ToListAsync();
        var rows = await _db.Users
            .Where(u => memberIds.Contains(u.Id) && u.Deleted != 1)
            .Select(u => new { username = u.Username, id = u.Id, avatar = u.Avatar, sign = u.Sign })
            .ToListAsync();

        var members = rows
            .Select(u => new { u.username, u.id, avatar = Avatars.OrDefault(u.avatar), u.sign })
            .ToList();

        return Ok(new { code = 0, msg = "", data = new { list = members } });
    }

    /// <summary>历史消息（chatlog 弹窗使用）；keyword 按关键字搜索，startDate/endDate 按日期区间过滤（本地日期，含首尾两天）</summary>
    [HttpGet("chatlog")]
    public async Task<IActionResult> ChatLog([FromQuery] string id, [FromQuery] string type, [FromQuery] string? keyword,
        [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        IQueryable<Message> query;
        if (type == "friend")
        {
            var me = MyId;
            query = _db.Messages.Where(m =>
                m.ToType == "friend" &&
                ((m.FromUserId == me && m.ToId == id) || (m.FromUserId == id && m.ToId == me)));
        }
        else if (type == "group")
        {
            // 仅群成员可查看群历史
            var isMember = await _db.GroupMembers.AnyAsync(m => m.GroupId == id && m.UserId == MyId);
            if (!isMember)
            {
                return Ok(new { code = 1, msg = "不是该群成员" });
            }
            query = _db.Messages.Where(m => m.ToType == "group" && m.ToId == id);
        }
        else
        {
            return Ok(new { code = 1, msg = "不支持的会话类型" });
        }

        // 日期区间过滤：前端传本地日期（yyyy-MM-dd），转换为 UTC 区间查询（库中 CreatedAt 为 UTC）
        var fromUtc = ParseLocalDate(startDate, isEnd: false);
        var toUtc = ParseLocalDate(endDate, isEnd: true);
        if (string.IsNullOrWhiteSpace(startDate) && string.IsNullOrWhiteSpace(endDate))
        {
            // 未选日期：不加时间限制
        }
        else
        {
            if (fromUtc == null && toUtc == null)
            {
                return Ok(new { code = 1, msg = "日期格式应为 yyyy-MM-dd" });
            }
            if (!string.IsNullOrWhiteSpace(startDate) && fromUtc == null)
            {
                return Ok(new { code = 1, msg = "开始日期格式应为 yyyy-MM-dd" });
            }
            if (!string.IsNullOrWhiteSpace(endDate) && toUtc == null)
            {
                return Ok(new { code = 1, msg = "结束日期格式应为 yyyy-MM-dd" });
            }
            if (fromUtc != null && toUtc != null && fromUtc > toUtc)
            {
                return Ok(new { code = 1, msg = "开始日期不能晚于结束日期" });
            }
            if (fromUtc != null) query = query.Where(m => m.CreatedAt >= fromUtc.Value);
            if (toUtc != null) query = query.Where(m => m.CreatedAt < toUtc.Value);
        }

        // 搜索模式：按关键字过滤整段会话（历史不止最近200条，搜索时放宽区间）
        var kw = keyword?.Trim();
        var isSearch = !string.IsNullOrEmpty(kw) || fromUtc != null || toUtc != null;
        if (!string.IsNullOrEmpty(kw))
        {
            query = query.Where(m => m.Content.Contains(kw!));
        }

        var total = await query.CountAsync(); // 匹配总数（仅在搜索时展示给用户）
        var take = isSearch ? 300 : 200;

        // 取最近 N 条，按时间升序返回
        var rows = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new { m.FromUserId, m.Content, m.CreatedAt })
            .ToListAsync();

        // 发送者可能在 sys_user（正式用户）或 chat_shadow_user（客服/访客）
        var senders = await UserDirectory.FindManyAsync(_db, rows.Select(r => r.FromUserId));

        var messages = rows.Select(m =>
        {
            senders.TryGetValue(m.FromUserId, out var u);
            return new
            {
                username = u?.Username ?? "未知用户",
                id = m.FromUserId,
                avatar = Avatars.OrDefault(u?.Avatar),
                timestamp = new DateTimeOffset(DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
                content = m.Content
            };
        }).ToList();

        return Ok(new { code = 0, msg = "", data = messages, total });
    }

    /// <summary>修改签名</summary>
    [HttpPost("sign")]
    public async Task<IActionResult> Sign([FromBody] SignRequest req)
    {
        var sign = req.Sign?.Trim() ?? "";
        var shadow = await _db.ShadowUsers.FirstOrDefaultAsync(u => u.Id == MyId);
        if (shadow != null)
        {
            shadow.Sign = sign;
            await _db.SaveChangesAsync();
            return Ok(new { code = 0, msg = "" });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == MyId);
        if (user == null)
        {
            return Unauthorized();
        }
        // 直接更新主系统用户表 sys_user.F_Signature（聊天资料变更同步回主系统）
        user.Sign = sign;
        await _db.SaveChangesAsync();
        return Ok(new { code = 0, msg = "" });
    }

    /// <summary>更换头像（multipart 上传图片，更新用户头像）</summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> Avatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Ok(new { code = 1, msg = "请选择图片文件" });
        }
        if (file.Length > 2 * 1024 * 1024)
        {
            return Ok(new { code = 1, msg = "头像图片不能超过 2MB" });
        }
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        string[] allowed = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        if (!allowed.Contains(ext))
        {
            return Ok(new { code = 1, msg = "仅支持 jpg/png/gif/bmp/webp 图片" });
        }

        var shadow = await _db.ShadowUsers.FirstOrDefaultAsync(u => u.Id == MyId);
        var user = shadow == null ? await _db.Users.FirstOrDefaultAsync(u => u.Id == MyId) : null;
        if (shadow == null && user == null)
        {
            return Unauthorized();
        }

        var oldAvatar = shadow?.Avatar ?? user?.Avatar ?? "";

        // 保存新头像
        var dir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(dir);
        var name = $"avatar-{MyId}-{DateTime.Now:yyyyMMddHHmmss}{ext}";
        var fullPath = Path.Combine(dir, name);
        await using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        // 删除旧头像文件（仅清理本系统上传的头像）
        if (!string.IsNullOrEmpty(oldAvatar) && oldAvatar.Contains("/uploads/avatar-"))
        {
            var oldPath = Path.Combine(dir, Path.GetFileName(oldAvatar));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        var newAvatar = "/uploads/" + name;
        if (shadow != null)
        {
            shadow.Avatar = newAvatar;
        }
        else
        {
            user!.Avatar = newAvatar;
        }
        await _db.SaveChangesAsync();
        return Ok(new { code = 0, msg = "", data = new { src = newAvatar } });
    }

    /// <summary>恢复默认头像（清空自定义头像，并删除已上传的头像文件）</summary>
    [HttpPost("avatar/reset")]
    public async Task<IActionResult> ResetAvatar()
    {
        var shadow = await _db.ShadowUsers.FirstOrDefaultAsync(u => u.Id == MyId);
        var user = shadow == null ? await _db.Users.FirstOrDefaultAsync(u => u.Id == MyId) : null;
        if (shadow == null && user == null)
        {
            return Unauthorized();
        }

        var oldAvatar = shadow?.Avatar ?? user?.Avatar ?? "";
        if (!string.IsNullOrEmpty(oldAvatar) && oldAvatar.Contains("/uploads/avatar-"))
        {
            var oldPath = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(oldAvatar));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        if (shadow != null)
        {
            shadow.Avatar = "";
        }
        else
        {
            user!.Avatar = "";
        }
        await _db.SaveChangesAsync();
        return Ok(new { code = 0, msg = "" });
    }

    public record SignRequest(string Sign);

    public record GroupRequest(string Id);

    /// <summary>全部群组（含是否已加入、成员数）</summary>
    [HttpGet("groups")]
    public async Task<IActionResult> AllGroups()
    {
        var myId = MyId;
        var allGroups = await _db.ChatGroups
            .Where(g => g.Deleted != 1 && g.Enabled != 0)
            .OrderBy(g => g.GroupName)
            .ToListAsync();
        // 避免 sys_organize 与 chat_group_member 的列间比较（排序规则不同）
        var memberships = await _db.GroupMembers
            .Select(m => new { m.GroupId, m.UserId })
            .ToListAsync();
        var rows = allGroups.Select(g => new
        {
            id = g.Id,
            groupname = g.GroupName ?? "",
            memberCount = memberships.Count(m => m.GroupId == g.Id),
            joined = memberships.Any(m => m.GroupId == g.Id && m.UserId == myId)
        }).ToList();

        return Ok(new
        {
            code = 0,
            msg = "",
            data = rows.Select(g => new
            {
                g.id,
                g.groupname,
                avatar = Avatars.OrDefault(""),
                g.memberCount,
                g.joined
            })
        });
    }

    /// <summary>加入群组：群成员由主系统组织架构决定，聊天端不提供手工入群</summary>
    [HttpPost("groups/join")]
    public async Task<IActionResult> JoinGroup([FromBody] GroupRequest req)
        => Ok(new { code = 1, msg = "群成员由管理系统组织架构维护，如需调整请在管理系统中变更所属部门" });

    /// <summary>退出群组：群成员由主系统组织架构决定，聊天端不提供手工退群</summary>
    [HttpPost("groups/leave")]
    public async Task<IActionResult> LeaveGroup([FromBody] GroupRequest req)
        => Ok(new { code = 1, msg = "群成员由管理系统组织架构维护，如需调整请在管理系统中变更所属部门" });
}
