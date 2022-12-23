namespace Moby;

using Discord;
using Microsoft.Extensions.Logging;

public static class MobyEmbeds
{
    public static Embed GetLog(LogLevel logLevel, Exception? exception)
    {
        var builder = new EmbedBuilder().WithColor(Moby.LogLevels[logLevel].Item1);

        if (exception is null)
        {
            builder.AddField("Current Time", DateTimeOffset.Now.ToString("F"));
        }
        else
        {
            builder.WithTitle(exception.Message)
                .AddField("Exception type", exception.GetType().FullName ?? "Unknown")
                .AddField("Source", exception.Source ?? "Unknown")
                .WithCurrentTimestamp();
        }

        return builder.Build();
    }
}