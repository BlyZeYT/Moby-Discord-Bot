namespace Moby.Common;

public sealed class FactOfTheDay
{
    private readonly DateTime _gotFactTime;

    public string Text { get; }

    public FactOfTheDay(string text)
    {
        _gotFactTime = DateTime.UtcNow.AddHours(24);

        Text = text;
    }

    public bool IsEmptyOrOutdated()
        => string.IsNullOrWhiteSpace(Text) || _gotFactTime <= DateTime.UtcNow;
}