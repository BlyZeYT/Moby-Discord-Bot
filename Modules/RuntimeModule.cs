namespace Moby.Modules;

using Discord.Interactions;
using Discord.WebSocket;
using Common;
using Services;
using Discord;

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

        var guild = await _database.GetGuildInfoAsync(Convert.ToUInt64(serverid));

        if (guild.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        if (!_client.Guilds.Any(x => x.Id == guild.GuildId))
        {
            await FollowupAsync($"The server: {guild.GuildId} can't be removed from the database because I'm on this server", ephemeral: true);

            return;
        }

        await _database.RemoveGuildAsync(guild.GuildId);

        await FollowupAsync($"The server: {guild.GuildId} was removed from the database", ephemeral: true);
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

        var guild = await _database.GetGuildInfoAsync(Convert.ToUInt64(serverid));

        if (guild.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a server with the Id: " + serverid, ephemeral: true);

            return;
        }

        await _database.RemoveAllPlaylistsAsync(guild.GuildId);

        await FollowupAsync($"The server: {guild.GuildId} was reset on the database", ephemeral: true);
    }

    [SlashCommand("adduser", "Add a user manually to the database")]
    public async Task AddUserAsync([Summary("userid", "Enter a user id")] [MinLength(10)] [MaxLength(30)] string userid)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _client.GetUserAsync(Convert.ToUInt64(userid));

        if (user is null)
        {
            await FollowupAsync("Couldn't fetch a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await _database.AddUserAsync(user.Id);

        await FollowupAsync($"The user: {user.Id} was added to the database", ephemeral: true);
    }

    [SlashCommand("removeuser", "Remove a user manually from the database")]
    public async Task RemoveUserAsync([Summary("userid", "Enter a user id")] [MinLength(10)] [MaxLength(30)] string userid)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _client.GetUserAsync(Convert.ToUInt64(userid));

        if (user is null)
        {
            await FollowupAsync("Couldn't fetch a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await _database.RemoveUserAsync(user.Id);

        await FollowupAsync($"The user: {user.Id} was removed from the database", ephemeral: true);
    }

    [SlashCommand("addscore", "Add score to a user")]
    public async Task AddScoreAsync([Summary("userid", "Enter a user id")] [MinLength(10)] [MaxLength(30)] string userid,
        [Summary("score", "The score to add")] [MinValue(1)] [MaxValue(int.MaxValue)] int score)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _database.GetUserInfoAsync(Convert.ToUInt64(userid));

        if (user.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await _database.AddScoreAsync(user.UserId, score);

        await FollowupAsync($"Added {score} to the user: {user.Id} on the database", ephemeral: true);
    }

    [SlashCommand("deductscore", "Deduct score from a user")]
    public async Task DeductScoreAsync([Summary("userid", "Enter a user id")] [MinLength(10)] [MaxLength(30)] string userid,
        [Summary("score", "The score to deduct")] [MinValue(1)] [MaxValue(int.MaxValue)] int score)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _database.GetUserInfoAsync(Convert.ToUInt64(userid));

        if (user.IsEmpty())
        {
            await FollowupAsync("Couldn't fetch a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await _database.AddScoreAsync(user.UserId, -score);

        await FollowupAsync($"Deducted {score} from the user: {user.Id} on the database", ephemeral: true);
    }

    [SlashCommand("setstatus", "Set the activity of the bot")]
    public async Task SetStatusAsync([Summary("activity", "The activity the bot should display")] string activity,
        [Summary("type", "The type of the activity")] ActivityType type,
        [Summary("url", "The url to a twitch stream")] string? url = null,
        [Summary("status", "The status the bot should have")] UserStatus status = UserStatus.Online)
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        await _client.SetStatusAsync(status);
        await _client.SetGameAsync(activity, url, type);

        await FollowupAsync("The status was set successfully", ephemeral: true);
    }

    [SlashCommand("leaveserver", "Leave a server manually")]
    public async Task LeaveServerAsync([Summary("serverid", "Enter a server id")] [MinLength(10)] [MaxLength(30)] string serverid)
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

        await guild.LeaveAsync();

        await FollowupAsync("Lefted the server successfully", ephemeral: true);
    }
}