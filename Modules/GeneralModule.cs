namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

public sealed class GeneralModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IHttpService _http;

    public GeneralModule(DiscordSocketClient client, InteractionService service, IHttpService http, ConsoleLogger consoleLogger) : base(consoleLogger)
    {
        _client = client;
        _service = service;
        _http = http;
    }

    [SlashCommand("help", "Get help for all my functions")]
    [RequireContext(ContextType.Guild)]
    public async Task HelpAsync()
    {

    }

    [SlashCommand("contact", "Contact my creator to submit ideas, give feedback or report bugs")]
    [RequireContext(ContextType.Guild)]
    public async Task ContactAsync()
        => await RespondAsync(ephemeral: true, components: MobyUtil.GetContactComponent());

    [SlashCommand("serverinfo", "Get information about the server")]
    [RequireContext(ContextType.Guild)]
    public async Task ServerInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetServerInfoEmbed(Context.Guild));

    [SlashCommand("userinfo", "Get information about a user")]
    [RequireContext(ContextType.Guild)]
    public async Task UserInfoAsync([Summary("mention", "Mention a user")] SocketGuildUser? user = null)
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user ?? (SocketGuildUser)Context.User));

    [UserCommand("userinfo")]
    [RequireContext(ContextType.Guild)]
    public async Task ContextUserInfoAsync(SocketGuildUser user)
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user));

    [SlashCommand("botinfo", "Get information about me")]
    [RequireContext(ContextType.Guild)]
    public async Task BotInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetBotInfoEmbed(Context.Guild.CurrentUser, _client.Guilds.Count));

    [SlashCommand("reddit", "Get a post from Reddit")]
    [RequireContext(ContextType.Guild)]
    public async Task RedditAsync([Summary("subreddit", "Enter a subreddit")] string subreddit)
    {
        var post = await _http.GetRedditPostAsync(subreddit);

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await RespondAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await RespondAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }

    [SlashCommand("meme", "Get a random meme from reddit")]
    [RequireContext(ContextType.Guild)]
    public async Task MemeAsync()
    {
        var post = await _http.GetMemeAsync();

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await RespondAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await RespondAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }
}