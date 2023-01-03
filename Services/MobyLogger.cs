namespace Moby.Services;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Text;

public interface IMobyLogger
{
    public void SetGuild(SocketGuild guild);

    public void SetMinimalLogLevel(LogLevel logLevel);

    public bool IsEnabled(LogLevel logLevel);

    public Task LogAsync(LogLevel logLevel, Exception? exception, string message);

    public Task LogImportantAsync(string message);

    public Task LogTraceAsync(string message);

    public Task LogDebugAsync(string message);

    public Task LogInformationAsync(string message);

    public Task LogWarningAsync(string message, Exception? exception = null);

    public Task LogErrorAsync(Exception? exception, string message);

    public Task LogCriticalAsync(Exception? exception, string message);
}

public sealed class MobyLogger : IMobyLogger
{
    private SocketTextChannel? _channel;
    private LogLevel _minimumLogLevel;

    public MobyLogger()
    {
        _minimumLogLevel = LogLevel.Trace;
    }

    public void SetGuild(SocketGuild guild) => _channel = guild.GetTextChannel(Moby.LogsChannelId);

    public void SetMinimalLogLevel(LogLevel logLevel) => _minimumLogLevel = logLevel;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel;

    public async Task LogAsync(LogLevel logLevel, Exception? exception, string message)
    {
        if (!IsEnabled(logLevel)) return;

        string logMessage = $"\\{Moby.LogLevels[logLevel].Item2} **{logLevel}**: {message}";

        if (exception is null)
        {
            if (_channel is not null) await _channel.SendMessageAsync(logMessage, embed: MobyUtil.GetLogEmbed(logLevel, null));
        }
        else
        {
            var sb = new StringBuilder();

            sb.Append("Exception: ");
            sb.AppendLine(exception.ToString());
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                sb.Append("Exception Message: ");
                sb.AppendLine(exception.Message);
                sb.AppendLine();
            }

            if (exception.TargetSite is not null)
            {
                sb.Append("Method: ");
                sb.AppendLine(exception.TargetSite.Name);
                sb.AppendLine();
            }

            if (exception.Data.Count > 0)
            {
                sb.AppendLine("Exception Data:");

                int iteration = 0;
                foreach (var key in exception.Data.Keys)
                {
                    iteration++;

                    sb.Append("Data ");
                    sb.Append(iteration);
                    sb.Append(": ");
                    sb.Append(key);
                    sb.Append(" - ");
                    sb.AppendLine((exception.Data[key] ?? "").ToString());
                }

                sb.AppendLine();
            }

            if (exception.InnerException is not null)
            {
                sb.Append("Inner Exception: ");
                sb.AppendLine(exception.InnerException.ToString());
                sb.AppendLine();
            }

            sb.Append("Base Exception: ");
            sb.AppendLine(exception.GetBaseException().ToString());
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                sb.Append("Exception Stack Trace: ");
                sb.AppendLine(exception.StackTrace);
            }

            if (_channel is not null)
            {
                using (var stream = sb.ToString().ToStream())
                {
                    await _channel.SendFileAsync(
                        new FileAttachment(stream, "exception-file.txt"),
                        logMessage, embed: MobyUtil.GetLogEmbed(logLevel, exception));
                }
            }
        }
    }

    public async Task LogImportantAsync(string message)
    {
        if (_channel is not null)
            await _channel.SendMessageAsync($"\\❗ **Important**: {message}", embed: MobyUtil.GetLogEmbed(LogLevel.Trace, null));
    }

    public async Task LogTraceAsync(string message)
        => await LogAsync(LogLevel.Trace, null, message);

    public async Task LogDebugAsync(string message)
        => await LogAsync(LogLevel.Debug, null, message);

    public async Task LogInformationAsync(string message)
        => await LogAsync(LogLevel.Information, null, message);

    public async Task LogWarningAsync(string message, Exception? exception = null)
        => await LogAsync(LogLevel.Warning, exception, message);

    public async Task LogErrorAsync(Exception? exception, string message)
        => await LogAsync(LogLevel.Error, exception, message);

    public async Task LogCriticalAsync(Exception? exception, string message)
        => await LogAsync(LogLevel.Critical, exception, message);
}