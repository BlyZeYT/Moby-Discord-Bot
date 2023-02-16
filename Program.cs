namespace Moby;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Victoria;
using Victoria.Node;
using Common;

sealed class Program
{
    private DiscordSocketClient _client = null!;
    private IConfiguration _config = null!;
    private InteractionService _service = null!;
    private IDatabase _database = null!;
    private IMobyLogger _logger = null!;
    private IHttpService _http = null!;
    private LavaNode<MobyPlayer, MobyTrack> _lavaNode = null!;

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
            .AddLavaNode<MobyPlayer, MobyTrack>(x =>
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
        _database = provider.GetRequiredService<IDatabase>();
        _logger = provider.GetRequiredService<IMobyLogger>();
        _http = provider.GetRequiredService<IHttpService>();
        _lavaNode = provider.GetRequiredService<LavaNode<MobyPlayer, MobyTrack>>();

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        await provider.GetRequiredService<MusicHandler>().InitializeAsync();

        _client.Log += msg => provider.GetRequiredService<ConsoleLogger>().Log(msg);
        _service.Log += msg => provider.GetRequiredService<ConsoleLogger>().Log(msg);

        _client.Ready += OnReadyAsync;

        _client.JoinedGuild += JoinedGuildAsync;
        _client.LeftGuild += LeftGuildAsync;

        _client.Disconnected += OnDisconnectedAsync;

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
        await SetColorQuizSetupAsync();
        await ConnectToLavaNodeAsync();

        if (Moby.IsDebug()) await _service.RegisterCommandsToGuildAsync(Convert.ToUInt64(_config["serverid"]), true);
        else
        {
            await _service.AddModulesToGuildAsync(Convert.ToUInt64(_config["serverid"]), true, _service.Modules.Where(x => x.Name == Moby.OnlyMobyGuildModule).ToArray());

            await _service.AddModulesGloballyAsync(true, _service.Modules.Where(x => x.Name != Moby.OnlyMobyGuildModule).ToArray());
        }

        await _logger.LogImportantAsync("Bot Startup completed");
    }

    private async Task SetStatusAsync()
    {
        await _client.SetStatusAsync(UserStatus.Online);

        await _logger.LogTraceAsync("Set Bot activity to " + _client.Status);
    }

    private async Task SetGameAsync()
    {
        await _client.SetGameAsync("🐳 noises", null, ActivityType.Listening);

        await _logger.LogTraceAsync("Set Bot game"); 
    }

    private async Task SetColorQuizSetupAsync()
    {
        _logger.LogDebugAsync("Started to initialize the Color Quiz data");

        var colors = await _http.GetColorQuizInfoAsync();

        Random.Shared.Shuffle(colors);

        Moby.ColorQuizInfo = colors;

        if (Moby.ColorQuizInfo.Length == 0) _logger.LogErrorAsync(null, "Error to initialize Color Quiz data successfully");
        else _logger.LogDebugAsync("Initialized Color Quiz data successfully");
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

    private async Task JoinedGuildAsync(SocketGuild guild)
    {
        await _database.AddGuildAsync(guild.Id);

        await _client.GetGuild(Convert.ToUInt64(_config["serverid"]))
            .GetTextChannel(Moby.InformationChannelId)
            .SendMessageAsync("**Joined a server** 🥳", embed: MobyUtil.GetServerInfoEmbed(guild));
    }

    private async Task LeftGuildAsync(SocketGuild guild)
    {
        await _database.RemoveGuildAsync(guild.Id);

        await _client.GetGuild(Convert.ToUInt64(_config["serverid"]))
            .GetTextChannel(Moby.InformationChannelId)
            .SendMessageAsync("**Lefted a server** 😢", embed: MobyUtil.GetServerInfoEmbed(guild));
    }

    private async Task OnDisconnectedAsync(Exception arg)
    {
        await _logger.LogInformationAsync("Disconnected!");

        if (arg is not null) await _logger.LogCriticalAsync(arg, "Disconnection Error");
    }
}