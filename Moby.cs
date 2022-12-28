namespace Moby;

using Discord;
using Microsoft.Extensions.Logging;

public static class Moby
{
    public const string LogoNormalUrl = "https://i.imgur.com/Za1cXgs.png";
    public const string LogoRoundUrl = "https://i.imgur.com/0OuzsfG.png";
    public const string ImageNotFound = "https://i.imgur.com/excUKal.png";

    public const ulong LogsChannelId = 1053392480034357432;
    public const ulong OwnerCommandsChannelId = 1053399251138396190;
    public const ulong RuntimeCommandsChannelId = 1053399158876283003;

    public static Color Color { get; }

    public static IDictionary<LogLevel, Tuple<Color, Emoji>> LogLevels { get; }

    static Moby()
    {
        Color = new Color(34, 141, 255);

        LogLevels = new Dictionary<LogLevel, Tuple<Color, Emoji>>()
        {
            { LogLevel.Trace, new Tuple<Color, Emoji>(new Color(240, 240, 240), new Emoji("👁️")) },
            { LogLevel.Debug, new Tuple<Color, Emoji>(new Color(200, 200, 200), new Emoji("🕸️")) },
            { LogLevel.Information, new Tuple<Color, Emoji>(Color, new Emoji("ℹ️")) },
            { LogLevel.Warning, new Tuple<Color, Emoji>(new Color(255, 229, 33), new Emoji("⚠️")) },
            { LogLevel.Error, new Tuple<Color, Emoji>(new Color(212, 0, 0), new Emoji("⛔")) },
            { LogLevel.Critical, new Tuple<Color, Emoji>(new Color(171, 12, 76), new Emoji("☢️")) }
        };
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}