namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireContext(ContextType.Guild)]
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
    public async Task HelpAsync()
    {

    }

    [SlashCommand("contact", "Contact my creator to give feedback, submit ideas or report bugs")]
    public async Task ContactAsync()
        => await RespondAsync(ephemeral: true, components: MobyUtil.GetContactComponent());

    [SlashCommand("botstats", "Get information about my system")]
    public async Task BotStatsAsync()
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetBotStatsEmbed(_client.CurrentUser, _client.Latency));

    [SlashCommand("serverinfo", "Get information about the server")]
    public async Task ServerInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetServerInfoEmbed(Context.Guild));

    [SlashCommand("userinfo", "Get information about the mentioned user or yourself")]
    public async Task UserInfoAsync([Summary("mention", "Mention the user you want information about")] SocketGuildUser? user = null)
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user ?? (SocketGuildUser)Context.User));

    [UserCommand("userinfo")]
    public async Task ContextUserInfoAsync(SocketGuildUser user)
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user));

    [SlashCommand("botinfo", "Get information about me")]
    public async Task BotInfoAsync()
        => await RespondAsync(ephemeral: true, embed: MobyUtil.GetBotInfoEmbed(Context.Guild.CurrentUser, _client.Guilds.Count));

    [SlashCommand("reddit", "Get a post from Reddit")]
    public async Task RedditAsync([Summary("subreddit", "Enter a subreddit from where the post should be fetched")] [MinLength(1)][MaxLength(100)] string subreddit)
    {
        await DeferAsync(ephemeral: true);

        var post = await _http.GetRedditPostAsync(subreddit);

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }

    [SlashCommand("meme", "Get a random meme from reddit")]
    public async Task MemeAsync()
    {
        await DeferAsync(ephemeral: true);

        var post = await _http.GetMemeAsync();

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This post contained Nsfw content but this isn't a Nsfw channel.", ephemeral: true);
            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }

    [SlashCommand("coinflip", "Flip a coin")]
    public async Task CoinflipAsync()
        => await RespondAsync($"You flipped {(Random.Shared.Next(0, 2) == 0 ? "Tails" : "Heads")} \\🪙", ephemeral: true);
}