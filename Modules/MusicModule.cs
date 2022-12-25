namespace Moby.Modules;

using global::Moby.Common;
using global::Moby.Services;

public sealed class MusicModule : MobyModuleBase
{
    private readonly IMobyLogger _logger;

    public MusicModule(IMobyLogger logger) : base(logger)
    {
        _logger = logger;
    }
}