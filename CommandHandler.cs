namespace Moby;

using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using global::Moby.Services;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Node;
using Discord.Interactions;

public sealed class CommandHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly IMobyLogger _mobyLogger;
    private readonly LavaNode _lavaNode;

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, InteractionService service,
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
        _client.Ready += OnClientReadyAsync;

        await Client.WaitForReadyAsync(cancellationToken);
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        await _mobyLogger.LogImportantAsync("Ready Setup finished");
        await _mobyLogger.LogImportantAsync("Now Up and Running");
    }

    private async Task OnClientReadyAsync()
    {
        _mobyLogger.SetGuild(_client.GetGuild(ulong.Parse(_config["serverid"])));

        await _mobyLogger.LogImportantAsync("Ready Setup is now executing");

        await SetStatusAsync();

        await ConnectToLavaNodeAsync();
    }

    private async Task SetStatusAsync()
    {
        await _mobyLogger.LogTraceAsync($"Set Status: {UserStatus.Online} and Game: **whale noises** - Activity Type: **{ActivityType.Listening}**");

        await _client.SetStatusAsync(UserStatus.Online);
        await _client.SetGameAsync("whale noises", null, ActivityType.Listening);
    }

    private async Task ConnectToLavaNodeAsync()
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