using LayimChat.Data;
using LayimChat.Models;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Services;

/// <summary>
/// 网站客服模式：固定客服账号 + 临时访客账号。
/// 两者都属于「聊天影子用户」（chat_shadow_user），不写入主系统 sys_user，
/// 因此不会出现在主系统用户管理和企业通讯录里。
/// - 客服是固定影子账号（ID 999998），对访客侧永远显示在线（见 ChatHub.IsOnline 特判）；
///   客服人员用聊天登录页以该账号登录后，可同时接待多名访客（一对多）。
/// - 访客是免登录的临时影子账号：用户名随机生成（访客+8位随机字符），
///   只与客服互为好友、不加入任何群组。
/// </summary>
public static class ServiceAgent
{
    /// <summary>固定客服账号 ID</summary>
    public const string Id = "999998";

    /// <summary>客服登录用户名</summary>
    public const string Username = "kefu";

    /// <summary>客服显示名</summary>
    public const string DisplayName = "在线客服";

    /// <summary>客服初始密码（首次建号时设置；生产环境请登录后自行修改）</summary>
    public const string Password = "kefu@123456";

    /// <summary>访客用户名前缀（识别访客：用户名以该前缀开头）</summary>
    public const string GuestPrefix = "访客";

    /// <summary>是否访客账号</summary>
    public static bool IsGuestName(string? username)
        => username != null && username.StartsWith(GuestPrefix);

    /// <summary>确保客服影子账号存在（幂等，每次启动调用；不加入任何群组）</summary>
    public static void EnsureAgent(LayimDbContext db)
    {
        var agent = db.ShadowUsers.FirstOrDefault(u => u.Id == Id)
            ?? db.ShadowUsers.FirstOrDefault(u => u.Username == Username);
        if (agent != null)
        {
            return; // 已存在：保留客服自己改过的头像/签名
        }

        var (hash, salt) = PasswordCrypto.Hash(Password);
        db.ShadowUsers.Add(new ChatShadowUser
        {
            Id = Id,
            Username = Username,
            PasswordHash = hash,
            Salt = salt,
            Avatar = "",
            Sign = "为您服务，请直接留言",
            Status = "online",
            CreatedAt = DateTime.UtcNow
        });

        // 客服的好友分组（接待的访客都放在这里）
        db.FriendGroups.Add(new FriendGroup { GroupName = "访客", OwnerId = Id });

        db.SaveChanges();
    }

    /// <summary>创建临时访客影子账号：随机用户名，只与客服互加好友，不加入任何群组</summary>
    public static async Task<User> CreateGuestAsync(LayimDbContext db)
    {
        var agent = await db.ShadowUsers.FirstOrDefaultAsync(u => u.Id == Id)
            ?? throw new InvalidOperationException("客服账号不存在");
        var agentGroupId = await db.FriendGroups
            .Where(g => g.OwnerId == agent.Id)
            .OrderBy(g => g.Id)
            .Select(g => (int?)g.Id)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException("客服好友分组不存在");

        // 随机用户名：访客 + 8 位随机字符（撞名时重试）
        string username;
        do
        {
            username = GuestPrefix + Guid.NewGuid().ToString("N")[..8];
        } while (await db.ShadowUsers.AnyAsync(u => u.Username == username));

        var (hash, salt) = PasswordCrypto.Hash(Guid.NewGuid().ToString("N")); // 访客无密码登录需求
        var guest = new ChatShadowUser
        {
            Id = "g" + Guid.NewGuid().ToString("N")[..15],
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            Avatar = "",
            Sign = "网站访客",
            Status = "online",
            CreatedAt = DateTime.UtcNow
        };
        db.ShadowUsers.Add(guest);

        var guestGroup = new FriendGroup { GroupName = "在线客服", OwnerId = guest.Id };
        db.FriendGroups.Add(guestGroup);
        await db.SaveChangesAsync(); // 先落库拿到分组自增ID

        // 访客 ↔ 客服 互加好友（单聊消息的好友校验要求至少一个方向存在关系）
        db.Friendships.Add(new Friendship { OwnerId = guest.Id, FriendId = agent.Id, FriendGroupId = guestGroup.Id });
        db.Friendships.Add(new Friendship { OwnerId = agent.Id, FriendId = guest.Id, FriendGroupId = agentGroupId });
        await db.SaveChangesAsync();

        return UserDirectory.ToUser(guest);
    }
}
