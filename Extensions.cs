namespace Moby;

using Discord;
using Discord.WebSocket;
using Common;
using System.Globalization;
using System.Linq;
using System.Text;
using DeepL;
using Enums;
using Victoria.Responses.Search;
using Victoria.Player;
using Victoria;
using System;

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

    public static string GetModifiedText(this TextModification mod, string text)
    {
        return mod switch
        {
            TextModification.Italic => $"*{text}*",
            TextModification.Bold => $"**{text}**",
            TextModification.BoldItalic => $"***{text}***",
            TextModification.Underline => $"__{text}__",
            TextModification.UnderlineItalic => $"__*{text}*__",
            TextModification.UnderlineBold => $"__**{text}**__",
            TextModification.UnderlineBoldItalic => $"__***{text}***__",
            TextModification.Strikethrough => $"~~{text}~~",
            TextModification.Spoiler => $"||{text}||",
            TextModification.SingleCode => $"`{text}`",
            TextModification.MultiCode => $"```\n{text}\n```",
            TextModification.SingleQuote => $"> {text}",
            TextModification.MultiQuote => $">>> {text}",
            _ => text
        };
    }

    public static string GetLanguageCode(this Language language)
    {
        return language switch
        {
            Language.AmericanEnglish => LanguageCode.EnglishAmerican,
            Language.BrazilianPortuguese => LanguageCode.PortugueseBrazilian,
            Language.BritishEnglish => LanguageCode.EnglishBritish,
            Language.Bulgarian => LanguageCode.Bulgarian,
            Language.Chinese => LanguageCode.Chinese,
            Language.Czech => LanguageCode.Czech,
            Language.Danish => LanguageCode.Danish,
            Language.Dutch => LanguageCode.Dutch,
            Language.English => LanguageCode.English,
            Language.EuropeanPortuguese => LanguageCode.PortugueseEuropean,
            Language.French => LanguageCode.French,
            Language.German => LanguageCode.German,
            Language.Greek => LanguageCode.Greek,
            Language.Hungarian => LanguageCode.Hungarian,
            Language.Indonesian => LanguageCode.Indonesian,
            Language.Italian => LanguageCode.Italian,
            Language.Japanese => LanguageCode.Japanese,
            Language.Polish => LanguageCode.Polish,
            Language.Portuguese => LanguageCode.Portuguese,
            Language.Romanian => LanguageCode.Romanian,
            Language.Russian => LanguageCode.Russian,
            Language.Spanish => LanguageCode.Spanish,
            Language.Swedish => LanguageCode.Swedish,
            Language.Turkish => LanguageCode.Turkish,
            Language.Ukrainian => LanguageCode.Ukrainian,
            _ => ""
        };
    }

    public static Language GetLanguage(this string str)
    {
        return str switch
        {
            LanguageCode.EnglishAmerican => Language.AmericanEnglish,
            LanguageCode.PortugueseBrazilian => Language.BrazilianPortuguese,
            LanguageCode.EnglishBritish => Language.BritishEnglish,
            LanguageCode.Bulgarian => Language.Bulgarian,
            LanguageCode.Chinese => Language.Chinese,
            LanguageCode.Czech => Language.Czech,
            LanguageCode.Danish => Language.Danish,
            LanguageCode.Dutch => Language.Dutch,
            LanguageCode.English => Language.English,
            LanguageCode.PortugueseEuropean => Language.EuropeanPortuguese,
            LanguageCode.French => Language.French,
            LanguageCode.German => Language.German,
            LanguageCode.Greek => Language.Greek,
            LanguageCode.Hungarian => Language.Hungarian,
            LanguageCode.Indonesian => Language.Indonesian,
            LanguageCode.Italian => Language.Italian,
            LanguageCode.Japanese => Language.Japanese,
            LanguageCode.Polish => Language.Polish,
            LanguageCode.Portuguese => Language.Portuguese,
            LanguageCode.Romanian => Language.Romanian,
            LanguageCode.Russian => Language.Russian,
            LanguageCode.Spanish => Language.Spanish,
            LanguageCode.Swedish => Language.Swedish,
            LanguageCode.Turkish => Language.Turkish,
            LanguageCode.Ukrainian => Language.Ukrainian,
            _ => new Language()
        };
    }

    public static string GetFormattedString(this Language language)
    {
        var langStr = language.ToString();

        if (string.IsNullOrWhiteSpace(langStr)) return "";

        StringBuilder sb = new StringBuilder(langStr.Length * 2);

        sb.Append(langStr[0]);

        for (int i = 1; i < langStr.Length; i++)
        {
            if (char.IsUpper(langStr[i]) && langStr[i - 1] != ' ') sb.Append(' ');

            sb.Append(langStr[i]);
        }

        return sb.ToString();
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

    public static async ValueTask<IUserMessage?> TrySendMessageAsync(this IUser user, string? text = null, bool isTTS = false,
        Embed? embed = null, RequestOptions? requestOptions = null, AllowedMentions? allowedMentions = null,
        MessageComponent? messageComponent = null, Embed[]? embeds = null)
    {
        try
        {
            return await user.SendMessageAsync(text, isTTS, embed, requestOptions, allowedMentions, messageComponent, embeds);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool IsInvalidVoiceState(this IVoiceState? voiceState)
        => voiceState?.VoiceChannel is null || voiceState.IsDeafened || voiceState.IsSelfDeafened;

    public static string GetFormattedQuery(this MusicSource source, string query)
    {
        return source switch
        {
            MusicSource.Url => IsHttpOrHttpsUrl(query) ? query : "http://" + query,
            MusicSource.Twitch => "https://www.twitch.tv/" + query,
            _ => query
        };
    }

    public static SearchType GetSearchType(this MusicSource source)
    {
        return source switch
        {
            MusicSource.YouTube => SearchType.YouTube,
            MusicSource.YouTubeMusic => SearchType.YouTubeMusic,
            MusicSource.Soundcloud => SearchType.SoundCloud,
            _ => SearchType.Direct
        };
    }

    public static string GetString(this MusicSource source)
    {
        return source switch
        {
            MusicSource.Url => "Link",
            _ => source.ToString()
        };
    }

    public static async ValueTask<string> GetArtworkOrDefault(this LavaTrack track)
    {
        var url = await track.FetchArtworkAsync();

        return string.IsNullOrWhiteSpace(url) ? Moby.ImageNotFound : url;
    }

    public static string GetFormattedDuration(this LavaTrack track)
    {
        return track.Duration.TotalSeconds switch
        {
            var seconds when seconds < 60 => $"{track.Duration.Seconds}s",
            var seconds when seconds < 3600 => $"{track.Duration.Minutes}:{track.Duration.Seconds}m",
            _ => $"{track.Duration.Hours}:{track.Duration.Minutes}:{track.Duration.Seconds}h"
        };
    }

    private static bool IsHttpOrHttpsUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}