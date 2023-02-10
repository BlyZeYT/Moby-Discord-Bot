namespace Moby.Common;

using Discord;
using Victoria.Player;

public sealed class MobyTrack : LavaTrack
{
    public IGuild Guild { get; }
    public IGuildUser RequestAuthor { get; }
    public bool IsLooped { get; set; }

    public MobyTrack(IGuild guild, IGuildUser requestAuthor, LavaTrack lavaTrack) : base(lavaTrack)
    {
        Guild = guild;
        RequestAuthor = requestAuthor;
        IsLooped = false;
    }
}