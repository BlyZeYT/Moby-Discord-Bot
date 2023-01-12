namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireOwner]
[Discord.Commands.Name(Moby.OnlyMobyGuildModule)]
public sealed class RuntimeModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly IDatabase _database;

    public RuntimeModule(DiscordSocketClient client, IDatabase database, ConsoleLogger console) : base(console)
    {
        _client = client;
        _database = database;
    }

    [SlashCommand("announce", "Announce something on all servers")]
    public async Task AnnounceAsync()
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }
        
        await RespondWithModalAsync(MobyUtil.GetAnnouncementModal());
    }

    [SlashCommand("addserver", "Add a server manually to the database")]
    public async Task AddServerAsync([Summary("serverid", "Enter a server id")] [MinLength(10)] [MaxLength(30)] string serverid)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var guild = _client.GetGuild(Convert.ToUInt64(serverid));

        if (guild is null)
        {
            await FollowupAsync("Couldn't fetch a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        await _database.AddGuildAsync(guild.Id);

        await FollowupAsync($"The server: {guild.Name} with Id: {guild.Id} was added to the database", ephemeral: true);
    }

    [SlashCommand("removeserver", "Remove a server manually from the database")]
    public async Task RemoveServerAsync([Summary("serverid", "Enter a server id")] [MinLength(10)] [MaxLength(30)] string serverid)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var guildInfo = await _database.GetGuildInfoAsync(Convert.ToUInt64(serverid));

        if (guildInfo.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        if (_client.GetGuild(guildInfo.GuildId) is not null)
        {
            await FollowupAsync($"The server: {guildInfo.GuildId} can't be removed from the database because I'm on this server", ephemeral: true);

            return;
        }

        await _database.RemoveGuildAsync(guildInfo.GuildId);

        await FollowupAsync($"The server: {guildInfo.GuildId} was removed from the database", ephemeral: true);
    }

    [SlashCommand("resetserver", "Reset the data of a server on the database")]
    public async Task ResetServerAsync([Summary("serverid", "Enter a server id")] [MinLength(10)] [MaxLength(30)] string serverid)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var guildInfo = await _database.GetGuildInfoAsync(Convert.ToUInt64(serverid));

        if (guildInfo.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        await _database.SetRepeatAsync(guildInfo.GuildId, false);

        await _database.RemoveAllPlaylistsAsync(guildInfo.GuildId);

        await FollowupAsync($"The server: {guildInfo.GuildId} was reset on the database", ephemeral: true);
    }
}