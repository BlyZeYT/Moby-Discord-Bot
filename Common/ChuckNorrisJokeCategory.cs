namespace Moby.Common;

using Discord.Interactions;

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