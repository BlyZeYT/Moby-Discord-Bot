namespace Moby.Common;

public sealed record DatabaseGuildInfo
{
    public int Id { get; }

    public ulong GuildId { get; }

    public Prefix Prefix { get; }

    public bool IsRepeatOn { get; }

    public DatabaseGuildInfo(int id, ulong guildId, Prefix prefix, bool isRepeatOn)
    {
        Id = id;
        GuildId = guildId;
        Prefix = prefix;
        IsRepeatOn = isRepeatOn;
    }

    public bool IsEmpty() => Id == -1 && GuildId == 0 && Prefix.Value == "null" && !IsRepeatOn;

    public static DatabaseGuildInfo Empty() => new(-1, 0, Prefix.Create("null"), false);
}