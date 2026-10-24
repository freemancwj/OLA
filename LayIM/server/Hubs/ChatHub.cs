using System.Security.Claims;
using LayimChat.Data;
using LayimChat.Models;
using LayimChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Hubs;

/// <summary>
/// 聊天 Hub：负责消息的实时转发与持久化。
/// - 发送者本地已由 LayIM 渲染自己的消息，服务端只转发给其他人。
/// - 好友消息通过 Clients.User 投递给目标用户的所有连接。
/// - 群消息通过 SignalR 组（group_{群ID}）投递给除发送者外的群成员。
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly LayimDbContext _db;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(LayimDbContext db, ILogger<ChatHub> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string MyId => Context.UserIdentifier
        ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new HubException("未登录");

    public static string GroupName(string groupId) => $"group_{groupId}";

    /// <summary>单聊/视频通话权限：企业用户之间可自由沟通（联系人列表里能看到的人就能发消息）；
    /// 网站访客仅能与客服单聊，客服仅能与访客单聊，访客之间互相隔离。</summary>
    private async Task<bool> CanChatAsync(string a, string b)
    {
        if (a == b) return false;
        var userA = await UserDirectory.FindAsync(_db, a);
        var userB = await UserDirectory.FindAsync(_db, b);
        if (userA == null || userB == null) return false;
        if (ServiceAgent.IsGuestName(userA.Username)) return b == ServiceAgent.Id;
        if (ServiceAgent.IsGuestName(userB.Username)) return a == ServiceAgent.Id;
        return true;
    }

    /// <summary>用户当前是否有至少一个在线连接（供 init 接口标记好友在线状态）。
    /// 直接以连接表为准：连接级计数在"断开与重连并发"时可能被误删，导致在线用户显示离线。
    /// 客服账号特判为恒在线：访客侧始终显示「在线」，客服不在线时消息按离线消息落库、上线后补推。</summary>
    public static bool IsOnline(string userId)
        => userId == ServiceAgent.Id
           || (UserConnections.TryGetValue(userId, out var conns) && !conns.IsEmpty);

    /// <summary>用户ID -> 其当前所有连接ID（加群/退群时需要把该用户所有连接同步到 SignalR 组）</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, byte>> UserConnections = new();

    /// <summary>取某用户当前所有在线连接ID（供 LayimController 加群/退群时同步组）</summary>
    public static List<string> ConnectionIdsOf(string userId)
        => UserConnections.TryGetValue(userId, out var conns) ? conns.Keys.ToList() : new List<string>();

    // ==================== 视频通话信令（1对1 WebRTC 协商转发） ====================
    // 通话会话表：主叫ID -> 被叫ID。邀请发起即占用（防并发呼叫/忙线），接通后补写反向条目，结束/拒绝/取消/断线时清理。
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> CallSessions = new();

    /// <summary>离线判定宽限期（毫秒）：断开后等待这么久，仍未重新连接才视为真离线并结束通话</summary>
    private const int OfflineGraceMs = 4000;

    /// <summary>发起视频通话邀请。对方在线且双方均空闲时推送 IncomingCall。</summary>
    public async Task CallInvite(string toUserId)
    {
        var myId = MyId;
        if (toUserId == myId) throw new HubException("不能与自己通话");

        var isFriend = await CanChatAsync(myId, toUserId);
        if (!isFriend) throw new HubException("无法与该用户通话");
        if (!IsOnline(toUserId)) throw new HubException("对方不在线，无法接通");
        if (CallSessions.ContainsKey(myId)) throw new HubException("你正在通话中，请先结束当前通话");
        if (CallSessions.ContainsKey(toUserId) || CallSessions.Values.Contains(toUserId))
            throw new HubException("对方忙线中，请稍后再试");

        var me = await UserDirectory.FindAsync(_db, myId) ?? throw new HubException("当前用户不存在");
        CallSessions[myId] = toUserId;
        await Clients.User(toUserId).SendAsync("IncomingCall", new
        {
            id = myId,
            username = me.Username,
            avatar = Avatars.OrDefault(me.Avatar)
        });
        _logger.LogInformation("视频通话邀请：{From} -> {To}", myId, toUserId);
    }

    /// <summary>被叫接听：建立双向会话并通知主叫开始 WebRTC 协商。</summary>
    public Task CallAccept(string fromUserId)
    {
        var myId = MyId;
        if (!CallSessions.TryGetValue(fromUserId, out var peer) || peer != myId)
            throw new HubException("通话邀请不存在或已失效");
        CallSessions[myId] = fromUserId;
        _logger.LogInformation("通话已接通：{A} <-> {B}", fromUserId, myId);
        return Clients.User(fromUserId).SendAsync("CallAccepted", new { by = myId });
    }

    /// <summary>被叫拒绝：清理会话并通知主叫。</summary>
    public Task CallReject(string fromUserId)
    {
        var myId = MyId;
        if (CallSessions.TryGetValue(fromUserId, out var peer) && peer == myId)
        {
            CallSessions.TryRemove(fromUserId, out _);
        }
        _logger.LogInformation("通话被拒绝：{B} 拒绝 {A}", myId, fromUserId);
        return Clients.User(fromUserId).SendAsync("CallRejected", new { by = myId });
    }

    /// <summary>主叫取消邀请（未接通前）：清理会话并通知被叫关闭来电窗口。</summary>
    public Task CallCancel(string toUserId)
    {
        var myId = MyId;
        if (CallSessions.TryGetValue(myId, out var peer) && peer == toUserId)
        {
            CallSessions.TryRemove(myId, out _);
        }
        _logger.LogInformation("呼叫被取消：{A} -> {B}", myId, toUserId);
        return Clients.User(toUserId).SendAsync("CallCancelled", new { by = myId });
    }

    /// <summary>任一方挂断（接通后）：双向清理会话并通知对方结束。</summary>
    public Task CallHangup(string peerId)
    {
        var myId = MyId;
        CallSessions.TryRemove(myId, out _);
        if (CallSessions.TryGetValue(peerId, out var p) && p == myId)
        {
            CallSessions.TryRemove(peerId, out _);
        }
        _logger.LogInformation("通话挂断：{A} 挂断与 {B} 的通话", myId, peerId);
        return Clients.User(peerId).SendAsync("CallEnded", new { by = myId, reason = "hangup" });
    }

    /// <summary>
    /// 客户端（重）连上后主动清理服务端可能残留的通话会话。
    /// 用于页面刷新场景：刷新后本地已无通话状态，但对端可能还挂着通话窗口。
    /// </summary>
    public async Task EndMyCalls()
    {
        var myId = MyId;
        var others = new HashSet<string>();
        foreach (var kv in CallSessions)
        {
            if (kv.Key == myId || kv.Value == myId)
            {
                if (CallSessions.TryRemove(kv.Key, out _))
                {
                    others.Add(kv.Key == myId ? kv.Value : kv.Key);
                }
            }
        }
        foreach (var other in others)
        {
            _logger.LogInformation("清理残留通话会话：{A} 的会话已移除，通知 {B}", myId, other);
            await Clients.User(other).SendAsync("CallEnded", new { by = myId, reason = "left" });
        }
    }

    /// <summary>转发 WebRTC 协商数据（SDP offer/answer、ICE candidate），仅限会话双方。</summary>
    public Task RtcSignal(string toUserId, string payload)
    {
        var myId = MyId;
        var ok = (CallSessions.TryGetValue(myId, out var p1) && p1 == toUserId) ||
                 (CallSessions.TryGetValue(toUserId, out var p2) && p2 == myId);
        if (!ok) throw new HubException("通话会话不存在");
        if ((payload?.Length ?? 0) > 64 * 1024) throw new HubException("信令数据过大");
        return Clients.User(toUserId).SendAsync("RtcSignal", new { from = myId, payload });
    }

    /// <summary>用户完全离线时清理其参与的通话会话并通知对端。</summary>
    private async Task EndCallsForOfflineUserAsync(string userId)
    {
        var others = new HashSet<string>();
        foreach (var kv in CallSessions)
        {
            if (kv.Key == userId || kv.Value == userId)
            {
                CallSessions.TryRemove(kv.Key, out _);
                others.Add(kv.Key == userId ? kv.Value : kv.Key);
            }
        }
        foreach (var other in others)
        {
            _logger.LogInformation("用户 {Uid} 已离线，结束其与 {Other} 的通话会话", userId, other);
            await Clients.User(other).SendAsync("CallEnded", new { by = userId, reason = "offline" });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var myId = MyId;
        var groupIds = await JoinMyGroupsAsync();
        var conns = UserConnections.GetOrAdd(myId, _ => new System.Collections.Concurrent.ConcurrentDictionary<string, byte>());
        conns.TryAdd(Context.ConnectionId, 0);
        _logger.LogInformation("用户 {Uid} 建立连接 {ConnId}，当前连接数 {Count}", myId, Context.ConnectionId, conns.Count);
        await PushOfflineMessagesAsync(myId, groupIds);

        // 补推完成后推进水位：避免用户长期保持在线时，新标签页/新设备反复收到同一批历史消息。
        // 补推之后产生的消息都会实时投递到本连接，因此不存在漏推空隙。
        await TouchLastSeenAsync(myId, "online");

        await NotifyStatusAsync(myId, "online");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var myId = MyId;
        // 先摘除本连接，再以剩余连接数判断是否真正离线（用户级计数在并发下不可靠）
        var left = 0;
        if (UserConnections.TryGetValue(myId, out var conns))
        {
            conns.TryRemove(Context.ConnectionId, out _);
            left = conns.Count;
            if (conns.IsEmpty)
            {
                UserConnections.TryRemove(myId, out _);
            }
        }
        _logger.LogInformation("用户 {Uid} 断开连接 {ConnId}，剩余连接数 {Left}，异常：{Ex}", myId, Context.ConnectionId, left, exception?.Message ?? "无");
        if (left <= 0)
        {
            // 最后一个连接断开：记录离线时间（下次上线据此推送离线消息）
            await TouchLastSeenAsync(myId, "offline");
            _logger.LogInformation("用户 {Uid} 已离线", myId);
            await NotifyStatusAsync(myId, "offline");

            // 结束通话必须滞后判定：页面刷新、SignalR 自动重连、网络抖动都会造成短暂断开，
            // 若立即结束通话，就会出现"一接通对方就马上挂断"。宽限期内重新连上则不动通话。
            await Task.Delay(OfflineGraceMs);
            if (!IsOnline(myId))
            {
                await EndCallsForOfflineUserAsync(myId);
            }
            else
            {
                _logger.LogInformation("用户 {Uid} 在宽限期({Grace}ms)内重新连接，保留其通话会话", myId, OfflineGraceMs);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>断线重连后重新加入群组（内存服务器重连后组信息会丢失）</summary>
    public async Task Rejoin()
    {
        await JoinMyGroupsAsync();
    }

    /// <summary>
    /// 发送消息并保存到数据库。
    /// toType: friend / group；toId: 好友用户ID 或 群ID。
    /// </summary>
    public async Task SendMessage(string toType, string toId, string content)
    {
        content = content?.Trim() ?? "";
        if (content.Length == 0)
        {
            throw new HubException("消息内容不能为空");
        }
        if (content.Length > 5000)
        {
            throw new HubException("消息内容过长");
        }
        if (toType is not ("friend" or "group") || string.IsNullOrEmpty(toId))
        {
            throw new HubException("无效的接收对象");
        }

        var fromId = MyId;
        var from = await UserDirectory.FindAsync(_db, fromId)
            ?? throw new HubException("当前用户不存在");

        if (toType == "friend" && toId != fromId)
        {
            var canChat = await CanChatAsync(fromId, toId);
            if (!canChat)
            {
                throw new HubException("无法给该用户发送消息");
            }
        }
        else if (toType == "group")
        {
            var isMember = await _db.GroupMembers.AnyAsync(m => m.GroupId == toId && m.UserId == fromId);
            if (!isMember)
            {
                throw new HubException("你不是该群成员");
            }
        }

        // 保存到数据库
        var message = new Message
        {
            FromUserId = fromId,
            ToType = toType,
            ToId = toId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var payload = new
        {
            username = from.Username,
            avatar = Avatars.OrDefault(from.Avatar),
            // LayIM 以 id 匹配聊天窗口：好友消息窗口以"好友(发送者)ID"为键，群消息窗口以群ID为键
            id = toType == "group" ? toId : fromId,
            type = toType,
            content,
            timestamp = new DateTimeOffset(DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds()
        };

        if (toType == "friend")
        {
            await Clients.User(toId).SendAsync("ReceiveMessage", payload);
        }
        else
        {
            // OthersInGroup：不回发给发送者当前连接（本地已渲染）
            await Clients.OthersInGroup(GroupName(toId)).SendAsync("ReceiveMessage", payload);
        }
    }

    private async Task<List<string>> JoinMyGroupsAsync()
    {
        var groupIds = await _db.GroupMembers
            .Where(m => m.UserId == MyId)
            .Select(m => m.GroupId)
            .ToListAsync();

        foreach (var gid in groupIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gid));
        }
        return groupIds;
    }

    /// <summary>
    /// 上线时把离线期间收到的消息主动推送给当前连接：
    /// 发给"我"的单聊消息 + "我"所在群的群消息（排除自己发的），最多补推最近 200 条。
    /// </summary>
    private async Task PushOfflineMessagesAsync(string myId, List<string> groupIds)
    {
        var lastSeen = await GetLastSeenAsync(myId);

        var msgs = await _db.Messages
            .Where(m => m.CreatedAt > lastSeen && m.FromUserId != myId &&
                ((m.ToType == "friend" && m.ToId == myId) ||
                 (m.ToType == "group" && groupIds.Contains(m.ToId))))
            .OrderBy(m => m.CreatedAt)
            .Take(200)
            .ToListAsync();
        if (msgs.Count == 0) return;

        var senderIds = msgs.Select(m => m.FromUserId).Distinct().ToList();
        var senders = await UserDirectory.FindManyAsync(_db, senderIds);

        foreach (var m in msgs)
        {
            senders.TryGetValue(m.FromUserId, out var sender);
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                username = sender?.Username ?? "未知用户",
                avatar = Avatars.OrDefault(sender?.Avatar),
                // 与实时转发保持同一规则：好友消息 id=发送者ID，群消息 id=群ID
                id = m.ToType == "group" ? m.ToId : m.FromUserId,
                type = m.ToType,
                content = m.Content,
                timestamp = new DateTimeOffset(DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds()
            });
        }
    }

    /// <summary>记录用户最后活跃时间（chat_user_state，避免写主系统 sys_user）</summary>
    private async Task TouchLastSeenAsync(string userId, string status)
    {
        var state = await _db.UserStates.FirstOrDefaultAsync(s => s.UserId == userId);
        if (state == null)
        {
            state = new ChatUserState { UserId = userId };
            _db.UserStates.Add(state);
        }
        state.LastSeenAt = DateTime.UtcNow;
        state.Status = status;
        await _db.SaveChangesAsync();
    }

    /// <summary>读取用户上次活跃时间（离线消息水位）</summary>
    private async Task<DateTime> GetLastSeenAsync(string userId)
    {
        var state = await _db.UserStates.FirstOrDefaultAsync(s => s.UserId == userId);
        return state?.LastSeenAt ?? DateTime.MinValue;
    }

    /// <summary>
    /// 广播上下线状态。通讯录已改为按主系统组织架构（群组）展示，任意两类用户之间都可能是可见联系人，
    /// 若仍只推送给 chat_friendship 里的好友，组织内用户永远收不到状态变化，就会一直显示"离线"。
    /// </summary>
    private async Task NotifyStatusAsync(string userId, string status)
        => await Clients.All.SendAsync("FriendStatus", new { id = userId, status });
}
