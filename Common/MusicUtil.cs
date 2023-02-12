namespace Moby.Common;

using Discord;

public static class MusicUtil
{
    public static bool IsInvalidVoiceState(IVoiceState? voiceState)
        => voiceState?.VoiceChannel is null || voiceState.IsDeafened || voiceState.IsSelfDeafened;
}