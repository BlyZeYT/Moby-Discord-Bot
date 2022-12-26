namespace Moby;

using Discord.Interactions;
using Discord.WebSocket;
using global::Moby.Services;
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
    private readonly LavaNode _lavaNode;

    public MusicHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider, ConsoleLogger console, IMobyLogger logger, LavaNode lavaNode)
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
        _lavaNode.OnStatsReceived += OnStatsReceived;
        _lavaNode.OnTrackStart += OnTrackStart;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnTrackException += OnTrackException;
        _lavaNode.OnTrackEnd += OnTrackEnd;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;

        return Task.CompletedTask;
    }

    private async Task OnStatsReceived(StatsEventArg arg)
    {

    }

    private async Task OnTrackStart(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {

    }

    private async Task OnTrackStuck(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {

    }

    private async Task OnTrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {

    }

    private async Task OnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {

    }

    private async Task OnWebSocketClosed(WebSocketClosedEventArg arg)
    {

    }
}