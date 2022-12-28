namespace Moby;

using Discord;
using Discord.WebSocket;
using global::Moby.Common;
using Microsoft.Extensions.Logging;
using System;

public static class MobyEmbeds
{
    public static Embed GetLog(LogLevel logLevel, Exception? exception)
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

    public static Embed GetServerInfo(SocketGuild guild)
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

    public static Embed GetUserInfo(SocketGuildUser user)
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

    public static Embed GetBotInfo(SocketGuildUser bot, int serverCount)
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

    public static Embed GetRedditPost(RedditPost post)
    {
        return new MobyEmbedBuilder()
            .WithTitle(post.Title)
            .WithImageUrl(post.ImageUrl)
            .WithUrl($"https://reddit.com{post.Permalink}")
            .WithFooter($"🗨 {post.CommentsCount} ⬆️ {post.UpvotesCount}")
            .Build();
    }
}