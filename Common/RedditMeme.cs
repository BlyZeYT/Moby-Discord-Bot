namespace Moby.Common;

public sealed record RedditPost
{
    public string ImageUrl { get; }

    public string Title { get; }

    public string Permalink { get; }

    public string CommentsCount { get; }

    public string UpvotesCount { get; }

    public bool IsNsfw { get; }

    public RedditPost(string imageUrl, string title, string permalink, string commentsCount, string upvotesCount, bool isNsfw)
    {
        ImageUrl = imageUrl;
        Title = title;
        Permalink = permalink;
        CommentsCount = commentsCount;
        UpvotesCount = upvotesCount;
        IsNsfw = isNsfw;
    }

    public static RedditPost Empty() => new("", "", "", "", "", false);

    public bool IsEmpty() => ImageUrl == "" && Title == "" && Permalink == "" && CommentsCount == "" && UpvotesCount == "" && !IsNsfw;
}