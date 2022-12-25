namespace Moby.Modules;

using Discord.Interactions;
using global::Moby.Common;
using global::Moby.Services;

public sealed class GeneralModule : MobyModuleBase
{
    private readonly IMobyLogger _logger;

    public GeneralModule(IMobyLogger logger) : base(logger)
    {
        _logger = logger;
    }

    [SlashCommand("test", "Test Command")]
    private async Task Test()
    {
        await ReplyAsync("Test worked!");
    }
}