﻿namespace Moby;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using global::Moby.Services;
using Microsoft.Extensions.Configuration;
using System;

public sealed class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IServiceProvider _provider;
    private readonly IConfiguration _config;
    private readonly ConsoleLogger _console;
    private readonly IMobyLogger _logger;

    public InteractionHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider, IConfiguration config, ConsoleLogger console, IMobyLogger logger)
    {
        _client = client;
        _service = service;
        _provider = provider;
        _config = config;
        _console = console;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        _client.InteractionCreated += HandleInteractionAsync;

        _client.SelectMenuExecuted += SelectMenuExecutedAsync;
        _client.ButtonExecuted += ButtonExecutedAsync;
        _client.ModalSubmitted += ModalSubmittedAsync;
        
        _service.SlashCommandExecuted += SlashCommandExecutedAsync;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
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

    private async Task SelectMenuExecutedAsync(SocketMessageComponent msgc)
    {
        _console.LogDebug($"Select Menu executed for: {msgc.GuildId} with Custom Id: {msgc.Data.CustomId}");

        var value = msgc.Data.Values.First();

        switch (msgc.Data.CustomId)
        {
            case Moby.ContactMenuCId:

                switch (value)
                {
                    case Moby.ContactMenuIdeaCId: await msgc.RespondWithModalAsync(MobyUtil.GetIdeaModal()); break;

                    case Moby.ContactMenuFeedbackCId: await msgc.RespondWithModalAsync(MobyUtil.GetFeedbackModal()); break;

                    case Moby.ContactMenuBugCId: await msgc.RespondWithModalAsync(MobyUtil.GetBugModal()); break;
                }

                await msgc.DeleteOriginalResponseAsync();

                break;
        }
    }

    private async Task ButtonExecutedAsync(SocketMessageComponent msgc)
    {
        _console.LogDebug($"Button executed for: {msgc.GuildId} with Custom Id: {msgc.Data.CustomId}");

        switch (msgc.Data.CustomId)
        {
            case Moby.DenyInvitationButtonCId:

                await msgc.Message.DeleteAsync();

                await msgc.DeferAsync(ephemeral: true);

                break;
        }
    }

    private async Task ModalSubmittedAsync(SocketModal modal)
    {
        _console.LogDebug($"Modal submitted for: {modal.GuildId} with Custom Id: {modal.Data.CustomId}");

        await modal.DeferAsync(ephemeral: true);

        Embed? embed;

        switch (modal.Data.CustomId)
        {
            case Moby.AnnouncementModalCId:

                embed = MobyUtil.GetAnnouncementEmbed(_client.CurrentUser, modal.Data.Components.ToArray());

                int successfullySent = 0;

                foreach (var guild in _client.Guilds)
                {
                    successfullySent += await guild.TrySendAnnouncement(embed) ? 1 : 0;
                }

                await modal.FollowupAsync($"Announcement was sent successfully to **{successfullySent}** servers out of **{_client.Guilds.Count}**", ephemeral: true);

                break;

            default:

                embed = modal.Data.CustomId switch
                {
                    Moby.IdeaModalCId => MobyUtil.GetIdeaModalEmbed(modal.Data.Components.ToArray(), modal.GuildId ?? 0),
                    Moby.FeedbackModalCId => MobyUtil.GetFeedbackModalEmbed(modal.Data.Components.ToArray(), modal.GuildId ?? 0),
                    Moby.BugModalCId => MobyUtil.GetBugModalEmbed(modal.Data.Components.ToArray(), modal.GuildId ?? 0),
                    _ => null
                };

                if (embed is null)
                {
                    _console.LogError("Couldn't get the correct embed for a modal", null);

                    await _logger.LogErrorAsync(null, "Couldn't get the correct embed for a modal");

                    await modal.FollowupAsync("Something went wrong when sending your message :(", ephemeral: true);

                    return;
                }

                await _client.GetGuild(Convert.ToUInt64(_config["serverid"])).GetTextChannel(Moby.ContactChannelId).SendMessageAsync(embed: embed);

                await modal.FollowupAsync("Your message was sent successfully to my creator\nThanks for helping him to make me better :)", ephemeral: true);

                break;
        }
    }

    private async Task SlashCommandExecutedAsync(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnknownCommand:
                    await context.Interaction.RespondAsync("Unknown command", ephemeral: true);

                    await _logger.LogWarningAsync($"Something went wrong on executing a Slash Command: {result.ErrorReason}");
                    break;

                case InteractionCommandError.BadArgs:
                    await context.Interaction.RespondAsync("Invalid number or argument", ephemeral: true);
                    break;

                case InteractionCommandError.Exception:
                    await context.Interaction.RespondAsync("Command exception: " + result.ErrorReason, ephemeral: true);

                    await _logger.LogWarningAsync($"Something went wrong on executing a Slash Command: {result.ErrorReason}");
                    break;

                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondAsync("Command could not be executed", ephemeral: true);

                    await _logger.LogWarningAsync($"Something went wrong on executing a Slash Command: {result.ErrorReason}");
                    break;

                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondAsync("Unmet Precondition: " + result.ErrorReason, ephemeral: true);
                    break;
            }
        }

        _console.LogDebug($"Slash command executed: {info?.Name} for {context.Guild.Name} {(result.IsSuccess ? "succeeded" : "failed")} {(result.IsSuccess ? "" : $" - Error: {result.ErrorReason}")}");
    }
}