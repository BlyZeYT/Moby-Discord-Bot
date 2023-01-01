namespace Moby.Modules;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Common;
using global::Moby.Services;

[RequireOwner]
[RequireContext(ContextType.Guild)]
public sealed class RuntimeModule : MobyModuleBase
{
    private readonly DiscordSocketClient _client;
    private readonly IDatabase _database;

    public RuntimeModule(DiscordSocketClient client, IDatabase database, ConsoleLogger console) : base(console)
    {
        _client = client;
        _database = database;
    }

    [SlashCommand("announce", "Announces something on all servers")]
    public async Task AnnounceAsync()
    {
        if (Context.Channel.Id is not Moby.RuntimeCommandsChannelId)
        {
            await RespondAsync("This is not the right channel", ephemeral: true);

            return;
        }
        
        await RespondWithModalAsync(MobyUtil.GetAnnouncementModal());
    }
}