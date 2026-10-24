namespace LayimChat.Services;

/// <summary>头像处理：未设置头像时回落到 LayIM 默认头像</summary>
public static class Avatars
{
    public const string Default = "/dist/layim-assets/images/default.png";

    public static string OrDefault(string? avatar)
        => string.IsNullOrWhiteSpace(avatar) ? Default : avatar;
}
