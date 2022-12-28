namespace Moby;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using global::Moby.Services;

public sealed class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IServiceProvider _provider;
    private readonly ConsoleLogger _console;
    private readonly IMobyLogger _logger;

    public InteractionHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider, ConsoleLogger console, IMobyLogger logger)
    {
        _client = client;
        _service = service;
        _provider = provider;
        _console = console;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        _client.InteractionCreated += HandleInteractionAsync;

        _service.SlashCommandExecuted += SlashCommandExecutedAsync;
        _service.ContextCommandExecuted += ContextCommandExecutedAsync;
        _service.ComponentCommandExecuted += ComponentCommandExecutedAsync;
    }

    private async Task ComponentCommandExecutedAsync(ComponentCommandInfo info, IInteractionContext context, IResult result)
    {
        if (context.Channel.GetChannelType() is ChannelType.DM) return;

        _console.LogDebug($"Component command executed: {info.Name} for {context.Guild.Name} {(result.IsSuccess ? "succeeded" : "failed")} {(result.IsSuccess ? "" : $" - Error: {result.ErrorReason}")}");
    }

    private async Task ContextCommandExecutedAsync(ContextCommandInfo info, IInteractionContext context, IResult result)
    {
        if (context.Channel.GetChannelType() is ChannelType.DM) return;

        _console.LogDebug($"Context command executed: {info.Name} for {context.Guild.Name} {(result.IsSuccess ? "succeeded" : "failed")} {(result.IsSuccess ? "" : $" - Error: {result.ErrorReason}")}");
    }

    private async Task SlashCommandExecutedAsync(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        if (context.Channel.GetChannelType() is ChannelType.DM) return;

        _console.LogDebug($"Slash command executed: {info.Name} for {context.Guild.Name} {(result.IsSuccess ? "succeeded" : "failed")} {(result.IsSuccess ? "" : $" - Error: {result.ErrorReason}")}");
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction.Channel.GetChannelType() is ChannelType.DM) return;

        _console.LogDebug($"Handling interaction in Guild: {interaction.GuildId}");

        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _service.ExecuteCommandAsync(context, _provider);
        }
        catch (Exception ex)
        {
            _console.LogError("Error when handling an interaction", ex);

            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}