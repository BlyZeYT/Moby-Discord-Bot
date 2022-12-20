namespace Moby;

using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using global::Moby.Services;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Node;

public sealed class CommandHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _service;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly IMobyLogger _mobyLogger;
    private readonly LavaNode _lavaNode;

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
        IConfiguration config, ILogger<DiscordClientService> logger, IMobyLogger mobyLogger, LavaNode lavaNode) : base(client, logger)
    {
        _provider = provider;
        _client = client;
        _service = service;
        _config = config;
        _logger = logger;
        _mobyLogger = mobyLogger;
        _lavaNode = lavaNode;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _client.Ready += async () => await _mobyLogger.LogImportantAsync("Ready Setup is now executing");
        _client.Ready += SetStatusAsync;
        _client.Ready += ConnectToLavalinkAsync;

        await Client.WaitForReadyAsync(cancellationToken);
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        await _mobyLogger.LogImportantAsync("Ready Setup finished");
        await _mobyLogger.LogImportantAsync("Now Up and Running");
    }

    private async Task SetStatusAsync()
    {
        await _mobyLogger.LogTraceAsync($"Set Status: {UserStatus.Online} and Game: **whale noises** - Activity Type: **{ActivityType.Listening}**");

        await _client.SetStatusAsync(UserStatus.Online);
        await _client.SetGameAsync("whale noises", null, ActivityType.Listening);
    }

    private async Task ConnectToLavalinkAsync()
    {
        if (!_lavaNode.IsConnected)
        {
            await _mobyLogger.LogDebugAsync("Attempting to connect LavaNode");

            await _lavaNode.ConnectAsync();

            if (_lavaNode.IsConnected) await _mobyLogger.LogDebugAsync("LavaNode connection was established successfully");
            else await _mobyLogger.LogCriticalAsync(null, "Failed to establish the LavaNode connection");
        }
    }
}