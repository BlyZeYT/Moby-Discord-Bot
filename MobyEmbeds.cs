namespace Moby;

using Discord;
using Microsoft.Extensions.Logging;

public static class MobyEmbeds
{
    public static Embed GetLog(LogLevel logLevel, Exception exception)
    {
        return new EmbedBuilder()
            .WithColor(Moby.LogLevels[logLevel].Item1)
            .WithTitle(exception.Message)
            .AddField("Exception type", exception.GetType().FullName)
            .AddField("Source", exception.Source ?? "")
            .WithCurrentTimestamp()
            .Build();
    }
}