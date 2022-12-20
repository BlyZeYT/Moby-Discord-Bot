namespace Moby.Common;

public sealed record Prefix
{
    public string Value { get; }

    private Prefix(string value)
    {
        Value = value;
    }

    public static Prefix Create(string value)
    {
        return value.Length is > 10 or < 1
            ? throw new ArgumentException("The value for a prefix is too long or 0", nameof(value))
            : new Prefix(value);
    }

    public static bool TryCreate(string value, out Prefix prefix)
    {
        if (value.Length is > 10 or < 1)
        {
            prefix = default!;
            return false;
        }
        else
        {
            prefix = new Prefix(value);
            return true;
        }
    }

    public override string ToString() => Value;
}