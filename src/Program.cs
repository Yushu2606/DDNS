using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using AliyunDynamicDomainNameServer.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Json;
using Tea;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.Sources.Clear();
IHostEnvironment env = builder.Environment;
builder.Configuration.AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
if (!File.Exists("appsettings.json"))
{
    File.WriteAllText("appsettings.json", JsonSerializer.Serialize(new Config(string.Empty, string.Empty, [], 0),
        new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        }));
}

Config config = builder.Configuration.Get<Config>()!;
Client client = new(new()
{
    AccessKeyId = config.AccessKeyId,
    AccessKeySecret = config.AccessKeySecret
});
while (true)
{
    try
    {
        using HttpClient httpClient = new();
        string? ipv4 = default;
        string? ipv6 = default;
        try
        {
            string response = await httpClient.GetStringAsync("http://192.168.1.1/fh_get_wan_info.ajax");
            Data? data = JsonSerializer.Deserialize<Data>(response, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            });
            foreach (WanConnect wanConnect in data!.WanConnects)
            {
                if (wanConnect.IfName is not "ppp1.3")
                {
                    continue;
                }

                if (wanConnect.IPv4Enabled is "1")
                {
                    ipv4 = wanConnect.ExternalIPAddress;
                }

                if (wanConnect.IPv6Enabled is "1")
                {
                    ipv6 = wanConnect.IPv6ExternalAddress;
                }
            }
        }
        catch (Exception ex) when (ex is NullReferenceException or HttpRequestException)
        {
        }

        foreach (Domain domain in config.Domains)
        {
            foreach (string subDomain in domain.SubDomains)
            {
                DescribeDomainRecordsRequest request = new()
                {
                    DomainName = domain.Name,
                    RRKeyWord = subDomain
                };
                DescribeDomainRecordsResponse response = await client.DescribeDomainRecordsAsync(request);
                foreach (DescribeDomainRecordsResponseBody.DescribeDomainRecordsResponseBodyDomainRecords.
                             DescribeDomainRecordsResponseBodyDomainRecordsRecord record in response.Body.DomainRecords
                             .Record)
                {
                    UpdateDomainRecordRequest updateRequest = new()
                    {
                        Line = record.Line,
                        Priority = record.Priority,
                        RR = record.RR,
                        RecordId = record.RecordId,
                        TTL = record.TTL,
                        Type = record.Type
                    };
                    switch (record.Type)
                    {
                        case "A":
                            if (!string.IsNullOrWhiteSpace(ipv4) || (record.Value == ipv4))
                            {
                                continue;
                            }

                            updateRequest.Value = ipv4;
                            break;
                        case "AAAA":
                            if (!string.IsNullOrWhiteSpace(ipv6) || (record.Value == ipv6))
                            {
                                continue;
                            }

                            updateRequest.Value = ipv6;
                            break;
                        default:
                            continue;
                    }

                    try
                    {
                        await client.UpdateDomainRecordAsync(updateRequest);
                    }
                    catch (TeaException)
                    {
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Directory.CreateDirectory("logs");
        await File.AppendAllTextAsync($"logs/{DateTime.Now:yy-MM-ddTHH-mm-ss}.log", ex.ToString());
    }

    await Task.Delay(config.Interval);
}