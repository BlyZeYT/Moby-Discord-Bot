namespace Moby;

using Discord;
using Discord.WebSocket;
using global::Moby.Common;
using System.Globalization;
using System.Linq;
using System.Text;

public static class Extensions
{
    public static string DiscordFormat(this string str)
        => str.Trim().Replace('`', '\'').Replace("||", "");

    public static Stream ToStream(this string str, Encoding? encoding = null)
        => new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(str));

    public static string RemoveAfter(this string str, int index, char character)
        => str.Length <= index ? str : str.Remove(str.IndicesOf(character).Where(x => x < index).Max());

    public static IEnumerable<int> IndicesOf(this string str, char character)
    {
        for (int i = str.IndexOf(character); i > -1; i = str.IndexOf(character, i + 1))
        {
            yield return i;
        }
    }

    public static async ValueTask<bool> TrySendAnnouncement(this SocketGuild guild, Embed announcement)
    {
        foreach (var channel in guild.TextChannels)
        {
            if (guild.CurrentUser.GetPermissions(channel).SendMessages)
            {
                await channel.SendMessageAsync(embed: announcement);

                return true;
            }
        }

        return false;
    }

    public static SocketTextChannel? GetFirstTextChannel(this SocketGuild guild)
    {
        foreach (var channel in guild.TextChannels)
        {
            if (guild.CurrentUser.GetPermissions(channel).SendMessages)
            {
                return channel;
            }
        }

        return null;
    }

    public static IEnumerable<Avatar> GetAllAvatarResolutions(this IUser user, ImageFormat imageFormat)
    {
        string? url;

        for (ushort size = 16; size <= 2048; size *= 2)
        {
            url = user.GetAvatarUrl(imageFormat, size);

            if (url is null) continue;

            yield return new Avatar(url, size, imageFormat);
        }
    }

    public static string ToHex(this Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    public static HSV ToHsv(this Color color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);

        var v = max;
        var delta = max - min;

        if (max == 0 || delta == 0) return new(0, 0, 0);

        var s = delta / max;
        double h;

        if (r == max) h = (g - b) / delta;
        else if (g == max) h = 2 + (b - r) / delta;
        else h = 4 + (r - g) / delta;

        h *= 60;
        if (h < 0) h += 360;

        return new(h, s * 100, v * 100);
    }

    public static HSL ToHsl(this Color color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);

        var l = (min + max) / 2;
        var delta = max - min;

        if (max == min) return new(0, 0, l);

        double h;

        var s = l < 0.5 ? delta / (max + min) : delta / (2 - max - min);

        if (r == max) h = (g - b) / delta;
        else if (g == max) h = 2 + (b - r) / delta;
        else h = 4 + (r - g) / delta;

        h *= 60;
        if (h < 0) h += 360;

        return new(h, s * 100, l * 100);
    }

    public static CMYK ToCmyk(this Color color)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var k = 1 - Math.Max(r, Math.Max(g, b));
        var c = (1 - r - k) / (1 - k);
        var m = (1 - g - k) / (1 - k);
        var y = (1 - b - k) / (1 - k);

        return new(c * 100, m * 100, y * 100, k * 100);
    }

    public static YCbCr ToYCbCr(this Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;

        var y = (int)(0.299 * r + 0.587 * g + 0.114 * b);
        var cb = (int)(0.564 * (b - y) + 128);
        var cr = (int)(0.713 * (r - y) + 128);

        return new(y, cb, cr);
    }

    public static double Round(this double d, int digits)
        => Math.Round(d, digits);

    public static Color? TryGetColor(this string? hex)
    {
        return string.IsNullOrWhiteSpace(hex) ? null : hex.Length is 6 or 7
            ? int.TryParse(hex.Replace("#", ""), NumberStyles.HexNumber, null, out int number) ? new Color((uint)number) : null : null;
    }

    public static void Shuffle<T>(this Random rng, T[] array)
    {
        int n = array.Length;

        while (n > 1)
        {
            int k = rng.Next(n--);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }

    public static T Random<T>(this IEnumerable<T> sequence)
    {
        if (!sequence.TryGetNonEnumeratedCount(out var count))
        {
            count = sequence.Count();
        }

        return sequence.ElementAt(System.Random.Shared.Next(0, count));
    }

    public static string GetString(this HashMethod method)
    {
        return method switch
        {
            HashMethod.Base64Encode => "Base64 Encoding",
            HashMethod.Base64Decode => "Base64 Decoding",
            HashMethod.MD5 => "MD5 Hashing",
            HashMethod.SHA1 => "SHA1 Hashing",
            HashMethod.SHA256 => "SHA256 Hashing",
            HashMethod.SHA384 => "SHA384 Hashing",
            HashMethod.SHA512 => "SHA512 Hashing",
            _ => ""
        };
    }

    public static bool IsDecode(this HashMethod method)
        => method is HashMethod.Base64Decode;
}