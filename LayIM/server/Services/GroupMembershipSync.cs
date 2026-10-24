using LayimChat.Data;
using LayimChat.Models;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Services;

/// <summary>
/// 群成员同步：群组即主系统组织机构（sys_organize），成员即该组织下未删除的用户。
/// 每次聊天初始化时按 sys_user.F_OrganizeId 对齐 chat_group_member，人员调动自动生效。
/// </summary>
public static class GroupMembershipSync
{
    public static async Task SyncAsync(LayimDbContext db)
    {
        var groupIds = await db.ChatGroups
            .Where(g => g.Deleted != 1 && g.Enabled != 0)
            .Select(g => g.Id)
            .ToListAsync();
        var groupSet = groupIds.ToHashSet();

        var users = await db.Users
            .Where(u => u.Deleted != 1 && u.OrganizeId != null && u.OrganizeId != "")
            .Select(u => new { u.Id, u.OrganizeId })
            .ToListAsync();

        // 主系统 F_OrganizeId 支持多组织（逗号分隔），需拆开分别匹配
        var desired = users
            .SelectMany(u => (u.OrganizeId ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(groupSet.Contains)
                .Select(g => (GroupId: g, Id: u.Id)))
            .ToHashSet();

        var existing = await db.GroupMembers
            .Select(m => new { m.Id, m.GroupId, m.UserId })
            .ToListAsync();
        var existingPairs = existing.Select(e => (GroupId: e.GroupId, e.UserId)).ToHashSet();

        var additions = desired.Except(existingPairs);
        foreach (var (groupId, userId) in additions)
        {
            db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
        }

        // 清理失效成员：已调离该组织、或群组（组织）已删除/停用
        var stale = existing.Where(e => !desired.Contains((e.GroupId, e.UserId))).ToList();
        if (stale.Count > 0)
        {
            var staleIds = stale.Select(s => s.Id).ToList();
            await db.GroupMembers.Where(m => staleIds.Contains(m.Id)).ExecuteDeleteAsync();
        }

        if (additions.Any() || stale.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}
