namespace Moby.Common;

using Discord;
using Victoria.Player;
using Victoria.WebSocket;

public sealed class MobyPlayer : LavaPlayer<MobyTrack>
{
    public bool IsRepeating { get; private set; }

    public MobyPlayer(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(socketClient, voiceChannel, textChannel)
    {
        IsRepeating = false;
    }

    public void SetRepeat(bool shouldRepeat)
        => IsRepeating = shouldRepeat;
}
