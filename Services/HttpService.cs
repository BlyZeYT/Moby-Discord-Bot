namespace Moby.Services;

using global::Moby.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public interface IHttpService
{
    public ValueTask<RedditPost> GetMemeAsync();

    public ValueTask<RedditPost> GetRedditPostAsync(string subreddit);

    public ValueTask<ChuckNorrisJoke> GetChuckNorrisJokeAsync(ChuckNorrisJokeCategory category);

    public ValueTask<ColorQuizColor[]> GetColorQuizInfoAsync();

    public ValueTask<AnimeQuote> GetAnimeQuoteAsync();

    public ValueTask<string> GetEightBallAnswerAsync(string question, bool lucky);

    public ValueTask<string> GetFactAsync(bool today);

    public ValueTask<string> GetTextFromUrlAsync(string url);

    public ValueTask<TriviaQuestion> GetTriviaQuestionAsync(TriviaQuestionDifficulty difficulty);

    public ValueTask<bool> IsUrlEmptyAsync(string url);
}

public sealed class HttpService : IHttpService
{
    private readonly HttpClient _client;
    private readonly ConsoleLogger _console;

    public HttpService(ConsoleLogger console)
    {
        _client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        _console = console;
    }

    public async ValueTask<RedditPost> GetMemeAsync() => await GetRedditPostAsync("memes");

    public async ValueTask<RedditPost> GetRedditPostAsync(string subreddit)
    {
        _console.LogDebug($"Attempting to get a Reddit Post for Subreddit: {subreddit}");

        string result;

        try
        {
            result = await _client.GetStringAsync($"https://reddit.com/r/{subreddit}/random.json?limit=1");
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a Reddit Post", ex);
            return RedditPost.Empty();
        }

        if (!result.StartsWith('['))
        {
            _console.LogWarning($"Couldn't get a Reddit Post because the Subreddit: {subreddit} did not exist");

            return RedditPost.Empty();
        }

        var array = JArray.Parse(result);
        var post = JObject.Parse(array[0]["data"]!["children"]![0]!["data"]!.ToString());

        _console.LogDebug("Reddit Post was returned successfully");

        return new RedditPost(
            $"{post["url"]}",
            $"{post["title"]}",
            $"{post["permalink"]}",
            $"{post["num_comments"]}",
            $"{post["ups"]}",
            post["over_18"]?.ToString() is "True");
    }

    public async ValueTask<ChuckNorrisJoke> GetChuckNorrisJokeAsync(ChuckNorrisJokeCategory category)
    {
        try
        {
            dynamic json = JObject.Parse(await _client.GetStringAsync($"https://api.chucknorris.io/jokes/random{GetEndpoint(category)}"));

            _console.LogDebug("Chuck Norris Joke was returned successfully");

            return new ChuckNorrisJoke(json.value.ToString(), category);
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a Chuck Norris Joke", ex);
            return ChuckNorrisJoke.Empty();
        }
    }

    public async ValueTask<ColorQuizColor[]> GetColorQuizInfoAsync()
    {
        try
        {
            var json = await _client.GetStringAsync("https://raw.githubusercontent.com/cheprasov/json-colors/master/colors.json");

            return JsonConvert.DeserializeObject<ColorQuizColor[]>(json) ?? throw new Exception("The deserialized Color Quiz Json was null");
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get the Color Quiz colors", ex);
            return Array.Empty<ColorQuizColor>();
        }
    }

    public async ValueTask<AnimeQuote> GetAnimeQuoteAsync()
    {
        try
        {
            dynamic json = JObject.Parse(await _client.GetStringAsync("https://animechan.vercel.app/api/random"));

            _console.LogDebug("Anime Quote was returned successfully");

            return new AnimeQuote(json.anime.ToString(), json.character.ToString(), json.quote.ToString());
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a Anime Quote", ex);

            return AnimeQuote.Empty();
        }
    }

    public async ValueTask<string> GetEightBallAnswerAsync(string question, bool lucky)
    {
        try
        {
            dynamic json = JObject.Parse(await _client.GetStringAsync($"https://www.eightballapi.com/api/biased?question={question.Replace(' ', '+')}?&lucky={lucky}"));

            _console.LogDebug("8ball answer was returned successfully");

            return json.reading.ToString();
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a 8ball answer", ex);

            return "";
        }
    }

