using System.Text.Json.Serialization;

namespace AliyunDynamicDomainNameServer.Utils;

internal record Config(string AccessKeyId, string AccessKeySecret, Domain[] Domains, int Interval);

public record Domain(string Name, string[] SubDomains);

public record Data(
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("subtype")]
    string Subtype,
    [property: JsonPropertyName("via")] string Via,
    [property: JsonPropertyName("padding")]
    string Padding);