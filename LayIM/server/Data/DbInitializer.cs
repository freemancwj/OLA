using LayimChat.Services;
using Microsoft.EntityFrameworkCore;

namespace LayimChat.Data;

/// <summary>
/// 初始化：聊天系统与主系统共用一个数据库。
/// 用户（sys_user）、组织/群组（sys_organize）、登录密码（sys_userlogon）由主系统维护，聊天系统只读；
/// 其余聊天业务表统一以 chat_ 前缀在本库中创建，不改动主系统任何既有表。
/// </summary>
public static class DbInitializer
{
    public static void Initialize(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LayimDbContext>();

        var provider = app.Configuration["Database:Provider"] ?? "SqlServer";
        if (!string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("聊天系统已改为与主系统共用 SQL Server 数据库，请将 Database:Provider 配置为 SqlServer");
        }

        EnsureChatTables(db);

        // 上传文件目录
        var uploadsDir = Path.Combine(app.Environment.WebRootPath ?? ".", "uploads");
        Directory.CreateDirectory(uploadsDir);

        // 网站客服影子账号（chat_shadow_user，不写主系统用户表）
        ServiceAgent.EnsureAgent(db);
    }

    /// <summary>创建聊天业务表（幂等，仅新建 chat_ 前缀的表）</summary>
    private static void EnsureChatTables(LayimDbContext db)
    {
        var statements = new[]
        {
            @"IF OBJECT_ID(N'chat_shadow_user', N'U') IS NULL
              CREATE TABLE chat_shadow_user(
                  Id nvarchar(50) NOT NULL PRIMARY KEY,
                  Username nvarchar(50) NOT NULL,
                  PasswordHash nvarchar(200) NOT NULL,
                  Salt nvarchar(100) NOT NULL,
                  Avatar nvarchar(200) NULL,
                  [Sign] nvarchar(500) NULL,
                  Status nvarchar(20) NULL,
                  IsAdmin bit NOT NULL DEFAULT 0,
                  LastSeenAt datetime2 NULL,
                  CreatedAt datetime2 NOT NULL
              )",
            @"IF OBJECT_ID(N'chat_user_state', N'U') IS NULL
              CREATE TABLE chat_user_state(
                  UserId nvarchar(50) NOT NULL PRIMARY KEY,
                  LastSeenAt datetime2 NULL,
                  Status nvarchar(20) NULL
              )",
            @"IF OBJECT_ID(N'chat_friend_group', N'U') IS NULL
              CREATE TABLE chat_friend_group(
                  Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  GroupName nvarchar(50) NOT NULL,
                  OwnerId nvarchar(50) NOT NULL
              )",
            @"IF OBJECT_ID(N'chat_friendship', N'U') IS NULL
              CREATE TABLE chat_friendship(
                  Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  OwnerId nvarchar(50) NOT NULL,
                  FriendId nvarchar(50) NOT NULL,
                  FriendGroupId int NOT NULL
              )",
            @"IF OBJECT_ID(N'chat_group_member', N'U') IS NULL
              CREATE TABLE chat_group_member(
                  Id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  GroupId nvarchar(50) NOT NULL,
                  UserId nvarchar(50) NOT NULL
              )",
            @"IF OBJECT_ID(N'chat_message', N'U') IS NULL
              CREATE TABLE chat_message(
                  Id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  FromUserId nvarchar(50) NOT NULL,
                  ToType nvarchar(10) NOT NULL,
                  ToId nvarchar(50) NOT NULL,
                  Content nvarchar(max) NULL,
                  CreatedAt datetime2 NOT NULL
              )"
        };

        foreach (var sql in statements)
        {
            db.Database.ExecuteSqlRaw(sql);
        }

        // 索引（幂等创建）
        var indexes = new[]
        {
            "CREATE UNIQUE INDEX IX_chat_shadow_user_Username ON chat_shadow_user(Username)",
            "CREATE INDEX IX_chat_friend_group_OwnerId ON chat_friend_group(OwnerId)",
            "CREATE UNIQUE INDEX IX_chat_friendship_Owner_Friend ON chat_friendship(OwnerId, FriendId)",
            "CREATE UNIQUE INDEX IX_chat_group_member_Group_User ON chat_group_member(GroupId, UserId)",
            "CREATE INDEX IX_chat_message_To ON chat_message(ToType, ToId, CreatedAt)"
        };

        foreach (var indexSql in indexes)
        {
            var indexName = indexSql.Substring(indexSql.IndexOf("INDEX ", StringComparison.Ordinal) + 6);
            indexName = indexName[..indexName.IndexOf(' ')];
            var tableName = indexSql[(indexSql.IndexOf(" ON ", StringComparison.Ordinal) + 4)..];
            tableName = tableName[..tableName.IndexOf('(')];

            try
            {
                db.Database.ExecuteSqlRaw($@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = '{indexName}' AND object_id = OBJECT_ID('{tableName}'))
    {indexSql}");
            }
            catch
            {
                // 索引已存在或数据重复时忽略，不影响启动
            }
        }
    }
}
