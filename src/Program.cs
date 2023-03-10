using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using SNDD.Utils;
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
Doing(default, default);
System.Timers.Timer timer = new(config.Interval);
timer.Elapsed += Doing;
timer.Start();
while (true)
{
    _ = Console.Read();
}

async void Doing(object _sender, ElapsedEventArgs _e)
{
    HttpClient httpClient = new();
    bool TryGet<T>(Func<Task<T>> action, out Task<T> @return)
    {
        try
        {
            @return = action.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        @return = default;
        return false;
    }
    bool hasIPv4 = TryGet(async () => JsonSerializer.Deserialize<Data>(DataRegex().Match(await httpClient.GetStringAsync("https://ipv4.test-ipv6.com/ip/")).Groups[1].Value).IP, out Task<string> ipv4);
    bool hasIPv6 = TryGet(async () => JsonSerializer.Deserialize<Data>(DataRegex().Match(await httpClient.GetStringAsync("https://ipv6.test-ipv6.com/ip/")).Groups[1].Value).IP, out Task<string> ipv6);
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
                UpdateDomainRecordRequest updateRequest = new()
                {
                    RecordId = record.RecordId,
                    RR = record.RR,
                    Type = record.Type
                };
                switch (record.Type)
                {
                    case "A":
                        if (!hasIPv4 || record.Value == await ipv4)
                        {
                            continue;
                        }
                        updateRequest.Value = await ipv4;
                        break;
                    case "AAAA":
                        if (!hasIPv6 || record.Value == await ipv6)
                        {
                            continue;
                        }
                        updateRequest.Value = await ipv6;
                        break;
                    default:
                        continue;
                }
                _ = await client.UpdateDomainRecordAsync(updateRequest);
            }
        }
    }
}

internal static partial class Program
{
    [GeneratedRegex("callback\\((.+)\\)")]
    internal static partial Regex DataRegex();
}
