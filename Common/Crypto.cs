namespace Moby.Common;

using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

public static class Crypto
{
    public static string ToBase64(string plainText)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    public static string FromBase64(string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    public static string ToMD5(string plainText)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            return Convert.ToHexString(hashBytes);
        }
    }

    public static string ToSHA1(string plainText)
    {
        using (var sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            return Convert.ToHexString(hashBytes);
        }
    }

    public static string ToSHA256(string plainText)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            return Convert.ToHexString(hashBytes);
        }
    }

    public static string ToSHA384(string plainText)
    {
        using (var sha384 = SHA384.Create())
        {
            byte[] hashBytes = sha384.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            return Convert.ToHexString(hashBytes);
        }
    }

    public static string ToSHA512(string plainText)
    {
        using (var sha512 = SHA512.Create())
        {
            byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            return Convert.ToHexString(hashBytes);
        }
    }
}