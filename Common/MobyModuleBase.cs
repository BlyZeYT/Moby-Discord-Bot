namespace Moby.Common;

using Discord.Interactions;
using global::Moby.Services;
using System.Threading.Tasks;

public abstract class MobyModuleBase : InteractionModuleBase
{
    private readonly IMobyLogger _logger;

    public MobyModuleBase(IMobyLogger logger)
    {
        _logger = logger;
    }

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        await base.BeforeExecuteAsync(command);

        await _logger.LogInformationAsync("Before command execution");
    }

    public override async Task AfterExecuteAsync(ICommandInfo command)
    {
        await base.AfterExecuteAsync(command);

        await _logger.LogInformationAsync("After command execution");
    }
}