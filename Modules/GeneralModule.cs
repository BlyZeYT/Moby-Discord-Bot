namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireContext(ContextType.Guild)]
[Discord.Commands.Name("General")]
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
        foreach (var module in _service.Modules.Where(x => x.Name != Moby.OnlyMobyGuildModule).OrderBy(x => x.Name))
        {
            foreach (var command in module.SlashCommands.OrderBy(x => x.Name))
            {
                
            }
        }
    }

    [SlashCommand("contact", "Contact my creator to give feedback, submit ideas or report bugs")]
    public async Task ContactAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, components: MobyUtil.GetContactComponent());
    }

    [SlashCommand("botstats", "Get information about my system")]
    public async Task BotStatsAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBotStatsEmbed(_client.CurrentUser, _client.Latency));
    }

    [SlashCommand("serverinfo", "Get information about the server")]
    public async Task ServerInfoAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetServerInfoEmbed(Context.Guild));
    }

    [SlashCommand("userinfo", "Get information about the mentioned user or yourself")]
    public async Task UserInfoAsync([Summary("user", "Mention the user you want information about")] SocketGuildUser? user = null)
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user ?? (SocketGuildUser)Context.User));
    }

    [SlashCommand("avatar", "Get the avatar of the mentioned user or yourself")]
    public async Task AvatarAsync([Summary("user", "Mention the user you want the avatar from")] SocketGuildUser? user = null)
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserAvatarEmbed(user ?? Context.User));
    }

    [SlashCommand("botinfo", "Get information about me")]
    public async Task BotInfoAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetBotInfoEmbed(Context.Guild.CurrentUser, _client.Guilds.Count));
    }

    [SlashCommand("reddit", "Get a post from Reddit")]
    public async Task RedditAsync([Summary("subreddit", "Enter a subreddit from where the post should be fetched")] [MinLength(1)] [MaxLength(100)] string subreddit)
    {
        await DeferAsync(ephemeral: true);

        var post = await _http.GetRedditPostAsync(subreddit);

        if (post.IsEmpty())
        {
            await FollowupAsync("Failed to get a Reddit post", ephemeral: true);
            return;
        }

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This post contained Nsfw content but this isn't a Nsfw channel", ephemeral: true);
            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }

    [SlashCommand("meme", "Get a random meme from reddit")]
    public async Task MemeAsync()
    {
        await DeferAsync(ephemeral: true);

        var post = await _http.GetMemeAsync();

        if (post.IsEmpty())
        {
            await FollowupAsync("Failed to get a meme", ephemeral: true);
            return;
        }

        if (post.IsNsfw && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This post contained Nsfw content but this isn't a Nsfw channel", ephemeral: true);
            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRedditPostEmbed(post));
    }

    [SlashCommand("coinflip", "Flip a coin")]
    public async Task CoinflipAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetCoinflipEmbed(Random.Shared.Next(0, 2) == 0), components: MobyUtil.GetCoinflipComponent());
    }

    [SlashCommand("dice", "Roll one or multiple dice")]
    public async Task DiceAsync([Summary("rolls", "How many dice should be rolled")] [MinValue(1)] [MaxValue(10)] int rolls = 1)
    {
        await DeferAsync(ephemeral: true);

        var diceRolls = new int[rolls];

        for (int i = 0; i < rolls; i++)
        {
            diceRolls[i] = Random.Shared.Next(1, 7);
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetDiceRollsEmbed(diceRolls));
    }

    [SlashCommand("chuck", "Get a random Chuck Norris joke")]
    public async Task ChuckNorrisAsync([Summary("category", "Choose the category of the joke")] NorrisJokeCategory category = NorrisJokeCategory.None)
    {
        await DeferAsync(ephemeral: true);

        var joke = await _http.GetChuckNorrisJokeAsync(category);

        if (joke.IsEmpty())
        {
            await FollowupAsync("Failed to get a Chuck Norris joke", ephemeral: true);
            return;
        }

        if (joke.IsExplicit && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This joke contained Nsfw content but this isn't a Nsfw channel", ephemeral: true);
            return;
        }

        await FollowupAsync(joke.Value, ephemeral: true);
    }

    [SlashCommand("top", "Get a list of the largest server where I'm in")]
    public async Task TopAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetTopServerListEmbed(_client.Guilds));
    }

    [Group("color", "Commands with colors")]
    [Discord.Commands.Name("Color Group Commands")]
    public sealed class ColorGroupCommands : MobyModuleBase
    {
        public ColorGroupCommands(ConsoleLogger console) : base(console) { }

        [SlashCommand("random", "Get one or multiple random colors")]
        public async Task ColorRandomAsync([Summary("amount", "How many random colors should be generated")] [MinValue(1)] [MaxValue(10)] int amount = 1)
        {
            await DeferAsync(ephemeral: true);

            if (amount == 1)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRandomColorEmbed());
                return;
            }

            await FollowupAsync(ephemeral: true, embeds: MobyUtil.GetRandomColorEmbeds(amount).ToArray());
        }

        [SlashCommand("rgb", "Get information about the provided RGB color")]
        public async Task ColorRgbAsync([Summary("red", "The red color amount")] [MinValue(0)] [MaxValue(255)] byte r,
            [Summary("green", "The green color amount")] [MinValue(0)] [MaxValue(255)] byte g,
            [Summary("blue", "The blue color amount")] [MinValue(0)] [MaxValue(255)] byte b)
        {
            await DeferAsync(ephemeral: true);

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetColorEmbed(new Color(r, g, b)));
        }
    }
}