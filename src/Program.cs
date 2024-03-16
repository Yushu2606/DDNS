using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using AliyunDynamicDomainNameServer.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using Tea;

DateTime startTime = DateTime.Now;
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
using HttpClient httpClient = new();
string wanInfo = await httpClient.GetStringAsync("http://192.168.1.1/fh_get_wan_info.ajax");
Data? data = JsonSerializer.Deserialize<Data>(wanInfo, new JsonSerializerOptions
{
    AllowTrailingCommas = true
});
string? ipv4 =
    (from wanConnect in data!.WanConnects
        where wanConnect.IfName.StartWith("ppp")
        where wanConnect.IPv4Enabled is "1"
        select wanConnect.ExternalIPAddress).FirstOrDefault();
string? ipv6 =
    (from address in Dns.GetHostAddresses(Dns.GetHostName())
        where address.AddressFamily is AddressFamily.InterNetworkV6
        select address.ToString()).LastOrDefault();
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
                RecordId = record.RecordId,
                RR = record.RR,
                Type = record.Type
            };
            switch (record.Type)
            {
                case "A":
                    if (string.IsNullOrWhiteSpace(ipv4) || (record.Value == ipv4))
                    {
                        continue;
                    }

                    updateRequest.Value = ipv4;
                    break;
                case "AAAA":
                    if (string.IsNullOrWhiteSpace(ipv6) || (record.Value == ipv6))
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
            catch (TeaException ex)
            {
                Directory.CreateDirectory("logs");
                string logFilePath = $"logs/{startTime:yy-MM-ddTHH-mm-ss}.log";
                if (File.Exists(logFilePath))
                {
                    await File.AppendAllTextAsync(logFilePath, "\n");
                }

                await File.AppendAllTextAsync(logFilePath, ex.ToString());
            }
        }
    }
}