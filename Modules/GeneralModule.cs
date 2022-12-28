namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

public sealed class GeneralModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly IHttpService _http;

    public GeneralModule(DiscordSocketClient client, IHttpService http, ConsoleLogger consoleLogger) : base(consoleLogger)
    {
        _client = client;
        _http = http;
    }

    [SlashCommand("help", "Get help for all my functions")]
    [RequireContext(ContextType.Guild)]
    public async Task HelpAsync()
    {

    }

    [SlashCommand("serverinfo", "Get information about the server")]
    [RequireContext(ContextType.Guild)]
    public async Task ServerInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetServerInfo(Context.Guild));

    [SlashCommand("userinfo", "Get information about a user")]
    [RequireContext(ContextType.Guild)]
    public async Task UserInfoAsync([Summary("mention", "Mention a user")] SocketGuildUser? user = null)
        => await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetUserInfo(user ?? (SocketGuildUser)Context.User));

    [UserCommand("userinfo")]
    [RequireContext(ContextType.Guild)]
    public async Task ContextUserInfoAsync(SocketGuildUser user)
        => await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetUserInfo(user));

    [SlashCommand("botinfo", "Get information about me")]
    [RequireContext(ContextType.Guild)]
    public async Task BotInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetBotInfo(Context.Guild.CurrentUser, _client.Guilds.Count));

    [SlashCommand("reddit", "Get a post from Reddit")]
    [RequireContext(ContextType.Guild)]
    public async Task RedditAsync([Summary("subreddit", "Enter a subreddit")] string subreddit)
    {
        var post = await _http.GetRedditPostAsync(subreddit);

        if (post.IsNsfw && ((ITextChannel)Context.Channel).IsNsfw)
        {
            await RespondAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetRedditPost(post));
    }

    [SlashCommand("meme", "Get a random meme from reddit")]
    [RequireContext(ContextType.Guild)]
    public async Task MemeAsync()
    {
        var post = await _http.GetMemeAsync();

        if (post.IsNsfw && ((ITextChannel)Context.Channel).IsNsfw)
        {
            await RespondAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await RespondAsync(ephemeral: true, embed: MobyEmbeds.GetRedditPost(post));
    }
}