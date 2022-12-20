namespace Moby.Services;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

public interface IMobyLogger
{
    public void SetMinimalLogLevel(LogLevel logLevel);

    public bool IsEnabled(LogLevel logLevel);

    public Task LogAsync(LogLevel logLevel, Exception? exception, string message);

    public Task LogImportantAsync(string message);

    public Task LogTraceAsync(string message);

    public Task LogDebugAsync(string message);

    public Task LogInformationAsync(string message);

    public Task LogWarningAsync(Exception? exception, string message);

    public Task LogErrorAsync(Exception? exception, string message);

    public Task LogCriticalAsync(Exception? exception, string message);
}

public sealed class MobyLogger : IMobyLogger
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly SocketTextChannel _channel;
    private LogLevel _minimumLogLevel;

    public MobyLogger(DiscordSocketClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
        _channel = _client.GetGuild(ulong.Parse(_config["serverid"])).GetTextChannel(Moby.LogsChannelId);
        _minimumLogLevel = LogLevel.Trace;
    }

    public void SetMinimalLogLevel(LogLevel logLevel) => _minimumLogLevel = logLevel;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel;

    public async Task LogAsync(LogLevel logLevel, Exception? exception, string message)
    {
        if (!IsEnabled(logLevel)) return;

        string logMessage = $"{Moby.LogLevels[logLevel].Item2} **{logLevel}:** {message}";

        if (exception is null) await _channel.SendMessageAsync(logMessage);
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

            await _channel.SendFileAsync(new FileAttachment(sb.ToString().ToStream(), "exception-file.txt"),
                logMessage, embed: MobyEmbeds.GetLog(logLevel, exception));
        }
    }

    public async Task LogImportantAsync(string message)
        => await _channel.SendMessageAsync($"{new Emoji("❗")} **Important:** {message}");

    public async Task LogTraceAsync(string message)
        => await LogAsync(LogLevel.Trace, null, message);

    public async Task LogDebugAsync(string message)
        => await LogAsync(LogLevel.Debug, null, message);

    public async Task LogInformationAsync(string message)
        => await LogAsync(LogLevel.Information, null, message);

    public async Task LogWarningAsync(Exception? exception, string message)
        => await LogAsync(LogLevel.Warning, exception, message);

    public async Task LogErrorAsync(Exception? exception, string message)
        => await LogAsync(LogLevel.Error, exception, message);

    public async Task LogCriticalAsync(Exception? exception, string message)
        => await LogAsync(LogLevel.Critical, exception, message);
}