namespace Moby.Common;

using Discord.Interactions;
using global::Moby.Services;
using System.Threading.Tasks;

public abstract class MobyModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ConsoleLogger _console;

    public MobyModuleBase(ConsoleLogger console)
    {
        _console = console;
    }

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        _console.LogDebug($"Now executing: {command.Name} - {command.Module.Name}");

        await base.BeforeExecuteAsync(command);

        using IDisposable typing = Context.Channel.EnterTypingState();
    }

    public override async Task AfterExecuteAsync(ICommandInfo command)
    {
        await base.AfterExecuteAsync(command);

        _console.LogDebug($"Executing completed: {command.Name} - {command.Module.Name}");
    }
}