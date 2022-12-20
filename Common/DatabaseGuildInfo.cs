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
}