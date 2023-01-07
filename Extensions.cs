namespace Moby;

using Discord;
using Discord.WebSocket;
using global::Moby.Common;
using System.Globalization;
using System.Text;

public static class Extensions
{
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

    public static Tuple<double, double, double> ToHsv(this Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;

        double min = Math.Min(Math.Min(r, g), b);
        double max = Math.Max(Math.Max(r, g), b);

        double delta = max - min;

        if (max == 0 || delta == 0) return Tuple.Create(0.0, 0.0, 0.0);

        double h;

        if (r == max)
        {
            h = (g - b) / delta;
        }
        else if (g == max)
        {
            h = 2 + (b - r) / delta;
        }
        else
        {
            h = 4 + (r - g) / delta;
        }

        h *= 60;
        if (h < 0)
        {
            h += 360;
        }

        return Tuple.Create(h, delta / max, max);
    }

    public static Tuple<double, double, double> ToHsl(this Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;

        double min = Math.Min(Math.Min(r, g), b);
        double max = Math.Max(Math.Max(r, g), b);
        double l = (max + min) / 2;

        if (max == min) return Tuple.Create(0.0, 0.0, l);

        double d = max - min;
        double s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        double h;

        if (r == max)
        {
            h = (g - b) / d + (g < b ? 6 : 0);
        }
        else if (g == max)
        {
            h = (b - r) / d + 2;
        }
        else
        {
            h = (r - g) / d + 4;
        }

        h /= 6;

        return Tuple.Create(h, s, l);
    }

    public static Tuple<double, double, double, double> ToCmyk(this Color color)
    {
        double c = 1 - (color.R / 255.0);
        double m = 1 - (color.G / 255.0);
        double y = 1 - (color.B / 255.0);
        double k = Math.Min(Math.Min(c, m), y);

        return k == 1 ? Tuple.Create(0.0, 0.0, 0.0, k) : Tuple.Create((c - k) / (1 - k), (m - k) / (1 - k), (y - k) / (1 - k), k);
    }

    public static Tuple<double, double, double> ToYCbCr(this Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;

        return Tuple.Create(
            0.299 * r + 0.587 * g + 0.114 * b,
            128 - 0.168736 * r - 0.331264 * g + 0.5 * b,
            128 + 0.5 * r - 0.418688 * g - 0.081312 * b);
    }

    public static string GetFormatted(this double d)
        => d.ToString("f2", new NumberFormatInfo() { NumberDecimalSeparator = "." });
}