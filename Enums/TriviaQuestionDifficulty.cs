namespace Moby.Enums;

using Discord.Interactions;

public enum TriviaQuestionDifficulty
{
    [ChoiceDisplay("🎲 Random")]
    Random,
    [ChoiceDisplay("📄 Easy")]
    Easy,
    [ChoiceDisplay("📙 Medium")]
    Medium,
    [ChoiceDisplay("📚 Hard")]
    Hard
}