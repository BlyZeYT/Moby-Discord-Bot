namespace Moby.Common;

using Newtonsoft.Json;

[Serializable]
public sealed record ColorQuizColor
{
    [JsonRequired]
    [JsonProperty("hex")]
    public string Hex { get; }
    [JsonRequired]
    [JsonProperty("name")]
    public string Name { get; }

    [JsonConstructor]
    public ColorQuizColor(string hex, string name)
    {
        Hex = hex;
        Name = name;
    }
}