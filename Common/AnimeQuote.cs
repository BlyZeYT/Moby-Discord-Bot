namespace Moby.Common;

public sealed record AnimeQuote
{
    public string Anime { get; }
    public string Character { get; }
    public string Quote { get; }

    public AnimeQuote(string anime, string character, string quote)
    {
        Anime = anime;
        Character = character;
        Quote = quote;
    }

    public bool IsEmpty()
        => Anime == "" && Character == "" && Quote == "";

    public static AnimeQuote Empty()
        => new("", "", "");
}