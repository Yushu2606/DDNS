using System.Text.Json.Serialization;

namespace AliyunDynamicDomainNameServer.Utils;

internal record Config(string AccessKeyId, string AccessKeySecret, Domain[] Domains, int Interval);

public record Domain(string Name, string[] SubDomains);

public record Data(
    [property: JsonPropertyName("asdf_key")]
    string AsdfKey,
    [property: JsonPropertyName("wanConnList")]
    WanConnect[] WanConnects,
    string AreaCode,
    string AreaCode2,
    [property: JsonPropertyName("fh_method")]
    string FhMethod
);

public record WanConnect(
    [property: JsonPropertyName("oid")] string Oid,
    [property: JsonPropertyName("inst0")] string Inst0,
    [property: JsonPropertyName("inst1")] string Inst1,
    [property: JsonPropertyName("inst2")] string Inst2,
    string Name,
    string ConnectionType,
    string VlanMuxID,
    string VlanMux8021p,
    string IfName,
    string FirewallEnabled,
    string NATEnabled,
    string MuticastVlan,
    string IPv4Enabled,
    string? AddressingType,
    string ConnectionStatus,
    string LastConnectionError,
    string ExternalIPAddress,
    string MACAddress,
    string SubnetMask,
    string DefaultGateway,
    string DNSServers,
    string IGMPEnabled,
    string IPv6Enabled,
    string IPv6AddressingType,
    string IPv6ConnStatus,
    string IPv6ExternalAddress,
    string PrefixLength,
    string IPv6DefaultGateway,
    string IPv6DNSServers,
    string MLDEnabled,
    string IPv6Prefix
);