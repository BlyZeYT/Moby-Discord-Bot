namespace Moby;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Victoria;
using Victoria.Node;

sealed class Program
{
    private DiscordSocketClient _client = null!;
    private IConfiguration _config = null!;
    private InteractionService _service = null!;
    private IMobyLogger _logger = null!;
    private LavaNode _lavaNode = null!;

    static Task Main() => new Program().MainAsync();

    private async Task MainAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
            .AddSingleton(config)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1000,
                LogLevel = LogSeverity.Debug
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<ConsoleLogger>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<MusicHandler>()
            .AddSingleton<IDatabase, Database>()
            .AddSingleton<IMobyLogger, MobyLogger>()
            .AddSingleton<IHttpService, HttpService>()
            .AddLavaNode(x =>
            {
                x.SelfDeaf = true;
                x.EnableResume = true;
                x.SocketConfiguration.ReconnectDelay = TimeSpan.FromSeconds(1);
                x.SocketConfiguration.BufferSize = 4096;
            }))
            .Build();
        
        await RunAsync(host);
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        _client = provider.GetRequiredService<DiscordSocketClient>();
        _config = provider.GetRequiredService<IConfigurationRoot>();
        _service = provider.GetRequiredService<InteractionService>();
        _logger = provider.GetRequiredService<IMobyLogger>();
        _lavaNode = provider.GetRequiredService<LavaNode>();

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        await provider.GetRequiredService<MusicHandler>().InitializeAsync();

        _client.Log += msg => provider.GetRequiredService<ConsoleLogger>().Log(msg);
        _service.Log += msg => provider.GetRequiredService<ConsoleLogger>().Log(msg);

        _client.Ready += OnReadyAsync;

        await _client.LoginAsync(TokenType.Bot, _config["token"]);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task OnReadyAsync()
    {
        _logger.SetMinimalLogLevel(LogLevel.Trace);
        _logger.SetGuild(_client.GetGuild(ulong.Parse(_config["serverid"]!)));

        await _logger.LogImportantAsync("Ready Setup is now executing");

        await SetStatusAsync();
        await SetGameAsync();
        await ConnectToLavaNodeAsync();

        if (Moby.IsDebug()) await _service.RegisterCommandsToGuildAsync(ulong.Parse(_config["serverid"]!), true);
        else await _service.RegisterCommandsGloballyAsync(true);

        await _logger.LogImportantAsync("Bot Startup completed");
    }

    private async Task SetStatusAsync()
    {
        await _client.SetStatusAsync(UserStatus.Online);

        await _logger.LogTraceAsync("Set Bot activity to " + _client.Status);
    }

    private async Task SetGameAsync()
    {
        await _client.SetGameAsync("whale noises", null, ActivityType.Listening);

        await _logger.LogTraceAsync("Set Bot game"); 
    }

    private async Task ConnectToLavaNodeAsync()
    {
        if (!_lavaNode.IsConnected)
        {
            await _logger.LogDebugAsync("Attempting to connect to LavaNode");

            try
            {
                await _lavaNode.ConnectAsync();

                if (_lavaNode.IsConnected) await _logger.LogImportantAsync("LavaNode connected successfully");
                else await _logger.LogCriticalAsync(null, "Failed to connect to LavaNode");
            }
            catch (Exception ex)
            {
                await _logger.LogCriticalAsync(ex, "Something went wrong connecting to LavaNode");
            }
        }
    }
}