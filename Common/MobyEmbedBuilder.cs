namespace Moby.Common;

using Discord;

public sealed class MobyEmbedBuilder : EmbedBuilder
{
    public MobyEmbedBuilder()
    {
        WithColor(Moby.Color);
    }
}