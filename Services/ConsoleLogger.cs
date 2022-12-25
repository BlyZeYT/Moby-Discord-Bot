namespace Moby.Services;

using Discord;

public sealed class ConsoleLogger : Logger
{
    public override async Task Log(LogMessage message)
        => await LogToConsoleAsync(this, message);

    public async Task LogDebug(string message)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Debug, "", message, null));

    public async Task LogVerbose(string message)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Verbose, "", message, null));

    public async Task LogInfo(string message)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Info, "", message, null));

    public async Task LogWarning(string message, Exception? exception = null)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Warning, "", message, exception));

    public async Task LogError(string message, Exception? exception)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Warning, "", message, exception));

    public async Task LogCritical(string message, Exception? exception)
        => await LogToConsoleAsync(this, new LogMessage(LogSeverity.Warning, "", message, exception));

    private static Task LogToConsoleAsync<T>(T logger, LogMessage message) where T : ILogger
    {
        var currentBgColor = Console.BackgroundColor;

        Console.BackgroundColor = message.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.DarkRed,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Warning => ConsoleColor.DarkYellow,
            LogSeverity.Info => ConsoleColor.Green,
            LogSeverity.Verbose => ConsoleColor.Cyan,
            LogSeverity.Debug or _ => currentBgColor
        };

        Console.Write(message.Severity);

        Console.BackgroundColor = currentBgColor;

        Console.WriteLine(": " + message.Message + " | Source: " + message.Source);

        if (message.Exception is not null) Console.WriteLine("Exception: " + message.Exception);

        return Task.CompletedTask;
    }
}

public interface ILogger
{
    public Task Log(LogMessage message);
}

public abstract class Logger : ILogger
{
    public abstract Task Log(LogMessage message);
}