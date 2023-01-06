namespace ChuckNorrisApi;

public sealed record ChuckNorrisJoke
{
    public string Value { get; }
    public bool IsExplicit { get; }

    public ChuckNorrisJoke(string value, bool isExplicit)
    {
        Value = value;
        IsExplicit = isExplicit;
    }

    public bool IsEmpty()
        => string.IsNullOrWhiteSpace(Value) && !IsExplicit;

    public static ChuckNorrisJoke Empty()
        => new("", false);
}