using LayimChat.Models;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Data;

/// <summary>
/// 聊天系统数据上下文：与主系统共用同一个数据库。
/// - 用户：sys_user（只读映射）
/// - 群组：sys_organize（只读映射）
/// - 登录密码：sys_userlogon（只读映射）
/// - 其余聊天数据：chat_* 表（启动时由 DbInitializer 创建）
/// </summary>
public class LayimDbContext : DbContext
{
    public LayimDbContext(DbContextOptions<LayimDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserLogon> UserLogons => Set<UserLogon>();
    public DbSet<ChatShadowUser> ShadowUsers => Set<ChatShadowUser>();
    public DbSet<ChatUserState> UserStates => Set<ChatUserState>();
    public DbSet<FriendGroup> FriendGroups => Set<FriendGroup>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // 主系统用户表（只读）
        mb.Entity<User>(e =>
        {
            e.ToTable("sys_user");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("F_Id").ValueGeneratedNever();
            e.Property(x => x.Username).HasColumnName("F_Account");
            e.Property(x => x.RealName).HasColumnName("F_RealName");
            e.Property(x => x.Avatar).HasColumnName("F_HeadIcon");
            e.Property(x => x.Sign).HasColumnName("F_Signature");
            e.Property(x => x.IsAdmin).HasColumnName("F_IsAdmin");
            e.Property(x => x.OrganizeId).HasColumnName("F_OrganizeId");
            e.Property(x => x.Enabled).HasColumnName("F_EnabledMark");
            e.Property(x => x.Deleted).HasColumnName("F_DeleteMark");
        });

        // 主系统登录密码表（只读）
        mb.Entity<UserLogon>(e =>
        {
            e.ToTable("sys_userlogon");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("F_Id").ValueGeneratedNever();
            e.Property(x => x.UserId).HasColumnName("F_UserId");
            e.Property(x => x.Password).HasColumnName("F_UserPassword");
            e.Property(x => x.SecretKey).HasColumnName("F_UserSecretkey");
        });

        // 主系统组织机构表（只读，作为聊天群组）
        mb.Entity<ChatGroup>(e =>
        {
            e.ToTable("sys_organize");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("F_Id").ValueGeneratedNever();
            e.Property(x => x.GroupName).HasColumnName("F_FullName");
            e.Property(x => x.OwnerId).HasColumnName("F_ManagerId");
            e.Property(x => x.Enabled).HasColumnName("F_EnabledMark");
            e.Property(x => x.Deleted).HasColumnName("F_DeleteMark");
            e.Ignore(x => x.Avatar);
        });

        mb.Entity<ChatShadowUser>(e =>
        {
            e.ToTable("chat_shadow_user");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.Username).IsUnique();
        });

        mb.Entity<ChatUserState>(e =>
        {
            e.ToTable("chat_user_state");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).ValueGeneratedNever();
        });

        mb.Entity<FriendGroup>(e =>
        {
            e.ToTable("chat_friend_group");
            e.HasIndex(x => x.OwnerId);
        });

        mb.Entity<Friendship>(e =>
        {
            e.ToTable("chat_friendship");
            e.HasIndex(x => new { x.OwnerId, x.FriendId }).IsUnique();
        });

        mb.Entity<GroupMember>(e =>
        {
            e.ToTable("chat_group_member");
            e.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
        });

        mb.Entity<Message>(e =>
        {
            e.ToTable("chat_message");
            e.HasIndex(x => new { x.ToType, x.ToId, x.CreatedAt });
        });
    }
}
