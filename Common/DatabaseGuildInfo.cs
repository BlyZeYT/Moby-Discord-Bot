namespace Moby.Common;

public sealed record DatabaseGuildInfo
{
    public int Id { get; }

    public ulong GuildId { get; }

    public bool IsRepeatOn { get; }

    public DatabaseGuildInfo(int id, ulong guildId, bool isRepeatOn)
    {
        Id = id;
        GuildId = guildId;
        IsRepeatOn = isRepeatOn;
    }

    public bool IsEmpty() => Id == -1 && GuildId == 0 && !IsRepeatOn;

    public static DatabaseGuildInfo Empty() => new(-1, 0, false);
}