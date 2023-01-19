namespace Moby;

using Discord;
using global::Moby.Common;
using Microsoft.Extensions.Logging;

public static class Moby
{
    public const string LogoNormalUrl = "https://i.imgur.com/Za1cXgs.png";
    public const string LogoRoundUrl = "https://i.imgur.com/0OuzsfG.png";
    public const string ImageNotFound = "https://i.imgur.com/excUKal.png";

    public const string ContactMenuCId = "contact-menu";

    public const string ContactMenuFeedbackCId = "contact-menu-feedback";
    public const string ContactMenuIdeaCId = "contact-menu-idea";
    public const string ContactMenuBugCId = "contact-menu-bug";

    public const string FeedbackModalCId = "contact-feedback-modal";
    public const string FeedbackModalTopicCId = "contact-feedback-modal-topic";
    public const string FeedbackModalDescriptionCId = "contact-feedback-modal-description";
    public const string FeedbackModalContactCId = "contact-feedback-modal-contact";

    public const string IdeaModalCId = "contact-idea-modal";
    public const string IdeaModalTopicCId = "contact-idea-modal-topic";
    public const string IdeaModalDescriptionCId = "contact-idea-modal-description";
    public const string IdeaModalContactCId = "contact-idea-modal-contact";

    public const string BugModalCId = "contact-bug-modal";
    public const string BugModalCommandCId = "contact-feedback-modal-command";
    public const string BugModalReproductionCId = "contact-feedback-modal-reproduction";
    public const string BugModalDescriptionCId = "contact-feedback-modal-description";
    public const string BugModalContactCId = "contact-feedback-modal-contact";

    public const string AnnouncementModalCId = "announcement-modal";
    public const string AnnouncementModalTitleCId = "announcement-modal-title";
    public const string AnnouncementModalMessageCId = "announcement-modal-message";

    public const string DenyInvitationButtonCId = "invitation-deny";

    public const string CoinflipAgainCId = "flip-a-coin-again";

    public const string ColorQuizCorrectAnswerCId = "color-quiz-correct-answer";
    public const string ColorQuizWrongAnswerCId1 = "color-quiz-wrong-answer-1";
    public const string ColorQuizWrongAnswerCId2 = "color-quiz-wrong-answer-2";
    public const string ColorQuizWrongAnswerCId3 = "color-quiz-wrong-answer-3";

    public const string TrueOrFalseCorrectAnswerCId = "true-or-false-question-correct-answer";
    public const string TrueOrFalseIncorrectAnswerCId = "true-or-false-question-wrong-answer";

    public const string MultipleChoiceCorrectAnswerCId = "multiple-choice-question-correct-answer";
    public const string MultipleChoiceIncorrectAnswerCId1 = "multiple-choice-question-wrong-answer-1";
    public const string MultipleChoiceIncorrectAnswerCId2 = "multiple-choice-question-wrong-answer-2";
    public const string MultipleChoiceIncorrectAnswerCId3 = "multiple-choice-question-wrong-answer-3";

    public const ulong InformationChannelId = 1058214987170058312;
    public const ulong ContactChannelId = 1057784058517667881;
    public const ulong LogsChannelId = 1053392480034357432;
    public const ulong OwnerCommandsChannelId = 1053399251138396190;
    public const ulong RuntimeCommandsChannelId = 1059255114847752282;

    public const string OnlyMobyGuildModule = "only-moby-guild-module";

    public static Color Color { get; }

    public static IDictionary<LogLevel, Tuple<Color, Emoji>> LogLevels { get; }

    public static ColorQuizColor[] ColorQuizInfo { get; set; }

    public static Emoji[] QuizEmojis { get; }

    public static FactOfTheDay FactOfTheDay { get; set; }

    static Moby()
    {
        Color = new Color(34, 141, 255);

        LogLevels = new Dictionary<LogLevel, Tuple<Color, Emoji>>()
        {
            { LogLevel.Trace, new Tuple<Color, Emoji>(new Color(240, 240, 240), new Emoji("👀")) },
            { LogLevel.Debug, new Tuple<Color, Emoji>(new Color(200, 200, 200), new Emoji("🕸️")) },
            { LogLevel.Information, new Tuple<Color, Emoji>(Color, new Emoji("ℹ️")) },
            { LogLevel.Warning, new Tuple<Color, Emoji>(new Color(255, 229, 33), new Emoji("⚠️")) },
            { LogLevel.Error, new Tuple<Color, Emoji>(new Color(212, 0, 0), new Emoji("⛔")) },
            { LogLevel.Critical, new Tuple<Color, Emoji>(new Color(171, 12, 76), new Emoji("☢️")) }
        };

        QuizEmojis = new Emoji[]
        {
            "🇦", "🇧", "🇨", "🇩"
        };

        FactOfTheDay = new("");
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}