namespace Moby.Common;

public sealed record DbGuild
{
    public int Id { get; }

    public ulong GuildId { get; }

    public bool IsRepeatOn { get; }

    public DbGuild(int id, ulong guildId, bool isRepeatOn)
    {
        Id = id;
        GuildId = guildId;
        IsRepeatOn = isRepeatOn;
    }

    public bool IsEmpty() => Id == -1 && GuildId == 0 && !IsRepeatOn;

    public static DbGuild Empty() => new(-1, 0, false);
}