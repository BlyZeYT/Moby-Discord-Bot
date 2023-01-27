namespace Moby.Enums;

using Discord.Interactions;

public enum HashMethod
{
    [ChoiceDisplay("🔒 Base64 Encode")]
    Base64Encode,
    [ChoiceDisplay("🔑 Base64 Decode")]
    Base64Decode,
    [ChoiceDisplay("🔒 MD5")]
    MD5,
    [ChoiceDisplay("🔒 SHA1")]
    SHA1,
    [ChoiceDisplay("🔒 SHA256")]
    SHA256,
    [ChoiceDisplay("🔒 SHA384")]
    SHA384,
    [ChoiceDisplay("🔒 SHA512")]
    SHA512
}