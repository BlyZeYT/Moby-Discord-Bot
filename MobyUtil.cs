namespace Moby;

using Discord;
using Discord.WebSocket;
using global::Moby.Common;
using Microsoft.Extensions.Logging;
using System;

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
            builder.WithTitle(exception.Message)
                .AddField("Exception type", exception.GetType().FullName ?? "Unknown")
                .AddField("Source", exception.Source ?? "Unknown")
                .WithCurrentTimestamp();
        }

        return builder.Build();
    }

    public static Embed GetServerInfoEmbed(SocketGuild guild)
    {
        return new MobyEmbedBuilder()
            .WithTitle($"Information about {guild.Name}")
            .WithThumbnailUrl(guild.IconUrl ?? Moby.ImageNotFound)
            .AddField("Created at", guild.CreatedAt.ToString("d"))
            .AddField("Member Count", guild.MemberCount)
            .AddField("Member Online", guild.Users.Count(x => x.Status is UserStatus.Online))
            .AddField("Boosts", guild.PremiumSubscriptionCount)
            .Build();
    }

    public static Embed GetUserInfoEmbed(SocketGuildUser user)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle($"Information about {user.Username}")
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
            .WithTitle("Information about me")
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
            .WithTitle(post.Title)
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

    public static Embed GetFeedbackModalEmbed(SocketMessageComponentData[] data)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("Provided Feedback 📝")
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

    public static Embed GetIdeaModalEmbed(SocketMessageComponentData[] data)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("Submitted idea 💡")
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

    public static Embed GetBugModalEmbed(SocketMessageComponentData[] data)
    {
        var builder = new MobyEmbedBuilder()
            .WithTitle("Bug Report 🐞")
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
}