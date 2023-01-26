namespace Moby;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using Services;
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
    private readonly IDatabase _database;

    public InteractionHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider, IConfiguration config, ConsoleLogger console, IMobyLogger logger, IDatabase database)
    {
        _client = client;
        _service = service;
        _provider = provider;
        _config = config;
        _console = console;
        _logger = logger;
        _database = database;
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
        _console.LogDebug($"Handling interaction in Guild: {interaction.GuildId} with Type: {interaction.Type}");

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
        finally
        {
            if (interaction.Type is InteractionType.ApplicationCommand && !(interaction.User.IsBot || interaction.User.IsWebhook))
                await _database.AddScoreAsync(interaction.User.Id, 5);
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

            case Moby.CoinflipAgainCId:

                await msgc.UpdateAsync(x => x.Embed = MobyUtil.GetCoinflipEmbed(Random.Shared.Next(0, 2) == 0));
                
                break;

            case Moby.ColorQuizCorrectAnswerCId:

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\✅ Correct**\n\nYou answered: {((ButtonComponent)msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components).First(x => x.CustomId == Moby.ColorQuizCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 25);

                break;

            case Moby.ColorQuizWrongAnswerCId1 or Moby.ColorQuizWrongAnswerCId2 or Moby.ColorQuizWrongAnswerCId3:

                var colors = msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components);

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\❌ Wrong**\n\nYou answered: {((ButtonComponent)colors.First(x => x.CustomId == msgc.Data.CustomId)).Label}\nThe right answer was: {((ButtonComponent)colors.First(x => x.CustomId == Moby.ColorQuizCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 10);

                break;

            case Moby.MultipleChoiceCorrectAnswerCId:

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\✅ Correct**\n\nYou answered: {((ButtonComponent)msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components).First(x => x.CustomId == Moby.MultipleChoiceCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 25);

                break;

            case Moby.TrueOrFalseCorrectAnswerCId:

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\✅ Correct**\n\nYou answered: {((ButtonComponent)msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components).First(x => x.CustomId == Moby.TrueOrFalseCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 25);

                break;

            case Moby.TrueOrFalseIncorrectAnswerCId:

                var trueOrFalse = msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components);

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\❌ Wrong**\n\nYou answered: {((ButtonComponent)trueOrFalse.First(x => x.CustomId == msgc.Data.CustomId)).Label}\nThe right answer was: {((ButtonComponent)trueOrFalse.First(x => x.CustomId == Moby.TrueOrFalseCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 10);

                break;

            case Moby.MultipleChoiceIncorrectAnswerCId1 or Moby.MultipleChoiceIncorrectAnswerCId2 or Moby.MultipleChoiceIncorrectAnswerCId3:

                var choices = msgc.Message.Components.ElementAt(0).Components.Union(msgc.Message.Components.ElementAt(1).Components);

                await msgc.UpdateAsync(x =>
                {
                    x.Embed = msgc.Message.Embeds.First().ToEmbedBuilder().WithDescription($"**\\❌ Wrong**\n\nYou answered: {((ButtonComponent)choices.First(x => x.CustomId == msgc.Data.CustomId)).Label}\nThe right answer was: {((ButtonComponent)choices.First(x => x.CustomId == Moby.MultipleChoiceCorrectAnswerCId)).Label}").Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await _database.AddScoreAsync(msgc.User.Id, 10);

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

                await _database.AddScoreAsync(modal.User.Id, 50);

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
                    await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);

                    await _logger.LogWarningAsync($"Something went wrong on executing a Slash Command: {result.ErrorReason}");
                    break;

                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondAsync("Command could not be executed", ephemeral: true);

                    await _logger.LogWarningAsync($"Something went wrong on executing a Slash Command: {result.ErrorReason}");
                    break;

                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
                    break;
            }
        }

        _console.LogDebug($"Slash command executed: {info?.Name} for {context.Guild.Name} {(result.IsSuccess ? "succeeded" : "failed")} {(result.IsSuccess ? "" : $" - Error: {result.ErrorReason}")}");
    }
}