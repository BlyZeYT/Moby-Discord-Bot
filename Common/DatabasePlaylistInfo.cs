namespace Moby.Common;

public sealed record DatabasePlaylist
{
    public int Id { get; }

    public string Name { get; }

    public ulong GuildId { get; }

    public DatabasePlaylist(int id, string name, ulong guildId)
    {
        Id = id;
        Name = name;
        GuildId = guildId;
    }
}