using LayimChat.Data;
using LayimChat.Models;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Services;

/// <summary>
/// 聊天用户目录：统一读取主系统 sys_user 与聊天影子用户 chat_shadow_user（客服/访客）。
/// </summary>
public static class UserDirectory
{
    /// <summary>按用户ID查找（先查主系统用户，再查影子用户）</summary>
    public static async Task<User?> FindAsync(LayimDbContext db, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Deleted != 1);
        if (user != null)
        {
            return user;
        }

        var shadow = await db.ShadowUsers.FirstOrDefaultAsync(u => u.Id == id);
        return shadow == null ? null : ToUser(shadow);
    }

    /// <summary>按账号查找（先查主系统用户，再查影子用户）</summary>
    public static async Task<User?> FindByAccountAsync(LayimDbContext db, string? account)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == account && u.Deleted != 1);
        if (user != null)
        {
            return user;
        }

        var shadow = await db.ShadowUsers.FirstOrDefaultAsync(u => u.Username == account);
        return shadow == null ? null : ToUser(shadow);
    }

    /// <summary>批量按ID查找（缺失的ID不会出现在结果中）</summary>
    public static async Task<Dictionary<string, User>> FindManyAsync(LayimDbContext db, IEnumerable<string> ids)
    {
        var idList = ids.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
        var result = new Dictionary<string, User>();
        if (idList.Count == 0)
        {
            return result;
        }

        var users = await db.Users.Where(u => idList.Contains(u.Id)).ToListAsync();
        foreach (var u in users)
        {
            result[u.Id] = u;
        }

        var shadowIds = idList.Where(i => !result.ContainsKey(i)).ToList();
        if (shadowIds.Count > 0)
        {
            var shadows = await db.ShadowUsers.Where(u => shadowIds.Contains(u.Id)).ToListAsync();
            foreach (var s in shadows)
            {
                result[s.Id] = ToUser(s);
            }
        }

        return result;
    }

    /// <summary>影子用户转成聊天用户实体</summary>
    public static User ToUser(ChatShadowUser shadow) => new()
    {
        Id = shadow.Id,
        Username = shadow.Username,
        RealName = shadow.Username,
        Avatar = shadow.Avatar,
        Sign = shadow.Sign,
        IsAdmin = shadow.IsAdmin ? (byte)1 : (byte)0,
        OrganizeId = ""
    };

    /// <summary>是否是影子用户（客服/访客）</summary>
    public static async Task<bool> IsShadowAsync(LayimDbContext db, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }
        return await db.ShadowUsers.AnyAsync(u => u.Id == id);
    }
}
