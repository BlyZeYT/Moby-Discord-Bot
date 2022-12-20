namespace Moby;

using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using global::Moby.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using Victoria;

sealed class Startup
{
    static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration(x =>
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

                x.AddConfiguration(config);
            })
            .ConfigureLogging(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Error);
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 1000,
                    GatewayIntents = GatewayIntents.All
                };

                config.Token = context.Configuration["token"];
            })
            .UseCommandService((_, config) => config = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Error
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IMobyLogger, MobyLogger>();
                services.AddSingleton<IDatabase, Database>();
                services.AddHostedService<CommandHandler>();
                services.AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                    x.EnableResume = true;
                    x.SocketConfiguration.ReconnectDelay = TimeSpan.FromSeconds(1);
                    x.SocketConfiguration.BufferSize = 4096;
                });
            })
            .UseConsoleLifetime()
            .Build();

        MySqlConnectorLogManager.Provider = new ConsoleLoggerProvider(MySqlConnectorLogLevel.Error);

        using (host)
        {
            await host.RunAsync();
        }
    }
}