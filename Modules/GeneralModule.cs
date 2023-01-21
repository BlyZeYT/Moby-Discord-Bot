namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Common;
using Services;
using System.Data;
using System.Diagnostics;
using System.Text;

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
    public async Task ChuckNorrisAsync([Summary("category", "Choose the category of the joke")] ChuckNorrisJokeCategory category = ChuckNorrisJokeCategory.None) 
    {
        await DeferAsync(ephemeral: true);

        var joke = await _http.GetChuckNorrisJokeAsync(category);

        var isEmpty = joke.IsEmpty();

        if (!isEmpty && joke.Category is ChuckNorrisJokeCategory.Excplicit && !((ITextChannel)Context.Channel).IsNsfw)
        {
            await FollowupAsync("This joke contained Nsfw content but this isn't a Nsfw channel", ephemeral: true);
            return;
        }

        await FollowupAsync(embed: MobyUtil.GetChuckNorrisJokeEmbed(isEmpty ?
            new ChuckNorrisJoke("Chuck Norris is so damn badass he won't allow me to send a joke at the moment.",
            ChuckNorrisJokeCategory.None) : joke), ephemeral: true);
    }

    [SlashCommand("top", "Get a list of the largest server where I'm on")]
    public async Task TopAsync()
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetTopServerListEmbed(_client.Guilds));
    }

    [SlashCommand("animequote", "Get a random anime quote")]
    public async Task AnimequoteAsync()
    {
        await DeferAsync(ephemeral: true);

        var quote = await _http.GetAnimeQuoteAsync();

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetAnimeQuoteEmbed(quote.IsEmpty() ? new AnimeQuote("Discord", "Moby", "I think it's enough for now my friend. You should try again later") : quote));
    }

    [SlashCommand("8ball", "Get an 8ball like answer to your question")]
    public async Task EightBallAsync([Summary("question", "The question you want the answer to")] [MinLength(1)] [MaxLength(100)] string question)
    {
        await DeferAsync(ephemeral: true);

        var answer = await _http.GetEightBallAnswerAsync(question, Random.Shared.Next(0, 2) == 0);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetEightBallEmbed(question, string.IsNullOrWhiteSpace(answer) ? "Ask again later" : answer));
    }

    [SlashCommand("fact", "Get a random fact")]
    public async Task FactAsync([Summary("today", "Do you want todays random fact?")] Answer today = Answer.No)
    {
        await DeferAsync(ephemeral: true);

        var fact = await _http.GetFactAsync(today is Answer.Yes);

        await FollowupAsync(ephemeral: true,
            embed: today is Answer.No
            ? MobyUtil.GetRandomFactEmbed(string.IsNullOrWhiteSpace(fact) ? "Sometimes...there are no facts." : fact)
            : MobyUtil.GetFactOfTheDayEmbed(string.IsNullOrWhiteSpace(fact) ? "Sometimes...there are no facts." : fact));
    }

    [SlashCommand("hash", "Hash, encode or decode the provided text")]
    public async Task HashAsync([Summary("method", "Choose the hashing method")] HashMethod method,
        [Summary("text", "Enter the text to hash, encode or decode")] [MinLength(1)] [MaxLength(500)] string text)
    {
        await DeferAsync(ephemeral: true);

        var sw = Stopwatch.StartNew();

        string hashed = method switch
        {
            HashMethod.Base64Encode => Crypto.ToBase64(text),
            HashMethod.Base64Decode => Crypto.FromBase64(text),
            HashMethod.MD5 => Crypto.ToMD5(text),
            HashMethod.SHA1 => Crypto.ToSHA1(text),
            HashMethod.SHA256 => Crypto.ToSHA256(text),
            HashMethod.SHA384 => Crypto.ToSHA384(text),
            HashMethod.SHA512 => Crypto.ToSHA512(text),
            _ => ""
        };

        sw.Stop();

        await FollowupAsync(
            ephemeral: true,
            embed: method.IsDecode()
            ? MobyUtil.GetDecodingEmbed(text, hashed, method.GetString(), sw.Elapsed)
            : MobyUtil.GetEncodingEmbed(text, hashed, method.GetString(), sw.Elapsed));
    }

    [SlashCommand("membercount", "Count all server members and bots")]
    public async Task MembercountAsync([Summary("exclude-bots", "Yes if you want to exclude bots")] Answer excludebots = Answer.No)
    {
        await DeferAsync(ephemeral: true);

        if (excludebots is Answer.Yes)
        {
            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetMemberCountEmbed((await Context.Guild.GetUsersAsync().FlattenAsync()).Where(x => !(x.IsBot || x.IsWebhook)).Count(), false));
            return;
        }

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetMemberCountEmbed(Context.Guild.MemberCount, true));
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

    [SlashCommand("trivia", "Get a question to test your general knowledge")]
    public async Task TriviaAsync([Summary("difficulty", "The difficulty of the question")] TriviaQuestionDifficulty difficulty = TriviaQuestionDifficulty.Random)
    {
        await DeferAsync(ephemeral: true);

        var question = await _http.GetTriviaQuestionAsync(difficulty);

        if (question.IsEmpty())
        {
            await FollowupAsync("Couldn't get a trivia question", ephemeral: true);
            return;
        }

        var embed = MobyUtil.GetTriviaEmbed(question);

        if (question is MultipleChoiceQuestion)
        {
            await FollowupAsync(ephemeral: true, embed: embed, components: MobyUtil.GetMultipleChoiceQuestionComponent((MultipleChoiceQuestion)question));
            return;
        }

        await FollowupAsync(ephemeral: true, embed: embed, components: MobyUtil.GetTrueOrFalseQuestionComponent((TrueOrFalseQuestion)question));
    }

    [Group("color", "Commands with colors")]
    [Discord.Commands.Name("Color Group Commands")]
    public sealed class ColorGroupCommands : MobyModuleBase
    {
        public ColorGroupCommands(ConsoleLogger console) : base(console) { }

        [SlashCommand("rgb", "Get information about the provided RGB color")]
        public async Task ColorRgbAsync([Summary("red", "The red color amount")] [MinValue(0)] [MaxValue(255)] int r,
            [Summary("green", "The green color amount")] [MinValue(0)] [MaxValue(255)] int g,
            [Summary("blue", "The blue color amount")] [MinValue(0)] [MaxValue(255)] int b)
        {
            await DeferAsync(ephemeral: true);

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetColorEmbed(new Color(r, g, b)));
        }

        [SlashCommand("hex", "Get information about the provided Hex color")]
        public async Task ColorHexAsync([Summary("hex", "Enter the Hex color value")] [MinLength(6)] [MaxLength(7)] string hex)
        {
            await DeferAsync(ephemeral: true);

            var color = hex.TryGetColor();

            if (color.HasValue)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetColorEmbed(color.Value));
                return;
            }

            await FollowupAsync("I can't get a color out of that Hex value", ephemeral: true);
        }

        [SlashCommand("quiz", "Guess the name of the shown color")]
        public async Task ColorQuizAsync()
        {
            await DeferAsync(ephemeral: true);

            var randoms = new ColorQuizColor[4];

            ColorQuizColor color;
            for (var i = 0; i < 4; i++)
            {
                do
                {
                    color = Moby.ColorQuizInfo[Random.Shared.Next(0, Moby.ColorQuizInfo.Length - 1)];
                } while (randoms.Contains(color));

                randoms[i] = color;
            }

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetColorQuizEmbed(randoms[0]), components: MobyUtil.GetColorQuizComponent(randoms));
        }
    }

    [Group("random", "Commands with Random")]
    [Discord.Commands.Name("Random Group Commands")]
    public sealed class RandomGroupCommands : MobyModuleBase
    {
        private readonly IHttpService _http;

        public RandomGroupCommands(IHttpService http, ConsoleLogger console) : base(console)
        {
            _http = http;
        }

        [SlashCommand("member", "Pick a random user from this server")]
        public async Task RandomMemberAsync([Summary("role", "Mention the role under which the user will be selected")] SocketRole? role = null,
            [Summary("include-bots", "Do you want to include Bots?")] Answer includebots = Answer.No,
            [Summary("only-boosters", "Do you want only to include Server Boosters?")] Answer onlyboosters = Answer.No)
        {
            await DeferAsync(ephemeral: true);

            var users = await Context.Guild.GetUsersAsync().FlattenAsync();

            if (role is not null) users = users.Where(x => x.RoleIds.Contains(role.Id));

            if (includebots is Answer.No) users = users.Where(x => !(x.IsBot || x.IsWebhook));

            if (onlyboosters is Answer.Yes) users = users.Where(x => x.PremiumSince is not null);

            if (!users.Any())
            {
                await FollowupAsync("Couldn't find a member on this server that satisfies all conditions", ephemeral: true);
                return;
            }

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRandomMemberEmbed(users.Random()));
        }

        [SlashCommand("role", "Pick a random role from this server")]
        public async Task RandomRoleAsync([Summary("include-everyone", "Do you want to include @everyone role?")] Answer includeeveryone = Answer.No,
            [Summary("from-user", "Only include roles that the mentioned user has")] SocketGuildUser? user = null)
        {
            await DeferAsync(ephemeral: true);

            var roles = user is null ? Context.Guild.Roles.AsEnumerable() : user.Roles.AsEnumerable();

            if (includeeveryone is Answer.No) roles = roles.Where(x => x != Context.Guild.EveryoneRole);

            if (!roles.Any())
            {
                await FollowupAsync("Couldn't find a role on this server that satisfies all conditions", ephemeral: true);
                return;
            }

            await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRandomRoleEmbed(roles.Random()));
        }

        [SlashCommand("number", "Pick one or multiple random numbers")]
        public async Task RandomNumberAsync([Summary("amount", "How many random numbers should be generated")] [MinValue(1)] [MaxValue(10)] int amount = 1,
            [Summary("lowest", "Enter the lowest possible number that could get picked")] [MinValue(0)] [MaxValue(int.MaxValue - 2)] int lowest = 0,
            [Summary("highest", "Enter the highest possible number that could get picked")] [MinValue(1)] [MaxValue(int.MaxValue - 1)] int highest = int.MaxValue - 1)
        {
            await DeferAsync(ephemeral: true);

            if (amount == 1)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRandomNumberEmbed(Random.Shared.Next(lowest, highest + 1)));
                return;
            }

            await FollowupAsync(ephemeral: true, embeds: MobyUtil.GetRandomNumberEmbeds(amount, lowest, highest + 1).ToArray());
        }

        [SlashCommand("color", "Pick one or multiple random colors")]
        public async Task RandomColorAsync([Summary("amount", "How many random colors should be generated")] [MinValue(1)] [MaxValue(10)] int amount = 1)
        {
            await DeferAsync(ephemeral: true);

            if (amount == 1)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetColorEmbed(MobyUtil.GetRandomColor()));
                return;
            }

            await FollowupAsync(ephemeral: true, embeds: MobyUtil.GetRandomColorEmbeds(amount).ToArray());
        }

        [SlashCommand("file", "Pick one or multiple random values from a file")]
        public async Task RandomWordAsync([Summary("file", "The file to pick the random value from")] IAttachment file,
            [Summary("seperator", "The seperator of the values")] [MinLength(1)] [MaxLength(10)] string seperator = "\n",
            [Summary("amount", "How many random values should be picked from the file")] [MinValue(1)] [MaxValue(10)] int amount = 1)
        {
            await DeferAsync(ephemeral: true);

            var text = await _http.GetTextFromUrlAsync(file.Url);

            if (string.IsNullOrWhiteSpace(text))
            {
                await FollowupAsync("Couldn't get any text out of the attachment", ephemeral: true);
                return;
            }

            string[] splitted = text.Split(seperator);

            if (amount == 1)
            {
                await FollowupAsync(ephemeral: true, embed: MobyUtil.GetRandomValueEmbed(splitted.Random()));
                return;
            }

            await FollowupAsync(ephemeral: true, embeds: MobyUtil.GetRandomValueEmbeds(splitted, amount).ToArray());
        }
    }
}