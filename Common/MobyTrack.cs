namespace Moby.Common;

using Discord;
using Victoria;
using Victoria.Player;

public sealed class MobyTrack : LavaTrack
{
    public IGuildUser RequestAuthor { get; }
    public string ArtworkUrl { get; }

    private MobyTrack(IGuildUser requestAuthor, string? artworkUrl, LavaTrack lavaTrack) : base(lavaTrack)
    {
        RequestAuthor = requestAuthor;
        ArtworkUrl = string.IsNullOrWhiteSpace(artworkUrl) ? Moby.ImageNotFound : artworkUrl;
    }

    public static async ValueTask<MobyTrack> LoadAsync(IGuildUser requestAuthor, LavaTrack lavaTrack)
        => new MobyTrack(requestAuthor, await lavaTrack.FetchArtworkAsync(), lavaTrack);

    public static async IAsyncEnumerable<MobyTrack> LoadAsync(IGuildUser requestAuthor, IEnumerable<LavaTrack> lavaTracks)
    {
        foreach (var track in lavaTracks)
        {
            yield return await LoadAsync(requestAuthor, track);
        }
    }
}