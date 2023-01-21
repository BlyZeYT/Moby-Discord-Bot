namespace Moby.Modules;

using Discord.Interactions;
using Common;
using Services;
using Victoria.Node;

[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode _lava;

    public MusicModule(LavaNode lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }
}