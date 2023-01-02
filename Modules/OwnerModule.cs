namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireOwner]
[RequireContext(ContextType.Guild)]
public sealed class OwnerModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly IDatabase _database;

    public OwnerModule(DiscordSocketClient client, IDatabase database, ConsoleLogger console) : base(console)
    {
        _client = client;
        _database = database;
    }

    [SlashCommand("checkdatabaseconnection", "Fetches the current connection delay to the Bot database")]
    public async Task CheckDatabaseConnectionAsync()
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
    public async Task GetServerAsync([Summary("serverid", "Enter a server id")] string serverid)
    {
        if (Context.Channel.Id is not Moby.OwnerCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var guild = _client.GetGuild(Convert.ToUInt64(serverid));

        if (guild is null)
        {
            await FollowupAsync("Couldn't fetch a Server with the Id: " + serverid, ephemeral: true);

            return;
        }

        await FollowupAsync(embed: MobyUtil.GetServerInfoEmbed(guild), ephemeral: true);
    }

    [SlashCommand("getalldatabaseserver", "Fetches all Servers that are currently on the database")]
    public async Task GetAllDatabaseServerAsync()
    {
        if (Context.Channel.Id is not Moby.OwnerCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var embeds = new List<Embed>();
        SocketGuild? guild;

        await foreach (var guildId in _database.GetAllGuildsAsync())
        {
            guild = _client.GetGuild(guildId);

            if (guild is not null) embeds.Add(MobyUtil.GetServerInfoEmbed(guild));
        }

        await FollowupAsync(embeds: embeds.ToArray(), ephemeral: true);
    }
}