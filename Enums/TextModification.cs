namespace Moby.Enums;

using Discord.Interactions;

public enum TextModification
{
    [ChoiceDisplay("📝 Italic")]
    Italic,
    [ChoiceDisplay("📝 Bold")]
    Bold,
    [ChoiceDisplay("📝 Bold & Italic")]
    BoldItalic,
    [ChoiceDisplay("📝 Underline")]
    Underline,
    [ChoiceDisplay("📝 Underline & Italic")]
    UnderlineItalic,
    [ChoiceDisplay("📝 Underline & Bold")]
    UnderlineBold,
    [ChoiceDisplay("📝 Underline & Bold & Italic")]
    UnderlineBoldItalic,
    [ChoiceDisplay("📝 Strikethrough")]
    Strikethrough,
    [ChoiceDisplay("📝 Spoiler")]
    Spoiler,
    [ChoiceDisplay("📝 Single Code Line")]
    SingleCode,
    [ChoiceDisplay("📝 Multiple Code Lines")]
    MultiCode,
    [ChoiceDisplay("📝 Single Line Quote")]
    SingleQuote,
    [ChoiceDisplay("📝 Multi Line Quote")]
    MultiQuote
}