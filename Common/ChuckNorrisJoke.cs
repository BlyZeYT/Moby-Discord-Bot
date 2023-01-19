namespace Moby.Common;

public sealed record ChuckNorrisJoke
{
    public string Value { get; }
    public ChuckNorrisJokeCategory Category { get; }

    public ChuckNorrisJoke(string value, ChuckNorrisJokeCategory category)
    {
        Value = value;
        Category = category;
    }

    public bool IsEmpty()
        => string.IsNullOrWhiteSpace(Value) && Category is ChuckNorrisJokeCategory.None;

    public static ChuckNorrisJoke Empty()
        => new("", ChuckNorrisJokeCategory.None);
}