namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;
using System;

[RequireOwner]
[Discord.Commands.Name(Moby.OnlyMobyGuildModule)]
public sealed class OwnerModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly IDatabase _database;

    public OwnerModule(DiscordSocketClient client, IDatabase database, ConsoleLogger console) : base(console)
    {
        _client = client;
        _database = database;
    }

    [SlashCommand("pingdatabase", "Fetches the current connection delay to the Bot database")]
    public async Task PingDatabaseAsync()
    {
        if (Context.Channel.Id is not Moby.OwnerCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var ping = await _database.PingAsync();

        if (ping == TimeSpan.MaxValue)
        {
            await FollowupAsync("Couldn't reach the database!", ephemeral: true);

            return;
        }

        await FollowupAsync($"The connection was established in {ping.TotalMilliseconds} ms", ephemeral: true);
    }

    [SlashCommand("getserver", "Fetches a server by the provided Server Id")]
    public async Task GetServerAsync([Summary("serverid", "Enter a server id")] [MinLength(10)] [MaxLength(30)] string serverid,
        [Summary("fromdatabase", "True if the server should be fetched from the database")] bool fromdatabase = false)
    {
        if (Context.Channel.Id is not Moby.OwnerCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        if (!ulong.TryParse(serverid, out var id))
        {
            await RespondAsync("Can't find a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        Embed embed;

        if (fromdatabase)
        {
            var guild = await _database.GetGuildInfoAsync(id);

            if (guild.IsEmpty())
            {
                await FollowupAsync("Couldn't fetch a Server with the Id: " + id, ephemeral: true);

                return;
            }

            embed = MobyUtil.GetServerDataEmbed(guild);
        }
        else
        {
            var guild = _client.GetGuild(id);

            if (guild is null)
            {
                await FollowupAsync("Couldn't fetch a Server with the Id: " + id, ephemeral: true);

                return;
            }

            embed = MobyUtil.GetServerInfoEmbed(guild);
        }

        await FollowupAsync(embed: embed, ephemeral: true);
    }

    [SlashCommand("getallserver", "Fetches all Servers where I'm currently on")]
    public async Task GetAllDatabaseServerAsync([Summary("fromdatabase", "True if the server should be fetched from the database")] bool fromdatabase = false)
    {
        if (Context.Channel.Id is not Moby.OwnerCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var embeds = new List<Embed>();

        if (fromdatabase)
        {
            await foreach (var guild in _database.GetAllGuildsAsync())
            {
                if (!guild.IsEmpty()) embeds.Add(MobyUtil.GetServerDataEmbed(guild));
            }
        }
        else
        {
            foreach (var guild in _client.Guilds)
            {
                if (guild is not null) embeds.Add(MobyUtil.GetServerInfoEmbed(guild));
            }
        }

        foreach (var chunk in embeds.Chunk(10))
        {
            await FollowupAsync(embeds: chunk, ephemeral: true);
        }
    }
}