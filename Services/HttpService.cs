namespace Moby.Services;

using ChuckNorrisApi;
using global::Moby.Common;
using Newtonsoft.Json.Linq;

public interface IHttpService
{
    public ValueTask<RedditPost> GetMemeAsync();

    public ValueTask<RedditPost> GetRedditPostAsync(string subreddit);

    public ValueTask<ChuckNorrisJoke> GetChuckNorrisJokeAsync(NorrisJokeCategory category);

    public ValueTask<bool> IsUrlEmpty(string url);
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

    public async ValueTask<ChuckNorrisJoke> GetChuckNorrisJokeAsync(NorrisJokeCategory category)
    {
        try
        {
            dynamic json = JObject.Parse(await _client.GetStringAsync($"https://api.chucknorris.io/jokes/random{GetEndpoint(category)}"));

            _console.LogDebug("Reddit Post was returned successfully");

            return new ChuckNorrisJoke(json.value.ToString(), category is NorrisJokeCategory.Excplicit);
        }
        catch (Exception ex)
        {
            _console.LogError("Something went wrong, attempting to get a Chuck Norris Meme", ex);
            return ChuckNorrisJoke.Empty();
        }
    }

    public async ValueTask<bool> IsUrlEmpty(string url)
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

    private static string GetEndpoint(NorrisJokeCategory category)
    {
        return category switch
        {
            NorrisJokeCategory.Animal => "?category=animal",
            NorrisJokeCategory.Career => "?category=career",
            NorrisJokeCategory.Celebrity => "?category=celebrity",
            NorrisJokeCategory.Dev => "?category=dev",
            NorrisJokeCategory.Excplicit => "?category=explicit",
            NorrisJokeCategory.Fashion => "?category=fashion",
            NorrisJokeCategory.Food => "?category=food",
            NorrisJokeCategory.History => "?category=history",
            NorrisJokeCategory.Money => "?category=money",
            NorrisJokeCategory.Movie => "?category=movie",
            NorrisJokeCategory.Music => "?category=music",
            NorrisJokeCategory.Political => "?category=political",
            NorrisJokeCategory.Religion => "?category=religion",
            NorrisJokeCategory.Science => "?category=science",
            NorrisJokeCategory.Sport => "?category=sport",
            NorrisJokeCategory.Travel => "?category=travel",
            _ => "",
        };
    }
}