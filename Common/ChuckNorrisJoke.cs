namespace Moby.Common;

using Discord.Interactions;

public sealed record ChuckNorrisJoke
{
    public string Value { get; }
    public ChuckNorrisJokeCategory Category { get; }

    public ChuckNorrisJoke(string value, ChuckNorrisJokeCategory category)
    {
        Value = value;
        Category = category;
    }

    public bool IsEmpty()
        => string.IsNullOrWhiteSpace(Value) && Category is ChuckNorrisJokeCategory.None;

    public static ChuckNorrisJoke Empty()
        => new("", ChuckNorrisJokeCategory.None);
}

public enum ChuckNorrisJokeCategory
{
    [ChoiceDisplay("🎲 Random")]
    None,
    [ChoiceDisplay("🐳 Animal")]
    Animal,
    [ChoiceDisplay("👮 Career")]
    Career,
    [ChoiceDisplay("✨ Celebrity")]
    Celebrity,
    [ChoiceDisplay("💻 Developer")]
    Dev,
    [ChoiceDisplay("🔞 NSFW")]
    Excplicit,
    [ChoiceDisplay("👕 Fashion")]
    Fashion,
    [ChoiceDisplay("🍕 Food")]
    Food,
    [ChoiceDisplay("🎥 History")]
    History,
    [ChoiceDisplay("💵 Money")]
    Money,
    [ChoiceDisplay("🎬 Movie")]
    Movie,
    [ChoiceDisplay("🎵 Music")]
    Music,
    [ChoiceDisplay("🤵 Political")]
    Political,
    [ChoiceDisplay("⛪ Religion")]
    Religion,
    [ChoiceDisplay("🧬 Science")]
    Science,
    [ChoiceDisplay("🏃 Sport")]
    Sport,
    [ChoiceDisplay("🌏 Travel")]
    Travel
}