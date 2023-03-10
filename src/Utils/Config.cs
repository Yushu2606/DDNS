using System.Text.Json.Serialization;

namespace SNDD.Utils;

internal readonly record struct Config
{
    public string AccessKeyId { get; init; }
    public string AccessKeySecret { get; init; }
    public Domain[] Domains { get; init; }
    public readonly record struct Domain
    {
        public required string Name { get; init; }
        public required string[] SubDomains { get; init; }
    }
    public double Interval { get; init; }
}

public readonly record struct Data
{
    [JsonPropertyName("ip")]
    public required string IP { get; init; }
    [JsonPropertyName("type")]
    public string Type { get; init; }
    [JsonPropertyName("subtype")]
    public string Subtype { get; init; }
    [JsonPropertyName("via")]
    public string Via { get; init; }
    [JsonPropertyName("padding")]
    public string Padding { get; init; }
}
