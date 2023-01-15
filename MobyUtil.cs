namespace Moby;

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using global::Moby.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class MobyUtil
{
    public static Embed GetLogEmbed(LogLevel logLevel, Exception? exception)
    {
        var builder = new EmbedBuilder().WithColor(Moby.LogLevels[logLevel].Item1);

        if (exception is null)
        {
            builder.AddField("Current Time", DateTimeOffset.Now.ToString("F"));
        }
        else
        {
            builder
                .WithTitle($"**{exception.Message}**")
                .AddField("Exception type", exception.GetType().FullName ?? "Unknown")
                .AddField("Source", exception.Source ?? "Unknown")
                .WithCurrentTimestamp();
        }

        return builder.Build();
    }

    public static Embed GetBotStatsEmbed(IUser bot, int latency)
    {
        var general = new StringBuilder($"```\n- Uptime > {Stats.Uptime.Days} days & {Stats.Uptime.Hours} hours\n");
        general.AppendLine($"- Latency > {latency} ms");
        general.AppendLine($"- Playing Players > {Stats.Players}");
        general.AppendLine($"- Frames Sent > {Stats.FramesSent}");
        general.Append("```");

        var system = new StringBuilder($"```\n- OS > {RuntimeInformation.OSDescription.RemoveAfter(25, ' ')}\n");
        system.AppendLine("- CPU > Broadcom BCM2711");
        system.AppendLine($"- CPU Cores > {Environment.ProcessorCount}");
        system.AppendLine($"- CPU Speed > 1,5 GHz");
        system.AppendLine($"- CPU Usage > {Math.Round(Stats.CpuLoad, 2, MidpointRounding.ToEven)} %");
        system.AppendLine("- Memory > 8 GB LPDDR4 SDRAM");
        system.AppendLine("- Memory Speed > 3200 MHz");
        system.AppendLine($"- Memory Usage > {Stats.AllocatedMemory * 0.00000095367431640625} MB");
        system.Append("```");

        return new MobyEmbedBuilder()
            .WithAuthor($"{bot.Username} - Statistics", bot.GetAvatarUrl(size: 2048) ?? bot.GetDefaultAvatarUrl())
            .AddField("General", general.ToString())
            .AddField("System", system.ToString())
            .Build();
    }

    public static Embed GetServerInfoEmbed(SocketGuild guild)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"**\\👥 Information about {guild.Name}**")
            .WithThumbnailUrl(guild.IconUrl ?? Moby.ImageNotFound)
            .AddField("Created at", guild.CreatedAt.ToString("d"))
            .AddField("Member Count", guild.MemberCount)
            .AddField("Member Online", guild.Users.Count(x => x.Status is UserStatus.Online))
            .AddField("Boosts", guild.PremiumSubscriptionCount)
            .Build();
    }

    public static Embed GetUserInfoEmbed(SocketGuildUser user)
    {
        var builder = new EmbedBuilder()
            .WithColor(user.Roles.MaxBy(x => x.Position)?.Color ?? Moby.Color)
            .WithTitle($"**\\🪪 Information about {user.Username}**")
            .WithThumbnailUrl(user.GetAvatarUrl(size: 2048) ?? user.GetDefaultAvatarUrl())
            .AddField("Created Account at", user.CreatedAt.ToString("d"));

        if (user.JoinedAt.HasValue) builder.AddField("Joined Server at", user.JoinedAt.Value.ToString("d"));

        return builder
            .AddField("Roles", string.Join(' ', user.Roles.Select(x => x.Mention)))
            .Build();
    }

    public static Embed GetBotInfoEmbed(SocketGuildUser bot, int serverCount)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🐳 Information about me**")
            .WithThumbnailUrl(bot.GetAvatarUrl(size: 2048) ?? bot.GetDefaultAvatarUrl())
            .AddField("Created at", bot.CreatedAt.ToString("d"));

        if (bot.JoinedAt.HasValue) builder.AddField("Joined Server at", bot.JoinedAt.Value.ToString("d"));

        return builder
            .AddField("Server Count", serverCount)
            .AddField("Creator", "BlyZe")
            .AddField("Roles", string.Join(' ', bot.Roles.Select(x => x.Mention)))
            .Build();
    }

    public static Embed GetRedditPostEmbed(RedditPost post)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"**{post.Title}**")
            .WithImageUrl(post.ImageUrl)
            .WithUrl($"https://reddit.com{post.Permalink}")
            .WithFooter($"🗨 {post.CommentsCount} ⬆️ {post.UpvotesCount}")
            .Build();
    }

    public static MessageComponent GetContactComponent()
    {
        var menu = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId(Moby.ContactMenuCId)
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Provide Feedback", Moby.ContactMenuFeedbackCId, "You can write anything in here", new Emoji("📝"))
            .AddOption("Submit an Idea", Moby.ContactMenuIdeaCId, "For example a feature you would like :)", new Emoji("💡"))
            .AddOption("Report a Bug", Moby.ContactMenuBugCId, "If you think that something is not working like it should", new Emoji("🐞"));

        return new ComponentBuilder()
            .WithSelectMenu(menu)
            .Build();
    }

    public static Modal GetFeedbackModal()
    {
        return new ModalBuilder()
            .WithTitle("Provide Feedback")
            .WithCustomId(Moby.FeedbackModalCId)
            .AddTextInput("Topic", Moby.FeedbackModalTopicCId, TextInputStyle.Short, "The topic of your feedback", 1, 50, true)
            .AddTextInput("Feedback", Moby.FeedbackModalDescriptionCId, TextInputStyle.Paragraph, "Your feedback here", 1, null, true)
            .AddTextInput("Your contact", Moby.FeedbackModalContactCId, TextInputStyle.Short, "For example: myemail@coolmail.com, MyName#1234", null, null, false)
            .Build();
    }

    public static Embed GetFeedbackModalEmbed(SocketMessageComponentData[] data, ulong guildId)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\📝 Provided Feedback**")
            .AddField("Guild Id", guildId)
            .AddField("Topic", data.First(x => x.CustomId == Moby.FeedbackModalTopicCId).Value)
            .AddField("Description", data.First(x => x.CustomId == Moby.FeedbackModalDescriptionCId).Value);

        var contact = data.First(x => x.CustomId == Moby.FeedbackModalContactCId).Value;

        if (!string.IsNullOrWhiteSpace(contact))
        {
            builder.AddField("Contact", contact);
        }

        return builder.Build();
    }

    public static Modal GetIdeaModal()
    {
        return new ModalBuilder()
            .WithTitle("Submit an Idea")
            .WithCustomId(Moby.IdeaModalCId)
            .AddTextInput("Topic", Moby.IdeaModalTopicCId, TextInputStyle.Short, "For example: New feature, Change existing feature, etc.", 1, 50, true, "New feature")
            .AddTextInput("Describe your idea", Moby.IdeaModalDescriptionCId, TextInputStyle.Paragraph, "Describe your idea as detailed as possible", 1, null, true)
            .AddTextInput("Your contact", Moby.IdeaModalContactCId, TextInputStyle.Short, "For example: myemail@coolmail.com, MyName#1234", null, null, false)
            .Build();
    }

    public static Embed GetIdeaModalEmbed(SocketMessageComponentData[] data, ulong guildId)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\💡 Submitted idea**")
            .AddField("Guild Id", guildId)
            .AddField("Topic", data.First(x => x.CustomId == Moby.IdeaModalTopicCId).Value)
            .AddField("Description", data.First(x => x.CustomId == Moby.IdeaModalDescriptionCId).Value);

        var contact = data.First(x => x.CustomId == Moby.IdeaModalContactCId).Value;

        if (!string.IsNullOrWhiteSpace(contact))
        {
            builder.AddField("Contact", contact);
        }

        return builder.Build();
    }

    public static Modal GetBugModal()
    {
        return new ModalBuilder()
            .WithTitle("Report a Bug")
            .WithCustomId(Moby.BugModalCId)
            .AddTextInput("Command", Moby.BugModalCommandCId, TextInputStyle.Short, "At which command the bug occurs", 1, 25, true)
            .AddTextInput("Steps to reproduce", Moby.BugModalReproductionCId, TextInputStyle.Paragraph, "Steps to reproduce the bug step by step", 1, null, true, "1.\n2.\n3.\n")
            .AddTextInput("Describe what happens", Moby.BugModalDescriptionCId, TextInputStyle.Paragraph, "Describe what happens as detailed as possible", 1, null, true)
            .AddTextInput("Your contact", Moby.BugModalContactCId, TextInputStyle.Short, "For example: myemail@coolmail.com, MyName#1234", null, null, false)
            .Build();
    }

    public static Embed GetBugModalEmbed(SocketMessageComponentData[] data, ulong guildId)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🐞Bug Report**")
            .AddField("Guild Id", guildId)
            .AddField("Command", data.First(x => x.CustomId == Moby.BugModalCommandCId).Value)
            .AddField("Steps to reproduce", data.First(x => x.CustomId == Moby.BugModalReproductionCId).Value)
            .AddField("Description", data.First(x => x.CustomId == Moby.BugModalDescriptionCId).Value);

        var contact = data.First(x => x.CustomId == Moby.BugModalContactCId).Value;

        if (!string.IsNullOrWhiteSpace(contact))
        {
            builder.AddField("Contact", contact);
        }

        return builder.Build();
    }

    public static Embed GetAnnouncementEmbed(IUser bot, SocketMessageComponentData[] data)
    {
        return new MobyEmbedBuilder()
            .WithAuthor($"{bot.Username} - Announcement", bot.GetAvatarUrl(size: 2048) ?? bot.GetDefaultAvatarUrl())
            .WithTitle($"**{data.First(x => x.CustomId == Moby.AnnouncementModalTitleCId).Value}**")
            .WithDescription(data.First(x => x.CustomId == Moby.AnnouncementModalMessageCId).Value)
            .Build();
    }

    public static Modal GetAnnouncementModal()
    {
        return new ModalBuilder()
            .WithTitle("Announcement")
            .WithCustomId(Moby.AnnouncementModalCId)
            .AddTextInput("Title", Moby.AnnouncementModalTitleCId, TextInputStyle.Short, "Enter the titel for the announcement", null, 30, true, "\\🐳")
            .AddTextInput("Message", Moby.AnnouncementModalMessageCId, TextInputStyle.Paragraph, "Enter the message for the announcement", null, null, true)
            .Build();
    }

    public static Embed GetServerDataEmbed(DatabaseGuildInfo guildInfo)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**Server data**")
            .AddField("Id", guildInfo.Id)
            .AddField("GuildId", guildInfo.GuildId)
            .AddField("IsRepeatOn", guildInfo.IsRepeatOn)
            .Build();
    }

    public static MessageComponent GetInvitationComponent(string invitationUrl)
    {
        return new ComponentBuilder()
            .WithButton("Join Server", null, ButtonStyle.Link, new Emoji("✅"), invitationUrl)
            .WithButton("Deny invitation", Moby.DenyInvitationButtonCId, ButtonStyle.Danger)
            .Build();
    }

    public static Embed GetInvitationEmbed(IUser sender, SocketGuild guild, string? message)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"**{sender.Username} invites you to {guild.Name}**")
            .WithThumbnailUrl(guild.IconUrl ?? Moby.ImageNotFound)
            .WithDescription($"**Members:** {guild.MemberCount}\n**Created at:** {guild.CreatedAt:d}{(string.IsNullOrWhiteSpace(message) ? "" : $"\n\n{message}")}")
            .Build();
    }

    public static Embed GetBanListEmbed(IEnumerable<RestBan> banlist)
    {
        var sb = new StringBuilder();

        foreach (var ban in banlist)
        {
            var mention = ban.User.Mention;
            var discriminator = ban.User.Discriminator;
            var userId = ban.User.Id.ToString();
            var reason = ban.Reason;

            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + mention.Length + discriminator.Length + userId.Length + reason.Length + 25) break;

            sb.AppendLine($"**{mention} #{discriminator}**");
            sb.AppendLine("Id: " + userId);
            sb.AppendLine("Reason: "+ reason);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("**\\⛔ Ban List**")
            .WithDescription(sb.Length == 0 ? "No users are currently banned \\💚" : sb.ToString())
            .Build();
    }

    public static Embed GetBoosterListEmbed(IEnumerable<SocketGuildUser> boosterlist)
    {
        var sb = new StringBuilder();

        foreach (var boost in boosterlist)
        {
            var mention = boost.Mention;
            var discriminator = boost.Discriminator;
            var userId = boost.Id.ToString();
            var premiumSince = boost.PremiumSince!.Value.UtcDateTime.ToString("d");

            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + mention.Length + discriminator.Length + userId.Length + premiumSince.Length + 25) break;

            sb.AppendLine($"**{mention} #{discriminator}**");
            sb.AppendLine("Id: " + userId);
            sb.AppendLine("Boosting since: " + premiumSince);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("**\\💎 Booster List**")
            .WithDescription(sb.Length == 0 ? "No users are currently boosting \\🥲" : sb.ToString())
            .Build();
    }

    public static Embed GetChannelListEmbed(IEnumerable<SocketGuildChannel> channellist)
    {
        var sb = new StringBuilder();

        foreach (var channel in channellist)
        {
            var channelName = channel.Name;
            var channelType = channel.GetChannelType() ?? ChannelType.Text;
            var channelId = channel.Id.ToString();
            var createdAt = channel.CreatedAt.UtcDateTime.ToString("d");
            
            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + channelName.Length + 20 + channelId.Length + createdAt.Length + 25) break;

            sb.AppendLine($"**{channelName}**");
            sb.AppendLine("Channel type: " + channelType);
            sb.AppendLine("Id: " + channelId);
            sb.AppendLine("Created at: " + createdAt);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("**\\🔤 Channel List**")
            .WithDescription(sb.Length == 0 ? "No channels found (how is that even possible?) \\🥲" : sb.ToString())
            .Build();
    }

    public static Embed GetEmoteListEmbed(IEnumerable<GuildEmote> emotelist)
    {
        var sb = new StringBuilder();

        foreach (var emote in emotelist)
        {
            var emoteName = emote.Name;
            var url = emote.Url;
            var emoteId = emote.Id.ToString();
            var createdAt = emote.CreatedAt.UtcDateTime.ToString("d");

            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + emoteName.Length + url.Length + emoteId.Length + createdAt.Length + 25) break;

            sb.AppendLine($"**[{emoteName}]({url})**");
            sb.AppendLine("Id: " + emoteId);
            sb.AppendLine("Created at: " + createdAt);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("**\\😃 Emote List**")
            .WithDescription(sb.Length == 0 ? "No emotes found \\🥲" : sb.ToString())
            .Build();
    }

    public static Embed GetRoleListEmbed(IEnumerable<SocketRole> rolelist)
    {
        var sb = new StringBuilder();

        foreach (var role in rolelist)
        {
            var roleNameOrMention = role.IsMentionable ? role.Mention : role.Name;
            var roleEmoji = role.Emoji?.Name ?? role.Icon ?? "";
            var roleId = role.Id.ToString();
            var createdAt = role.CreatedAt.UtcDateTime.ToString("d");

            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + roleNameOrMention.Length + roleEmoji.Length + roleId.Length + createdAt.Length + 25) break;

            sb.AppendLine($"**{(string.IsNullOrWhiteSpace(roleEmoji) ? "" : $"\\{roleEmoji} ")}{roleNameOrMention}**");
            sb.AppendLine("Id: " + roleId);
            sb.AppendLine("Created at: " + createdAt);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("**\\🧻 Role List**")
            .WithDescription(sb.Length == 0 ? "No roles found \\🥲" : sb.ToString())
            .Build();
    }

    public static Embed GetAuditLogListEmbed(IEnumerable<RestAuditLogEntry> auditloglist)
    {
        var sb = new StringBuilder();

        foreach (var entry in auditloglist)
        {
            var userMention = entry.User.Mention;
            var entryId = entry.Id.ToString();
            var entryAction = entry.Action.ToString();
            var createdAt = entry.CreatedAt.UtcDateTime.ToString("d");
            var reason = entry.Reason ?? "-";

            if (EmbedBuilder.MaxDescriptionLength <= sb.Length + userMention.Length + 20 + entryId.Length + createdAt.Length + reason.Length + 25) break;

            sb.AppendLine($"**{userMention}**");
            sb.AppendLine("Id: " + entryId);
            sb.AppendLine("Action: " + entryAction);
            sb.AppendLine("Reason: " + reason);
            sb.AppendLine("Created at: " + createdAt);
            sb.AppendLine();
        }

        return new MobyEmbedBuilder()
            .WithTitle("\\📜 Audit Log List")
            .WithDescription(sb.Length == 0 ? "No audit log entries found \\🥲" : sb.ToString())
            .Build();
    }

    public static Embed GetMessageInfoEmbed(IMessage message)
    {
        var sb = new StringBuilder($"**Id:** {message.Id}\n");

        sb.AppendLine("**Source:** " + message.Source);
        sb.AppendLine("**Author:** " + message.Author.Mention);
        sb.AppendLine("**Sent:** " + message.Timestamp.ToString("G"));
        sb.AppendLine("**Last edited:** " + (message.EditedTimestamp.HasValue ? message.EditedTimestamp.Value.ToString("G") : "-"));
        sb.AppendLine("**Pinned:** " + (message.IsPinned ? "Yes" : "No"));
        sb.AppendLine("**Text-To-Speech:** " + (message.IsTTS ? "Yes" : "No"));
        sb.AppendLine("**Jump-Url:** " + message.GetJumpUrl());
        sb.AppendLine("**Flags:** " + message.Flags);

        if (message.Reference?.GuildId.IsSpecified ?? false) sb.AppendLine("**Reference Guild Id:** " + message.Reference.GuildId.Value);
        if (message.Reference?.ChannelId is not null or 0) sb.AppendLine("**Reference Channel Id:** " + message.Reference.ChannelId);
        if (message.Reference?.MessageId.IsSpecified ?? false) sb.AppendLine("**Reference Message Id:** " + message.Reference.MessageId.Value);

        if (sb.Length > EmbedBuilder.MaxDescriptionLength) sb.Length = EmbedBuilder.MaxDescriptionLength;

        return new MobyEmbedBuilder()
            .WithDescription(sb.ToString())
            .Build();
    }

    public static Embed GetUserAvatarEmbed(IUser user)
    {
        var sb = new StringBuilder();

        var iterations = user.AvatarId.StartsWith("a_") ? 4 : 3;

        for (int i = 1; i <= iterations; i++)
        {
            sb.Append($"**{(ImageFormat)i}:** ");

            int counter = 0;
            foreach (var avatar in user.GetAllAvatarResolutions((ImageFormat)i))
            {
                sb.Append($"[{avatar.Size}]({avatar.Url}) | ");

                counter++;
            }

            if (counter == 0) sb.Append('-');
            else sb.Length -= 3;

            sb.Append('\n');
        }

        sb.Length--;

        return new MobyEmbedBuilder()
            .WithTitle($"**{user.Username}'s Avatar**")
            .WithDescription(sb.ToString())
            .WithImageUrl(user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl())
            .Build();
    }

    public static Embed GetDiceRollsEmbed(params int[] rolls)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🎲 Dice rolls**");

        for (int i = 0; i < rolls.Length; i++)
        {
            builder.AddField($"\\🎲 Dice #{i + 1}", $"```\nRolled a {rolls[i]}\n```", i % 4 != 0);
        }

        if (rolls.Length > 1) builder.AddField($"**\\🎲 Total**", $"```\n{rolls.Sum()}\n```");

        return builder.Build();
    }

    public static Embed GetCoinflipEmbed(bool isHeads)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🪙 Coinflip**")
            .WithDescription($"You flipped **{(isHeads ? "Heads" : "Tails")}**")
            .Build();
    }

    public static MessageComponent GetCoinflipComponent()
    {
        return new ComponentBuilder()
            .WithButton("Flip again", Moby.CoinflipAgainCId, ButtonStyle.Primary, new Emoji("🪙"))
            .Build();
    }

    public static Embed GetTopServerListEmbed(IEnumerable<SocketGuild> guilds)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🏆 Top 10 servers**");

        var sb = new StringBuilder();

        int iteration = 0;
        foreach (var guild in guilds.OrderByDescending(x => x.MemberCount))
        {
            iteration++;

            if (iteration == 11) break;

            if (iteration == 1) sb.Append("\\🥇 ");
            if (iteration == 2) sb.Append("\\🥈 ");
            if (iteration == 3) sb.Append("\\🥉 ");

            if (iteration > 3) sb.Append($"**{iteration}.** ");

            sb.AppendLine($"**Member:** {guild.MemberCount} - **Name:** {guild.Name}");
        }

        sb.Length--;

        return builder
            .WithDescription(sb.ToString())
            .WithCurrentTimestamp()
            .Build();
    }

    public static Embed GetColorEmbed(Color color)
    {
        var hsv = color.ToHsv();
        var hsl = color.ToHsl();
        var cmyk = color.ToCmyk();
        var ycbcr = color.ToYCbCr();

        return new EmbedBuilder()
            .WithColor(color)
            .WithTitle("**\\🌈 Color**")
            .AddField("Hex", color.ToHex())
            .AddField("RGB", $"{color.R}, {color.G}, {color.B}")
            .AddField("HSV", $"{hsv.H.Round(0)}°, {hsv.S.Round(0)}%, {hsv.V.Round(0)}%")
            .AddField("HSL", $"{hsl.H.Round(0)}°, {hsl.S.Round(0)}%, {hsl.L.Round(0)}%")
            .AddField("CMYK", $"{cmyk.C.Round(0)}%, {cmyk.M.Round(0)}%, {cmyk.Y.Round(0)}%, {cmyk.K.Round(0)}%")
            .AddField("YCbCr", $"{ycbcr.Y}, {ycbcr.Cb}, {ycbcr.Cr}")
            .Build();
    }

    public static IEnumerable<Embed> GetRandomColorEmbeds(int amount)
    {
        var rgb = new byte[3 * amount];

        Random.Shared.NextBytes(rgb);

        Color color;
        for (int i = 0; i < rgb.Length; i += 3)
        {
            color = new Color(rgb[i], rgb[i + 1], rgb[i + 2]);

            yield return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($"**Hex:** {color.ToHex()} - **RGB:** {color.R}, {color.G}, {color.B}")
                .Build();
        }
    }

    public static Color GetRandomColor()
    {
        Span<byte> rgb = new byte[3];

        Random.Shared.NextBytes(rgb);

        return new Color(rgb[0], rgb[1], rgb[2]);
    }

    public static Embed GetColorQuizEmbed(ColorQuizColor color)
    {
        return new EmbedBuilder()
            .WithTitle("**\\🌈 Color Quiz**")
            .WithColor(color.Hex.TryGetColor() ?? Moby.Color)
            .WithImageUrl($"https://singlecolorimage.com/get/{color.Hex.Replace("#", "")}/150x100.png")
            .Build();
    }

    public static MessageComponent GetColorQuizComponent(ColorQuizColor[] colors)
    {
        var builder = new ComponentBuilder();

        var correctColor = colors[0];

        Random.Shared.Shuffle(colors);

        var wrongIterations = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == correctColor) builder.WithButton(colors[i].Name, Moby.ColorQuizCorrectAnswerCId, ButtonStyle.Secondary, Moby.QuizEmojis[i], row: i % 2 == 0 ? 0 : 1);
            else
            {
                wrongIterations++;

                builder.WithButton(colors[i].Name, wrongIterations switch
                {
                    1 => Moby.ColorQuizWrongAnswerCId1,
                    2 => Moby.ColorQuizWrongAnswerCId2,
                    3 => Moby.ColorQuizWrongAnswerCId3,
                    _ => ""
                }, ButtonStyle.Secondary, Moby.QuizEmojis[i], row: i % 2 == 0 ? 0 : 1);
            }
        }

        return builder.Build();
    }

    public static Embed GetAnimeQuoteEmbed(AnimeQuote quote)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\💮 Anime Quote**")
            .WithDescription($"> \"{quote.Quote.DiscordFormat()}\"\n\n» {quote.Character.DiscordFormat()}\n« {quote.Anime.DiscordFormat()}")
            .Build();
    }

    public static Embed GetEightBallEmbed(string question, string answer)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🎱 8ball**")
            .WithDescription($"**You asked:** {question.DiscordFormat()}\n**The answer is:** {answer}")
            .Build();
    }

    public static Embed GetBanInfoEmbed(RestBan ban)
    {
        return new MobyEmbedBuilder()
            .WithThumbnailUrl(ban.User.GetAvatarUrl(size: 2048) ?? ban.User.GetDefaultAvatarUrl())
            .WithTitle("**\\⛔ Ban Info**")
            .WithDescription($"{ban.User.Username} #{ban.User.Discriminator}\nId: {ban.User.Id}\nReason: {ban.Reason}")
            .Build();
    }

    public static Embed GetUserMutedEmbed(SocketGuildUser user, string? reason)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🔇 Muted** " + user.Username);

        if (!string.IsNullOrWhiteSpace(reason)) builder.WithDescription("Reason: " + reason);

        return builder.Build();
    }

    public static Embed GetUserDeafenedEmbed(SocketGuildUser user, string? reason)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("**\\🔇 Deafened** " + user.Username);

        if (!string.IsNullOrWhiteSpace(reason)) builder.WithDescription("Reason: " + reason);

        return builder.Build();
    }

    public static Embed GetUserUnmutedEmbed(SocketGuildUser user)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🔈 Unmuted** " + user.Username)
            .Build();
    }

    public static Embed GetUserUndeafenedEmbed(SocketGuildUser user)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🔈 Undeafened** " + user.Username)
            .Build();
    }

    public static Embed GetRoleInfoEmbed(SocketRole role)
    {
        var permissions = role.Permissions.ToList();

        return new EmbedBuilder()
            .WithColor(role.Color)
            .WithTitle($"**{(string.IsNullOrWhiteSpace(role.Emoji.Name) ? "" : $"\\{role.Emoji.Name} ")}Information about {role.Name}**")
            .WithThumbnailUrl(role.GetIconUrl() ?? Moby.ImageNotFound)
            .AddField("Position", role.Position)
            .AddField("Permissions", permissions.Count == 0 ? "None" : string.Join("\n", permissions.OrderByDescending(x => x)))
            .Build();
    }

    public static Embed GetRandomFactEmbed(string fact)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\📖 Fact**")
            .WithDescription($"> {fact.DiscordFormat()}")
            .Build();
    }

    public static Embed GetFactOfTheDayEmbed(string fact)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\📖 Fact of the day**")
            .WithDescription($"> {fact.DiscordFormat()}")
            .Build();
    }

    public static Embed GetEncodingEmbed(string plainText, string encodedText, string encoding, TimeSpan time)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"**\\🔒 {encoding}**")
            .AddField("Input text", plainText)
            .AddField("Encoded text", $"```\n{encodedText}\n```")
            .AddField("Elapsed milliseconds", time.TotalMilliseconds)
            .Build();
    }

    public static Embed GetDecodingEmbed(string encodedText, string decodedText, string encoding, TimeSpan time)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"**\\🔑 {encoding}**")
            .AddField("Input text", encodedText)
            .AddField("Decoded text", $"```\n{decodedText}\n```")
            .AddField("Elapsed milliseconds", time.TotalMilliseconds)
            .Build();
    }

    public static Embed GetRandomMemberEmbed(IGuildUser member)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🍀 Random Member**")
            .WithThumbnailUrl(member.GetAvatarUrl(size: 2048) ?? member.GetDefaultAvatarUrl())
            .WithDescription($"Picked member: || **{member.Mention}** ||")
            .Build();
    }

    public static Embed GetRandomNumberEmbed(int number)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🍀 Random Number**")
            .WithDescription($"Picked number: || **{number}** ||")
            .Build();
    }

    public static IEnumerable<Embed> GetRandomNumberEmbeds(int amount, int lowest, int highest)
    {
        for (var i = 0; i < amount; i++)
        {
            yield return new MobyEmbedBuilder()
                .WithDescription($"**Number:** {Random.Shared.Next(lowest, highest)}")
                .Build();
        }
    }

    public static Embed GetRandomRoleEmbed(SocketRole role)
    {
        return new EmbedBuilder()
            .WithTitle("**\\🍀 Random Role**")
            .WithColor(role.Color)
            .WithDescription($"Picked role: || **{(string.IsNullOrWhiteSpace(role.Emoji.Name) ? "" : $"\\{role.Emoji.Name} ")}{(role.IsMentionable ? role.Mention : role.Name)}** ||")
            .Build();
    }

    public static Embed GetRandomValueEmbed(string value)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\🍀 Random Value**")
            .WithDescription($"Picked value: || **{value}** ||")
            .Build();
    }

    public static IEnumerable<Embed> GetRandomValueEmbeds(string[] values, int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            yield return new MobyEmbedBuilder()
                .WithDescription($"**Value:** {values.Random()}")
                .Build();
        }
    }

    public static FileAttachment GetBanListAttachment(IEnumerable<RestBan> banlist)
    {
        var sb = new StringBuilder();

        foreach (var ban in banlist)
        {
            sb.Append(ban.User.Id);
            sb.Append(" - ");
            sb.Append(ban.User.Username);
            sb.Append('#');
            sb.Append(ban.User.Discriminator);
            sb.Append(" - ");
            sb.AppendLine(ban.Reason);
        }

        if (sb.Length == 0) return new FileAttachment("No users are currently banned".ToStream(), "banlist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "banlist.txt");
    }

    public static FileAttachment GetBoosterListAttachment(IEnumerable<SocketGuildUser> boosterlist)
    {
        var sb = new StringBuilder();

        foreach (var boost in boosterlist)
        {
            sb.Append(boost.Id);
            sb.Append(" - ");
            sb.Append(boost.Username);
            sb.Append('#');
            sb.Append(boost.Discriminator);
            sb.Append(" - ");
            sb.AppendLine(boost.PremiumSince!.Value.UtcDateTime.ToString("d"));
        }

        if (sb.Length == 0) return new FileAttachment("No users are currently boosting".ToStream(), "boosterlist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "boosterlist.txt");
    }

    public static FileAttachment GetChannelListAttachment(IEnumerable<SocketGuildChannel> channellist)
    {
        var sb = new StringBuilder();

        foreach (var channel in channellist)
        {
            sb.Append(channel.Id);
            sb.Append(" - ");
            sb.Append(channel.Name);
            sb.Append(" - ");
            sb.Append(channel.GetChannelType() ?? ChannelType.Text);
            sb.Append(" - ");
            sb.AppendLine(channel.CreatedAt.UtcDateTime.ToString("d"));
        }

        if (sb.Length == 0) return new FileAttachment("No channel found (how is that even possible?)".ToStream(), "channellist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "channellist.txt");
    }

    public static FileAttachment GetEmoteListAttachment(IEnumerable<GuildEmote> emotelist)
    {
        var sb = new StringBuilder();

        foreach (var emote in emotelist)
        {
            sb.Append(emote.Id);
            sb.Append(" - ");
            sb.Append(emote.Name);
            sb.Append(" - ");
            sb.Append(emote.CreatedAt.UtcDateTime.ToString("d"));
            sb.Append(" - ");
            sb.AppendLine(emote.Url);
        }

        if (sb.Length == 0) return new FileAttachment("No emotes found".ToStream(), "emotelist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "emotelist.txt");
    }

    public static FileAttachment GetRoleListAttachment(IEnumerable<SocketRole> rolelist)
    {
        var sb = new StringBuilder();

        foreach (var role in rolelist)
        {
            sb.Append(role.Id);
            sb.Append(" - ");
            sb.Append(role.Name);
            sb.Append(" - ");
            sb.Append(role.CreatedAt.UtcDateTime.ToString("d"));

            if (role.Emoji?.Name is null && role.Icon is null)
            {
                sb.AppendLine();
                continue;
            }

            sb.Append(" - ");
            sb.AppendLine(role.Emoji?.Name ?? role.Icon ?? "");
        }

        if (sb.Length == 0) return new FileAttachment("No roles found".ToStream(), "rolelist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "rolelist.txt");
    }

    public static FileAttachment GetAuditLogListAttachment(IEnumerable<RestAuditLogEntry> auditloglist)
    {
        var sb = new StringBuilder();

        foreach (var entry in auditloglist)
        {
            sb.Append(entry.Id);
            sb.Append(" - ");
            sb.Append(entry.User.Username);
            sb.Append('#');
            sb.Append(entry.User.Discriminator);
            sb.Append(" - ");
            sb.Append(entry.Action);
            sb.Append(" - ");
            sb.Append(entry.CreatedAt.UtcDateTime.ToString("d"));
            sb.Append(" - ");
            sb.AppendLine(entry.Reason ?? "");
        }

        if (sb.Length == 0) return new FileAttachment("No audit log entries found".ToStream(), "auditloglist.txt");

        sb.Length--;

        return new FileAttachment(sb.ToString().ToStream(), "auditloglist.txt");
    }

    public static Embed GetMemberCountEmbed(int membercount, bool includingBots)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\👥 Member Count**")
            .WithDescription($"The current member count {(includingBots ? "" : "excluding bots ")}is **{membercount}**")
            .Build();
    }

    public static Embed GetKickDmEmbed(SocketUser user, SocketGuild guild, string? reason)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\⛔ You got kicked**")
            .WithThumbnailUrl(guild.IconUrl ?? Moby.ImageNotFound)
            .WithDescription($"{user.Username}#{user.Discriminator} kicked you from {guild.Name}\nReason: {(string.IsNullOrWhiteSpace(reason) ? "-" : reason)}")
            .Build();
    }

    public static Embed GetKickEmbed(SocketUser kickedUser, string? reason)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\⛔ Kicked User**")
            .WithThumbnailUrl(kickedUser.GetAvatarUrl(size: 2048) ?? kickedUser.GetDefaultAvatarUrl())
            .WithDescription($"User: {kickedUser.Username}#{kickedUser.Discriminator}\nReason: {(string.IsNullOrWhiteSpace(reason) ? "-" : reason)}")
            .Build();
    }

    public static Embed GetBanDmEmbed(SocketUser user, SocketGuild guild, string? reason)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\⛔ You got banned**")
            .WithThumbnailUrl(guild.IconUrl ?? Moby.ImageNotFound)
            .WithDescription($"{user.Username}#{user.Discriminator} banned you from {guild.Name}\nReason: {(string.IsNullOrWhiteSpace(reason) ? "-" : reason)}")
            .Build();
    }

    public static Embed GetBanEmbed(SocketUser kickedUser, string? reason)
    {
        return new MobyEmbedBuilder()
            .WithTitle("**\\⛔ Banned User**")
            .WithThumbnailUrl(kickedUser.GetAvatarUrl(size: 2048) ?? kickedUser.GetDefaultAvatarUrl())
            .WithDescription($"User: {kickedUser.Username}#{kickedUser.Discriminator}\nReason: {(string.IsNullOrWhiteSpace(reason) ? "-" : reason)}")
            .Build();
    }

    public static Embed GetPollEmbed(IUser user, string question, string?[] responses, Emoji[] emojis)
    {
        var sb = new StringBuilder($"**{question}**");
        sb.AppendLine();
        sb.AppendLine();

        for (var i = 0; i < responses.Length; i++)
        {
            sb.AppendLine($"{emojis[i]} {responses[i]}");
        }

        sb.Length--;

        return new MobyEmbedBuilder()
            .WithTitle("**\\📊 Poll**")
            .WithDescription(sb.ToString())
            .WithFooter($"Poll by {user.Username}", user.GetAvatarUrl(size: 2048) ?? user.GetDefaultAvatarUrl())
            .Build();
    }
}