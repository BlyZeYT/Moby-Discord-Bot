namespace Moby.Common;

using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

public sealed class RateLimitAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {

    }
}