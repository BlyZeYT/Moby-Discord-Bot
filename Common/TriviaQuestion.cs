namespace Moby.Common;

using Discord.Interactions;

public record TriviaQuestion
{
    public TriviaQuestionDifficulty Difficulty { get; }
    public string Question { get; }
    public string CorrectAnswer { get; }

    public TriviaQuestion(TriviaQuestionDifficulty difficulty, string question, string correctAnswer)
    {
        Difficulty = difficulty;
        Question = question;
        CorrectAnswer = correctAnswer;
    }

    public static TriviaQuestion Empty()
        => new(TriviaQuestionDifficulty.Easy, "", "");

    public bool IsEmpty()
        => Difficulty == TriviaQuestionDifficulty.Easy && Question == "" && CorrectAnswer == "";
}

public sealed record TrueOrFalseQuestion : TriviaQuestion
{
    public string IncorrectAnswer { get; }

    public TrueOrFalseQuestion(TriviaQuestion original, string incorrectAnswer) : base(original)
    {
        IncorrectAnswer = incorrectAnswer;
    }
}

public sealed record MultipleChoiceQuestion : TriviaQuestion
{
    public string[] IncorrectAnswers { get; }

    public MultipleChoiceQuestion(TriviaQuestion original, string[] incorrectAnswers) : base(original)
    {
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