namespace Moby.Modules;

using Common;
using Discord;
using Discord.Interactions;
using Services;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode<LavaPlayer<MobyTrack>, MobyTrack> _lava;

    public MusicModule(LavaNode<LavaPlayer<MobyTrack>, MobyTrack> lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }

    [SlashCommand("youtube", "Play anything from YouTube")]
    public async Task YouTubeAsync([Summary("query", "A link from YouTube or a search term")] [MinLength(1)] [MaxLength(250)] string query)
    {
        await DeferAsync();

        if (MusicUtil.IsInvalidVoiceState((IVoiceState)Context.User))
        {
            await FollowupAsync("You have to be in a voice channel and not deafened");
            return;
        }

        
    }
}