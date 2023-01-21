namespace Moby.Modules;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Common;
using Services;

[RequireContext(ContextType.Guild)]
[Discord.Commands.Name("Context")]
public sealed class ContextModule : MobyModuleBase
{
    public ContextModule(ConsoleLogger console) : base(console) { }

    [UserCommand("Get User Info")]
    public async Task ContextUserInfoAsync(SocketGuildUser user)
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserInfoEmbed(user));
    }

    [UserCommand("Get Avatar")]
    public async Task ContextGetAvatarAsync(SocketGuildUser user)
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetUserAvatarEmbed(user));
    }

    [MessageCommand("Get Message Info")]
    public async Task ContextMessageInfoAsync(IMessage message)
    {
        await DeferAsync(ephemeral: true);

        await FollowupAsync(ephemeral: true, embed: MobyUtil.GetMessageInfoEmbed(message));
    }
}