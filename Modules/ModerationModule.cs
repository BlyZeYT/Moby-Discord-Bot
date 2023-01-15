namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

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
    public async Task PurgeAsync([Summary("amount", "Enter the amount of messages that should be purged")] [MinValue(1)] [MaxValue(100)] int amount)
    {
        await DeferAsync(ephemeral: true);

        var messages = (await Context.Channel.GetMessagesAsync(amount + 1, CacheMode.AllowDownload).FlattenAsync()).Where(x => x.Timestamp > DateTime.UtcNow.AddDays(-14) && x.Flags is not MessageFlags.Ephemeral or MessageFlags.Loading);

        if (!messages.TryGetNonEnumeratedCount(out amount))
        {
            amount = messages.Count();
        }

        if (amount <= 1)
        {
            await FollowupAsync("Couldn't get any deletable messages", ephemeral: true);

            return;
        }

        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

        await FollowupAsync($"Deleted {amount} messages \\✉️", ephemeral: true);
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
        [Summary("message", "Enter the custom invitation message")] [MinLength(1)] [MaxLength(2500)] string? message = null,
        [Summary("url", "Enter the custom invitation link")] [MinLength(10)] [MaxLength(100)] string? url = null)
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
    public async Task KickAsync([Summary("user", "Mention the user that should get kicked")] SocketGuildUser user,
        [Summary("reason", "Enter the reason why the user is kicked")] [MinLength(1)] [MaxLength(250)] string? reason = null,
        [Summary("send-dm", "Yes if you want me to send a DM to the user that he got kicked")] Answer senddm = Answer.No)
    {
        await DeferAsync(ephemeral: true);

        if (((SocketGuildUser)Context.User).Hierarchy <= user.Hierarchy)
        {
            await FollowupAsync("You can't kick a user who is higher or equal to you in the hierarchy", ephemeral: true);

            return;
        }

        IUserMessage? dm = null;
        try
        {
            await user.KickAsync(reason);

            if (senddm is Answer.Yes) dm = await user.TrySendMessageAsync(embed: MobyUtil.GetKickDmEmbed(Context.User, Context.Guild, reason));

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetKickEmbed(user, reason));
        }
        catch (Exception)
        {
            if (dm is not null) await dm.DeleteAsync();

            await FollowupAsync("I can't kick this user", ephemeral: true);
        }
    }

    [SlashCommand("ban", "Ban a user from the server")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanAsync([Summary("user", "Mention the user that should get banned")] SocketGuildUser user,
        [Summary("reason", "Enter the reason why the user is banned")] [MinLength(1)] [MaxLength(250)] string? reason = null,
        [Summary("prunedays", "Enter the days from which the message history should be deleted")] [MinValue(0)] [MaxValue(7)] int prunedays = 0,
        [Summary("send-dm", "Yes if you want me to send a DM to the user that he got banned")] Answer senddm = Answer.No)
    {
        await DeferAsync(ephemeral: true);

        if (((SocketGuildUser)Context.User).Hierarchy <= user.Hierarchy)
        {
            await FollowupAsync("You can't ban a user who is higher or equal to you in the hierarchy", ephemeral: true);

            return;
        }

        IUserMessage? dm = null;
        try
        {
            await user.BanAsync(prunedays, reason);

            if (senddm is Answer.Yes) dm = await user.TrySendMessageAsync(embed: MobyUtil.GetBanDmEmbed(Context.User, Context.Guild, reason));

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBanEmbed(user, reason));
        }
        catch (Exception)
        {
            if (dm is not null) await dm.DeleteAsync();

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
    public async Task MuteAsync([Summary("user", "Mention the user that should get muted")] SocketGuildUser user,
        [Summary("reason", "Enter the reason why the user is muted")] [MinLength(1)] [MaxLength(250)] string? reason = null)
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
    public async Task DeafAsync([Summary("user", "Mention the user that should get deafened")] SocketGuildUser user,
        [Summary("reason", "Enter the reason why the user is deafened")] [MinLength(1)] [MaxLength(250)] string? reason = null)
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
    public async Task UnmuteAsync([Summary("user", "Mention the user that should get unmuted")] SocketGuildUser user)
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
    public async Task UndeafAsync([Summary("user", "Mention the user that should get undeafened")] SocketGuildUser user)
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

        await FollowupAsync(interval == 0 ? "Slowmode was deactivated \\🐆" : $"The slowmode interval was set to {interval} seconds \\🐢", ephemeral: true);
    }

    [SlashCommand("poll", "Create a poll where users can vote")]
    public async Task PollAsync([Summary("question", "The question that's being asked")] [MinLength(1)] [MaxLength(35)] string question,
        [Summary("response1", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string response1,
        [Summary("response2", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response2 = null,
        [Summary("response3", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response3 = null,
        [Summary("response4", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response4 = null,
        [Summary("response5", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response5 = null,
        [Summary("response6", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response6 = null,
        [Summary("response7", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response7 = null,
        [Summary("response8", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response8 = null,
        [Summary("response9", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response9 = null,
        [Summary("response10", "A response to vote for")] [MinLength(1)] [MaxLength(35)] string? response10 = null,
        [Summary("emojiset", "The set of emotes that should be used for the answers")] EmojiSet emojiset = EmojiSet.Letters)
    {
        await DeferAsync();

        var emojis = emojiset switch
        {
            EmojiSet.Love => new Emoji[] { "❤️", "🧡", "💙", "💜", "🤍", "💚", "💛", "🤎", "💝", "💗" },
            EmojiSet.Animals => new Emoji[] { "🐶", "🐱", "🐭", "🐷", "🐮", "🦁", "🐨", "🐰", "🦊", "🐵" },
            EmojiSet.Nature => new Emoji[] { "🌞", "🌚", "🌹", "🌷", "🌵", "🌼", "🌻", "🍁", "🌲", "🍄" },
            EmojiSet.Food => new Emoji[] { "🍎", "🍐", "🍇", "🍌", "🍉", "🍒", "🍕", "🥨", "🍓", "🍫" },
            EmojiSet.Vehicles => new Emoji[] { "🚗", "🚙", "🏍️", "🚲", "🚂", "🚓", "🚑", "🚁", "⛵", "🛴" },
            _ => new Emoji[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯" }
        };

        var responses = new string?[]
        {
            response1,
            response2,
            response3,
            response4,
            response5,
            response6,
            response7,
            response8,
            response9,
            response10
        }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        var message = await FollowupAsync(embed: MobyUtil.GetPollEmbed(Context.User, question, responses, emojis));

        await message.AddReactionsAsync(emojis.Take(responses.Length));
    }

    [Group("role", "Commands to manage roles")]
    [Discord.Commands.Name("Role Group Commands")]
    public sealed class RoleCommandGroup : MobyModuleBase
    {
        public RoleCommandGroup(ConsoleLogger console) : base(console) { }

        [SlashCommand("grant", "Grant the mentioned user a role")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RoleGrantAsync([Summary("user", "Mention the user that should get the role")] SocketGuildUser user,
            [Summary("role", "Mention the role that the user should get")] SocketRole role)
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
        public async Task RoleRemoveAsync([Summary("user", "Mention the user from whom the role should be removed")] SocketGuildUser user,
            [Summary("role", "Mention the role that should get removed from the user")] SocketRole role)
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

        [SlashCommand("info", "Get information about the mentioned role")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RoleInfoAsync([Summary("role", "Mention the role you want information about")] SocketRole role)
        {
            await DeferAsync(ephemeral: true);

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRoleInfoEmbed(role));
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
        public async Task ListBansAsync([Summary("amount", "The amount of bans that should be in the list")] [MinValue(1)] [MaxValue(500)] int amount = 100,
            [Summary("as-file", "Yes if you want to get the whole ban list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var bans = await Context.Guild.GetBansAsync(amount).FlattenAsync();

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBanListEmbed(bans));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetBanListAttachment(bans), ephemeral: true);
        }

        [SlashCommand("boosters", "Get a list of the users that are currently boosting the server")]
        public async Task ListBoostersAsync([Summary("as-file", "Yes if you want to get the whole booster list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var boosters = Context.Guild.Users.Where(x => x.PremiumSince is not null);

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBoosterListEmbed(boosters));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetBoosterListAttachment(boosters), ephemeral: true);
        }

        [SlashCommand("channels", "Get a list of all channels on this server")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ListChannelsAsync([Summary("as-file", "Yes if you want to get the whole channel list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var channels = Context.Guild.Channels.OrderByDescending(x => x.Position);

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetChannelListEmbed(channels));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetChannelListAttachment(channels), ephemeral: true);
        }

        [SlashCommand("emotes", "Get a list of all emotes on this server")]
        [RequireUserPermission(GuildPermission.ManageEmojisAndStickers)]
        [RequireBotPermission(GuildPermission.ManageEmojisAndStickers)]
        public async Task ListEmotesAsync([Summary("as-file", "Yes if you want to get the whole emote list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var emotes = Context.Guild.Emotes.OrderByDescending(x => x.CreatedAt);

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetEmoteListEmbed(emotes));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetEmoteListAttachment(emotes), ephemeral: true);
        }

        [SlashCommand("roles", "Get a list of all roles on this server")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ListRolesAsync([Summary("as-file", "Yes if you want to get the whole role list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var roles = Context.Guild.Roles.OrderByDescending(x => x.CreatedAt);

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRoleListEmbed(roles));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetRoleListAttachment(roles), ephemeral: true);
        }

        [SlashCommand("audit-log", "Get a list of audit log entries on this server")]
        [RequireUserPermission(GuildPermission.ViewAuditLog)]
        [RequireBotPermission(GuildPermission.ViewAuditLog)]
        public async Task ListAuditLogAsync([Summary("amount", "The amount of audit log entries that should be in the list")] [MinValue(1)] [MaxValue(500)] int amount = 100,
            [Summary("user", "Mention the user to filter the audit log entries from")] IUser? user = null,
            [Summary("as-file", "Yes if you want to get the whole audit log list as a text file")] Answer asfile = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var roles = (await Context.Guild.GetAuditLogsAsync(amount, userId: user?.Id).FlattenAsync()).OrderByDescending(x => x.CreatedAt);

            if (asfile is Answer.No)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetAuditLogListEmbed(roles));
                return;
            }

            await FollowupWithFileAsync(MobyUtil.GetAuditLogListAttachment(roles), ephemeral: true);
        }
    }
}