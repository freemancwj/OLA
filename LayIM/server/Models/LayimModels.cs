using System.ComponentModel.DataAnnotations.Schema;

namespace LayimChat.Models;

/// <summary>
/// 聊天用户：直接映射主系统用户表 sys_user（只读为主）。
/// 字段对应关系：Id→F_Id、Username→F_Account、RealName→F_RealName、
/// Avatar→F_HeadIcon、Sign→F_Signature、IsAdmin→F_IsAdmin、OrganizeId→F_OrganizeId。
/// </summary>
public class User
{
    /// <summary>用户ID（sys_user.F_Id，GUID 字符串）</summary>
    public string Id { get; set; } = "";

    /// <summary>登录账号（sys_user.F_Account）</summary>
    public string? Username { get; set; }

    /// <summary>姓名（sys_user.F_RealName）</summary>
    public string? RealName { get; set; }

    /// <summary>头像（sys_user.F_HeadIcon）</summary>
    public string? Avatar { get; set; }

    /// <summary>个性签名（sys_user.F_Signature）</summary>
    public string? Sign { get; set; }

    /// <summary>是否管理员（sys_user.F_IsAdmin，tinyint 0/1）</summary>
    public byte? IsAdmin { get; set; }

    /// <summary>所属部门/组织（sys_user.F_OrganizeId），群组成员关系由此推导</summary>
    public string? OrganizeId { get; set; }

    /// <summary>有效标记（sys_user.F_EnabledMark，tinyint 0/1）</summary>
    public byte? Enabled { get; set; }

    /// <summary>删除标记（sys_user.F_DeleteMark，tinyint 0/1）</summary>
    public byte? Deleted { get; set; }
}

/// <summary>主系统登录密码表 sys_userlogon（只读，用于校验聊天登录密码）</summary>
public class UserLogon
{
    public string Id { get; set; } = "";

    public string? UserId { get; set; }

    public string? Password { get; set; }

    public string? SecretKey { get; set; }
}

/// <summary>
/// 聊天影子用户：主系统 sys_user 之外的身份（网站客服 kefu、免登录访客）。
/// 存放在聊天库 chat_shadow_user，避免污染主系统用户表。
/// </summary>
public class ChatShadowUser
{
    public string Id { get; set; } = "";

    public string Username { get; set; } = "";

    /// <summary>PBKDF2 密码哈希（Base64）</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>密码盐（Base64）</summary>
    public string Salt { get; set; } = "";

    public string Avatar { get; set; } = "";

    public string Sign { get; set; } = "";

    public string Status { get; set; } = "online";

    public bool IsAdmin { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>聊天用户状态（离线消息水位），避免向 sys_user 写状态字段</summary>
public class ChatUserState
{
    public string UserId { get; set; } = "";

    public DateTime? LastSeenAt { get; set; }

    public string Status { get; set; } = "online";
}

/// <summary>好友分组（每个用户自己的分组）</summary>
public class FriendGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = "";

    /// <summary>所属用户</summary>
    public string OwnerId { get; set; } = "";
}

/// <summary>好友关系</summary>
public class Friendship
{
    public int Id { get; set; }

    /// <summary>拥有者（谁的好友列表）</summary>
    public string OwnerId { get; set; } = "";

    public string FriendId { get; set; } = "";

    public int FriendGroupId { get; set; }
}

/// <summary>
/// 聊天群组：直接映射主系统组织机构表 sys_organize。
/// 字段对应关系：Id→F_Id、GroupName→F_FullName、OwnerId→F_ManagerId。
/// </summary>
public class ChatGroup
{
    /// <summary>群ID（sys_organize.F_Id）</summary>
    public string Id { get; set; } = "";

    /// <summary>群名称（sys_organize.F_FullName）</summary>
    public string? GroupName { get; set; }

    /// <summary>群主（sys_organize.F_ManagerId）</summary>
    public string? OwnerId { get; set; }

    /// <summary>有效标记（sys_organize.F_EnabledMark，tinyint 0/1）</summary>
    public byte? Enabled { get; set; }

    /// <summary>删除标记（sys_organize.F_DeleteMark，tinyint 0/1）</summary>
    public byte? Deleted { get; set; }

    /// <summary>头像：sys_organize 无对应字段，固定为空（前端回落默认图）</summary>
    [NotMapped]
    public string Avatar { get; set; } = "";
}

/// <summary>群成员（聊天库维护，启动/初始化时按 sys_user.F_OrganizeId 自动同步）</summary>
public class GroupMember
{
    public long Id { get; set; }

    public string GroupId { get; set; } = "";

    public string UserId { get; set; } = "";
}

/// <summary>聊天消息</summary>
public class Message
{
    public long Id { get; set; }

    /// <summary>发送者用户ID</summary>
    public string FromUserId { get; set; } = "";

    /// <summary>friend / group</summary>
    public string ToType { get; set; } = "";

    /// <summary>接收者：好友用户ID 或 群ID</summary>
    public string ToId { get; set; } = "";

    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
