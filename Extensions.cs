namespace Moby;

using System.Text;

public static class Extensions
{
    public static Stream ToStream(this string str, Encoding? encoding = null)
        => new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(str));
}