namespace Moby.Common;

using Discord;
using Victoria.Player;

public sealed class MobyTrack : LavaTrack
{
    public IGuildUser RequestAuthor { get; }

    public MobyTrack(IGuildUser requestAuthor, LavaTrack lavaTrack) : base(lavaTrack)
    {
        RequestAuthor = requestAuthor;
    }
}