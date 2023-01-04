namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireContext(ContextType.Guild)]
[Discord.Commands.Name("Moderation")]
public sealed class ModerationModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;

    public ModerationModule(DiscordSocketClient client, ConsoleLogger console) : base(console)
    {
        _client = client;
    }

    [SlashCommand("purge", "Delete messages that are not older than 14 days")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    public async Task PurgeAsync([Summary("amount", "Enter a amount of messages that should be purged between 1 and 100")] [MinValue(1)] [MaxValue(100)] int amount)
    {
        await DeferAsync(ephemeral: true);

        var messages = (await Context.Channel.GetMessagesAsync(amount, CacheMode.AllowDownload).FlattenAsync()).Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays < 14 && x.Flags is not MessageFlags.Ephemeral or MessageFlags.Loading);

        if (!messages.TryGetNonEnumeratedCount(out int count))
        {
            count = messages.Count();
        }

        if (count <= 1)
        {
            await FollowupAsync("Couldn't get any deletable messages", ephemeral: true);

            return;
        }

        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

        await FollowupAsync($"Deleted {count--} messages \\✉️", ephemeral: true);
    }

    [SlashCommand("invite", "Invite a user to the server")]
    [RequireUserPermission(GuildPermission.CreateInstantInvite)]
    [RequireBotPermission(GuildPermission.CreateInstantInvite)]
    public async Task InviteAsync([Summary("userid", "Enter the id of the user that should get an invite")] [MinLength(10)] [MaxLength(30)] string userid,
        [Summary("message", "Enter a custom invitation message")] [MinLength(1)] [MaxLength(2500)] string? message = null,
        [Summary("url", "Enter a custom invitation link")] [MinLength(10)] [MaxLength(100)] string? url = null)
    {
        if (!ulong.TryParse(userid, out var id))
        {
            await RespondAsync("Couldn't find a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _client.GetUserAsync(id);

        if (user is null)
        {
            await FollowupAsync("Couldn't find a user with the Id: " + id, ephemeral: true);

            return;
        }

        try
        {
            await user.SendMessageAsync(
                embed: MobyUtil.GetInvitationEmbed(Context.User, Context.Guild, message),
                components: MobyUtil.GetInvitationComponent(url ?? (await (Context.Guild.GetFirstTextChannel() ?? Context.Guild.DefaultChannel).CreateInviteAsync(null, null, false, false)).Url));

            await FollowupAsync($"Successfully sent an invitation to **{user.Username}#{user.Discriminator}** \\📬", ephemeral: true);
        }
        catch (HttpException http)
        {
            if (http.DiscordCode.HasValue)
            {
                if (http.DiscordCode is DiscordErrorCode.CannotSendMessageToUser)
                {
                    await FollowupAsync("Can't send a DM to this user", ephemeral: true);
                }
            }
        }
    }

    [SlashCommand("grant-role", "Grant the mentioned user a role")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task GrantRoleAsync([Summary("user", "Mention a user that should get the role")] SocketGuildUser user,
        [Summary("role", "Mention a role that the user should get")] SocketRole role)
    {
        await DeferAsync(ephemeral: true);

        if (role.IsManaged || role.IsEveryone)
        {
            await FollowupAsync("I can't work with this role", ephemeral: true);

            return;
        }

        if (Context.Guild.CurrentUser.Roles.Max(x => x.Position) <= role.Position)
        {
            await FollowupAsync("I can't distribute a role that is higher or equal to mine", ephemeral: true);

            return;
        }

        if (user.Roles.Contains(role))
        {
            await FollowupAsync("The user already has that role", ephemeral: true);

            return;
        }

        await user.AddRoleAsync(role);

        await FollowupAsync($"Granted {user.Mention} the role {(role.IsMentionable ? role.Mention : role.Name)} {role.Emoji}", ephemeral: true);
    }

    [SlashCommand("remove-role", "Remove a role from the mentioned user")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RemoveRoleAsync([Summary("user", "Mention a user from whom the role should be removed")] SocketGuildUser user,
        [Summary("role", "Mention a role that should get removed from the user")] SocketRole role)
    {
        await DeferAsync(ephemeral: true);

        if (role.IsManaged || role.IsEveryone)
        {
            await FollowupAsync("I can't work with this role", ephemeral: true);

            return;
        }

        if (Context.Guild.CurrentUser.Roles.Max(x => x.Position) <= role.Position)
        {
            await FollowupAsync("I can't distribute a role that is higher or equal to mine", ephemeral: true);

            return;
        }

        if (!user.Roles.Contains(role))
        {
            await FollowupAsync("The user doesn't have that role", ephemeral: true);

            return;
        }

        await user.RemoveRoleAsync(role);

        await FollowupAsync($"Removed the role {(role.IsMentionable ? role.Mention : role.Name)} from {user.Mention} {role.Emoji}", ephemeral: true);
    }

    [SlashCommand("kick", "Kick a user from the server")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    public async Task KickAsync([Summary("user", "Mention a user that should get kicked")] SocketGuildUser user,
        [Summary("reason", "Enter a reason why the user is kicked")] [MinLength(1)] [MaxLength(250)] string? reason = null)
    {
        await DeferAsync(ephemeral: true);

        if (((SocketGuildUser)Context.User).Hierarchy <= user.Hierarchy)
        {
            await FollowupAsync("You can't kick a user who is higher or equal to you in the hierarchy", ephemeral: true);

            return;
        }

        try
        {
            await user.KickAsync(reason);

            await FollowupAsync($"Kicked \\⛔: **{user.Username}#{user.Discriminator}**{(reason is null ? "" : $"\nReason \\💬: {reason}")}", ephemeral: true);
        }
        catch (Exception)
        {
            await FollowupAsync("I can't kick this user", ephemeral: true);
        }
    }

    [SlashCommand("ban", "Ban a user from the server")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanAsync([Summary("user", "Mention a user that should get banned")] SocketGuildUser user,
        [Summary("reason", "Enter a reason why the user is banned")] [MinLength(1)] [MaxLength(250)] string? reason = null,
        [Summary("prunedays", "Enter the days from which the message history should be deleted")] [MinValue(0)] [MaxValue(7)] int prunedays = 0)
    {
        await DeferAsync(ephemeral: true);

        if (((SocketGuildUser)Context.User).Hierarchy <= user.Hierarchy)
        {
            await FollowupAsync("You can't ban a user who is higher or equal to you in the hierarchy", ephemeral: true);

            return;
        }

        try
        {
            await user.BanAsync(prunedays, reason);

            await FollowupAsync($"Banned \\⛔: **{user.Username}#{user.Discriminator}**{(reason is null ? "" : $"\nReason \\💬: {reason}")}", ephemeral: true);
        }
        catch (Exception)
        {
            await FollowupAsync("I can't ban this user", ephemeral: true);
        }
    }

    [SlashCommand("banlist", "Get a list of the currently banned users")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanlistAsync([Summary("amount", "The amount of bans that should be in the list")] [MinValue(1)] [MaxValue(500)] int amount = 100)
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetBanlistEmbed(await Context.Guild.GetBansAsync(amount).FlattenAsync()));

    [SlashCommand("pardon", "Unban a user from the server")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)] 
    public async Task PardonAsync([Summary("userid", "Enter the id of the user that should get unbanned")] [MinLength(10)] [MaxLength(30)] string userid)
    {
        if (!ulong.TryParse(userid, out var id))
        {
            await RespondAsync("Couldn't find a user with the Id: " + userid, ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        var user = await _client.GetUserAsync(id);

        if (user is null)
        {
            await FollowupAsync("Couldn't find a user with the Id: " + id, ephemeral: true);

            return;
        }

        try
        {
            await Context.Guild.RemoveBanAsync(user);

            await FollowupAsync($"Unbanned \\💚: **{user.Username}#{user.Discriminator}**", ephemeral: true);
        }
        catch (Exception)
        {
            await FollowupAsync("I can't unban this user", ephemeral: true);
        }
    }
}