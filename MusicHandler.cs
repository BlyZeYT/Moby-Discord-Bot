namespace Moby;

using Discord.Interactions;
using Discord.WebSocket;
using Common;
using Services;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

public sealed class MusicHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IServiceProvider _provider;
    private readonly ConsoleLogger _console;
    private readonly IMobyLogger _logger;
    private readonly LavaNode<MobyPlayer, LavaTrack> _lavaNode;

    public MusicHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider, ConsoleLogger console, IMobyLogger logger, LavaNode<MobyPlayer, LavaTrack> lavaNode)
    {
        _client = client;
        _service = service;
        _provider = provider;
        _console = console;
        _logger = logger;
        _lavaNode = lavaNode;
    }

    public Task InitializeAsync()
    {
        _lavaNode.OnStatsReceived += OnStatsReceivedAsync;
        _lavaNode.OnTrackStart += OnTrackStartAsync;
        _lavaNode.OnTrackStuck += OnTrackStuckAsync;
        _lavaNode.OnTrackException += OnTrackExceptionAsync;
        _lavaNode.OnTrackEnd += OnTrackEndAsync;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;

        return Task.CompletedTask;
    }

    private async Task OnStatsReceivedAsync(StatsEventArg stats)
        => await Stats.UpdateAsync(stats.Cpu.SystemLoad, stats.Memory.Allocated, stats.Frames.Sent, stats.Players, stats.Uptime);

    private async Task OnTrackStartAsync(TrackStartEventArg<MobyPlayer, LavaTrack> arg)
    {
        if (arg.Player.IsRepeating)
        {

        }

        await arg.Player.TextChannel.SendMessageAsync(embed: MobyUtil.GetTrackStartedEmbed(arg.Track, await arg.Track.GetArtworkOrDefault()));
    }

    private async Task OnTrackStuckAsync(TrackStuckEventArg<MobyPlayer, LavaTrack> arg)
    {

    }

    private async Task OnTrackExceptionAsync(TrackExceptionEventArg<MobyPlayer, LavaTrack> arg)
    {

    }

    private async Task OnTrackEndAsync(TrackEndEventArg<MobyPlayer, LavaTrack> arg)
    {

    }

    private async Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
    {

    }
}