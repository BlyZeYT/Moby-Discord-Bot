namespace Moby.Modules;

using global::Moby.Common;
using global::Moby.Services;
using Victoria.Node;

public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode _lava;

    public MusicModule(LavaNode lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }
}