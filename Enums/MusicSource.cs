namespace Moby.Enums;

using Discord.Interactions;

public enum MusicSource
{
    [ChoiceDisplay("💻 Local")]
    Local,
    [ChoiceDisplay("🌐 Link")]
    Url,
    [ChoiceDisplay("🔍 YouTube Search")]
    YouTube,
    [ChoiceDisplay("🔍 YouTubeMusic Search")]
    YouTubeMusic,
    [ChoiceDisplay("🔍 Soundcloud Search")]
    Soundcloud,
    [ChoiceDisplay("🔍 Twitch Search")]
    Twitch
}