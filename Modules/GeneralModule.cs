namespace Moby.Modules;

using Discord.Interactions;
using global::Moby.Common;
using global::Moby.Services;

public sealed class GeneralModule : MobyModuleBase
{
    public GeneralModule(ConsoleLogger consoleLogger) : base(consoleLogger) { }

    [SlashCommand("testcmd", "Test Command")]
    private async Task TestAsync()
    {
        await RespondAsync("Test worked!");
    }
}