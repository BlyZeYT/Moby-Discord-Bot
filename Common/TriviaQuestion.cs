namespace Moby.Common;

using Discord.Interactions;

public record TriviaQuestion
{
    public TriviaQuestionDifficulty Difficulty { get; }
    public string Question { get; }

    public TriviaQuestion(TriviaQuestionDifficulty difficulty, string question)
    {
        Difficulty = difficulty;
        Question = question;
    }

    public static TriviaQuestion Empty()
        => new(TriviaQuestionDifficulty.Random, "");

    public bool IsEmpty()
        => Difficulty == TriviaQuestionDifficulty.Random && string.IsNullOrWhiteSpace(Question);
}

public sealed record TrueOrFalseQuestion : TriviaQuestion
{
    public bool CorrectAnswer { get; }
    public bool IncorrectAnswer { get; }

    public TrueOrFalseQuestion(TriviaQuestion original, bool correctAnswer, bool incorrectAnswer) : base(original)
    {
        CorrectAnswer = correctAnswer;
        IncorrectAnswer = incorrectAnswer;
    }
}

public sealed record MultipleChoiceQuestion : TriviaQuestion
{
    public string CorrectAnswer { get; }
    public string[] IncorrectAnswers { get; }

    public MultipleChoiceQuestion(TriviaQuestion original, string correctAnswer, string[] incorrectAnswers) : base(original)
    {
        CorrectAnswer = correctAnswer;
        IncorrectAnswers = incorrectAnswers;
    }
}

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