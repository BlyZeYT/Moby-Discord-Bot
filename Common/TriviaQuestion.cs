namespace Moby.Common;

using Discord.Interactions;

public record TriviaQuestion
{
    public TriviaQuestionDifficulty Difficulty { get; }
    public string Question { get; }
    public string Category { get; }

    public TriviaQuestion(TriviaQuestionDifficulty difficulty, string question, string category)
    {
        Difficulty = difficulty;
        Question = question;
        Category = category;
    }

    public static TriviaQuestion Empty()
        => new(TriviaQuestionDifficulty.Random, "", "");

    public bool IsEmpty()
        => Difficulty == TriviaQuestionDifficulty.Random && string.IsNullOrWhiteSpace(Question) && string.IsNullOrWhiteSpace(Category);
}

public sealed record TrueOrFalseQuestion : TriviaQuestion
{
    public bool CorrectAnswer { get; }

    public TrueOrFalseQuestion(TriviaQuestion original, bool correctAnswer) : base(original)
    {
        CorrectAnswer = correctAnswer;
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