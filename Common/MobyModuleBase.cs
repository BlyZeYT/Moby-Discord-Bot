namespace Moby.Common;

using Discord;
using Discord.Interactions;
using Services;
using System.Threading.Tasks;

[RequireContext(ContextType.Guild)]
[RequireBotPermission(ChannelPermission.ViewChannel)]
[RequireBotPermission(ChannelPermission.ReadMessageHistory)]
[RequireBotPermission(ChannelPermission.SendMessages)]
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

        using (IDisposable typing = Context.Channel.EnterTypingState())
        {
            await base.BeforeExecuteAsync(command);
        }
    }

    public override async Task AfterExecuteAsync(ICommandInfo command)
    {
        await base.AfterExecuteAsync(command);

        _console.LogDebug($"Executing completed: {command.Name} - {command.Module.Name}");
    }
}