    public async ValueTask<string> GetFactAsync(bool today)
    {
        try
        {
            if (today)
            {
                if (!Moby.FactOfTheDay.IsEmptyOrOutdated()) return Moby.FactOfTheDay.Text;
            }

            dynamic json = JObject.Parse(await _client.GetStringAsync($"https://uselessfacts.jsph.pl/{(today ? "today" : "random")}.json?language=en"));

            _console.LogDebug("Fact was returned successfully");

            return json.text.ToString();
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a fact", ex);

            return "";
        }
    }

    public async ValueTask<string> GetTextFromUrlAsync(string url)
    {
        try
        {
            var text = await _client.GetStringAsync(url);

            _console.LogDebug("Attachment text was returned successfully");

            return text;
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get the attachment text", ex);

            return "";
        }
    }

    public async ValueTask<TriviaQuestion> GetTriviaQuestionAsync(TriviaQuestionDifficulty difficulty)
    {
        try
        {
            dynamic json = JObject.Parse(await _client.GetStringAsync("https://opentdb.com/api.php?amount=1&category=9&encode=base64" + GetEndpoint(difficulty)));

            if (json.response_code != 0) throw new Exception("Response code was not 0");

            json = JArray.Parse(json.results.ToString());

            json = JObject.Parse(json[0].ToString());

            difficulty = difficulty is TriviaQuestionDifficulty.Random ? FromString(FromBase64String(json.difficulty.ToString())) : difficulty;

            var question = new TriviaQuestion(difficulty, FromBase64String(json.question.ToString()));

            _console.LogDebug("Trivia question was returned successfully");

            return FromBase64String(json.type.ToString()) == "multiple"
                ? new MultipleChoiceQuestion(question, FromBase64String(json.correct_answer.ToString()), new string[]
                {
                    FromBase64String(json.incorrect_answers[0].ToString()),
                    FromBase64String(json.incorrect_answers[1].ToString()),
                    FromBase64String(json.incorrect_answers[2].ToString())
                }) : new TrueOrFalseQuestion(question, bool.Parse(FromBase64String(json.correct_answer.ToString())));
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a trivia question", ex);

            return TriviaQuestion.Empty();
        }
    }

    public async ValueTask<bool> IsUrlEmptyAsync(string url)
    {
        try
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            return !response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetEndpoint(ChuckNorrisJokeCategory category)
    {
        return category switch
        {
            ChuckNorrisJokeCategory.Animal => "?category=animal",
            ChuckNorrisJokeCategory.Career => "?category=career",
            ChuckNorrisJokeCategory.Celebrity => "?category=celebrity",
            ChuckNorrisJokeCategory.Dev => "?category=dev",
            ChuckNorrisJokeCategory.Excplicit => "?category=explicit",
            ChuckNorrisJokeCategory.Fashion => "?category=fashion",
            ChuckNorrisJokeCategory.Food => "?category=food",
            ChuckNorrisJokeCategory.History => "?category=history",
            ChuckNorrisJokeCategory.Money => "?category=money",
            ChuckNorrisJokeCategory.Movie => "?category=movie",
            ChuckNorrisJokeCategory.Music => "?category=music",
            ChuckNorrisJokeCategory.Political => "?category=political",
            ChuckNorrisJokeCategory.Religion => "?category=religion",
            ChuckNorrisJokeCategory.Science => "?category=science",
            ChuckNorrisJokeCategory.Sport => "?category=sport",
            ChuckNorrisJokeCategory.Travel => "?category=travel",
            _ => "",
        };
    }

    private static string GetEndpoint(TriviaQuestionDifficulty difficulty)
    {
        return difficulty switch
        {
            TriviaQuestionDifficulty.Easy => "&difficulty=easy",
            TriviaQuestionDifficulty.Medium => "&difficulty=medium",
            TriviaQuestionDifficulty.Hard => "&difficulty=hard",
            _ => ""
        };
    }

    private static TriviaQuestionDifficulty FromString(string str)
    {
        return str.ToLower() switch
        {
            "easy" => TriviaQuestionDifficulty.Easy,
            "medium" => TriviaQuestionDifficulty.Medium,
            "hard" => TriviaQuestionDifficulty.Hard,
            _ => TriviaQuestionDifficulty.Random
        };
    }

    private static string FromBase64String(string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
}