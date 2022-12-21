using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using SNDD.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Timers;

Config config = new();
if (!File.Exists("config.json"))
{
    File.WriteAllText("config.json", JsonSerializer.Serialize(config, new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    }));
}
config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));
Client client = new(new()
{
    AccessKeyId = config.AccessKeyId,
    AccessKeySecret = config.AccessKeySecret
});
Doing(null, null);
Timer timer = new(config.Interval);
timer.Elapsed += Doing;
timer.Start();
while (true)
{
    _ = Console.Read();
}

async void Doing(object _sender, ElapsedEventArgs _e)
{
    HttpClient httpClient = new();
    string ipv4 = JsonSerializer.Deserialize<Data>(Regex.Match(await httpClient.GetStringAsync("https://ipv4.testipv6.cn/ip/"), "callback\\((.+)\\)").Groups[1].Value).ip;
    string ipv6 = JsonSerializer.Deserialize<Data>(Regex.Match(await httpClient.GetStringAsync("https://ipv6.testipv6.cn/ip/"), "callback\\((.+)\\)").Groups[1].Value).ip;
    httpClient.Dispose();
    foreach (Config.Domain domain in config.Domains)
    {
        foreach (string subDomain in domain.SubDomains)
        {
            DescribeDomainRecordsRequest request = new()
            {
                DomainName = domain.Name,
                RRKeyWord = subDomain
            };
            DescribeDomainRecordsResponse response = await client.DescribeDomainRecordsAsync(request);
            foreach (DescribeDomainRecordsResponseBody.DescribeDomainRecordsResponseBodyDomainRecords.DescribeDomainRecordsResponseBodyDomainRecordsRecord record in response.Body.DomainRecords.Record)
            {
                if (record.Type is not "A" and not "AAAA" || record.Value == (record.Type is "AAAA" ? ipv6 : ipv4))
                {
                    continue;
                }
                UpdateDomainRecordRequest updateRequest = new()
                {
                    RecordId = record.RecordId,
                    RR = record.RR,
                    Type = record.Type,
                    Value = record.Type is "AAAA" ? ipv6 : ipv4
                };
                _ = await client.UpdateDomainRecordAsync(updateRequest);
            }
        }
    }
}
