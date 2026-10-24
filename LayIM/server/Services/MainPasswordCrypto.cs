using System.Security.Cryptography;
using System.Text;

namespace LayimChat.Services;

/// <summary>
/// 主系统密码算法兼容（WaterCloud：DES + MD5）。
/// sys_userlogon.F_UserPassword = md5( DES( md5(明文, 32位小写), F_UserSecretkey ).小写, 32位小写 ).小写
/// 复制自主系统 WaterCloud.Code 的 DESEncrypt / Md5，避免聊天服务引用整个主系统程序集。
/// </summary>
public static class MainPasswordCrypto
{
    /// <summary>校验主系统账号密码</summary>
    public static bool Verify(string plainPassword, string storedPassword, string secretKey)
    {
        if (string.IsNullOrEmpty(storedPassword) || string.IsNullOrEmpty(secretKey))
        {
            return false;
        }

        try
        {
            var md5Password = Md5Hex32(plainPassword);
            // 主系统流程：md5( DES( md5(明文) , secretkey ).ToLower() )，注意 DES 结果参与最终 MD5 前必须转小写
            var encrypted = DesEncryptToHex(md5Password, secretKey).ToLowerInvariant();
            var computed = Md5Hex32(encrypted);
            return string.Equals(computed, storedPassword.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string Md5Hex32(string text)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var data = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
        var sb = new StringBuilder();
        foreach (var b in data)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>与主系统 DESEncrypt.Encrypt(text, key) 等价：密钥取 MD5(key) 的前 8 位十六进制字符</summary>
    private static string DesEncryptToHex(string text, string key)
    {
        var desKey = Md5Hex32(key).Substring(0, 8).ToUpperInvariant();
        using var des = DES.Create();
        des.Key = Encoding.ASCII.GetBytes(desKey);
        des.IV = Encoding.ASCII.GetBytes(desKey);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
        {
            var input = Encoding.UTF8.GetBytes(text);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
        }
        var sb = new StringBuilder();
        foreach (var b in ms.ToArray())
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}
