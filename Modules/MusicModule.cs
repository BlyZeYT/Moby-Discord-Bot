namespace Moby.Modules;

using Common;
using Services;
using Victoria.Node;
using Victoria.Player;

[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode<LavaPlayer<MobyTrack>, MobyTrack> _lava;

    public MusicModule(LavaNode<LavaPlayer<MobyTrack>, MobyTrack> lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }


}