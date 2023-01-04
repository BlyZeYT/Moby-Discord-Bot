namespace Moby.Modules;

using Discord.Interactions;
using global::Moby.Common;
using global::Moby.Services;
using Victoria.Node;

[RequireContext(ContextType.Guild)]
[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode _lava;

    public MusicModule(LavaNode lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }
}