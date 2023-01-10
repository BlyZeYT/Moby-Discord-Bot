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

        await FollowupAsync($"Deleted {count} messages \\✉️", ephemeral: true);
    }

    [SlashCommand("unpin", "Unpin all messages in the mentioned channel")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    public async Task UnpinAsync([Summary("channel", "Mention the channel where all messages should get unpinned")] ITextChannel channel)
    {
        await DeferAsync(ephemeral: true);

        foreach (var message in await channel.GetPinnedMessagesAsync())
        {
            await ((IUserMessage)message).UnpinAsync();
        }

        await FollowupAsync($"Unpinned all messages in {channel.Mention}", ephemeral: true);
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

    [SlashCommand("baninfo", "Get information about a banned user")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanInfoAsync([Summary("userid", "Enter the id of the user you want information about")] [MinLength(10)] [MaxLength(30)] string userid)
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

        var ban = await Context.Guild.GetBanAsync(user);

        if (ban is null)
        {
            await FollowupAsync($"{user.Mention} is not banned from this server", ephemeral: true);

            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBanInfoEmbed(ban));
    }

    [SlashCommand("unban", "Unban a user from the server")]
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

    [SlashCommand("mute", "Mute a user in a voice channel")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    public async Task MuteAsync([Summary("user", "Mention a user that should get muted")] SocketGuildUser user,
        [Summary("reason", "Enter a reason why the user is muted")] [MinLength(1)] [MaxLength(250)] string? reason = null)
    {
        await DeferAsync(ephemeral: true);

        if (user.VoiceChannel is null)
        {
            await FollowupAsync($"{user.Mention} is not in a voice channel", ephemeral: true);
            return;
        }

        if (user.IsMuted || user.IsSelfMuted)
        {
            await FollowupAsync($"{user.Mention} is already muted", ephemeral: true);
            return;
        }

        await user.VoiceChannel.ModifyAsync(x => x.SelfMute = true);

        await user.VoiceChannel.SendMessageAsync(embed: MobyUtil.GetUserMutedEmbed(user, reason));
    }

    [SlashCommand("deaf", "Deafen a user in a voice channel")]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public async Task DeafAsync([Summary("user", "Mention a user that should get deafened")] SocketGuildUser user,
        [Summary("reason", "Enter a reason why the user is deafened")] [MinLength(1)] [MaxLength(250)] string? reason = null)
    {
        await DeferAsync(ephemeral: true);

        if (user.VoiceChannel is null)
        {
            await FollowupAsync($"{user.Mention} is not in a voice channel", ephemeral: true);
            return;
        }

        if (user.IsDeafened || user.IsSelfDeafened)
        {
            await FollowupAsync($"{user.Mention} is already deafened", ephemeral: true);
            return;
        }

        await user.VoiceChannel.ModifyAsync(x => x.SelfDeaf = true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserDeafenedEmbed(user, reason));
    }

    [SlashCommand("unmute", "Unmute a user in a voice channel")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    public async Task UnmuteAsync([Summary("user", "Mention a user that should get unmuted")] SocketGuildUser user)
    {
        await DeferAsync(ephemeral: true);

        if (user.VoiceChannel is null)
        {
            await FollowupAsync($"{user.Mention} is not in a voice channel", ephemeral: true);
            return;
        }

        if (!(user.IsMuted || user.IsSelfMuted))
        {
            await FollowupAsync($"{user.Mention} is already unmuted", ephemeral: true);
            return;
        }

        await user.VoiceChannel.ModifyAsync(x => x.SelfMute = false);

        await user.VoiceChannel.SendMessageAsync(embed: MobyUtil.GetUserUnmutedEmbed(user));
    }

    [SlashCommand("undeaf", "Undeafen a user in a voice channel")]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public async Task UndeafAsync([Summary("user", "Mention a user that should get undeafened")] SocketGuildUser user)
    {
        await DeferAsync(ephemeral: true);

        if (user.VoiceChannel is null)
        {
            await FollowupAsync($"{user.Mention} is not in a voice channel", ephemeral: true);
            return;
        }

        if (!(user.IsDeafened || user.IsSelfDeafened))
        {
            await FollowupAsync($"{user.Mention} is already undeafened", ephemeral: true);
            return;
        }

        await user.VoiceChannel.ModifyAsync(x => x.SelfDeaf = false);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserUndeafenedEmbed(user));
    }

    [SlashCommand("slowmode", "Change the slowmode settings for this channel")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SlowmodeAsync([Summary("interval", "Enter the interval for the slowmode, 0 to disable slowmode")] [MinValue(0)] [MaxValue(2880)] int interval)
    {
        await DeferAsync(ephemeral: true);
        
        await ((ITextChannel)Context.Channel).ModifyAsync(x => x.SlowModeInterval = interval);

        await FollowupAsync(interval == 0 ? "Slowmode was deactivated" : $"The slowmode interval was set to {interval} seconds", ephemeral: true);
    }

    [Group("role", "Commands to manage roles")]
    [Discord.Commands.Name("Role Group Commands")]
    public sealed class RoleCommandGroup : MobyModuleBase
    {
        public RoleCommandGroup(ConsoleLogger console) : base(console) { }

        [SlashCommand("grant", "Grant the mentioned user a role")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RoleGrantAsync([Summary("user", "Mention a user that should get the role")] SocketGuildUser user,
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

        [SlashCommand("remove", "Remove a role from the mentioned user")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RoleRemoveAsync([Summary("user", "Mention a user from whom the role should be removed")] SocketGuildUser user,
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
    }

    [Group("list", "Commands to list something")]
    [Discord.Commands.Name("List Group Commands")]
    public sealed class ListGroupCommands : MobyModuleBase
    {
        public ListGroupCommands(ConsoleLogger console) : base(console) { }

        [SlashCommand("bans", "Get a list of the currently banned users")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task ListBansAsync([Summary("amount", "The amount of bans that should be in the list")] [MinValue(1)] [MaxValue(500)] int amount = 100)
        {
            await DeferAsync(ephemeral: true);

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBanlistEmbed(await Context.Guild.GetBansAsync(amount).FlattenAsync()));
        }
    }
}