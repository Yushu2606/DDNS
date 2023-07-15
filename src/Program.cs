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
    bool TryGet<T>(Func<T> action, out T @return)
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
    bool hasIPv4 = TryGet(() => JsonSerializer.Deserialize<Data>(DataRegex().Match(httpClient.GetStringAsync("https://ipv4.test-ipv6.com/ip/").Result).Groups[1].Value).IP, out string ipv4);
    bool hasIPv6 = TryGet(() => JsonSerializer.Deserialize<Data>(DataRegex().Match(httpClient.GetStringAsync("https://ipv6.test-ipv6.com/ip/").Result).Groups[1].Value).IP, out string ipv6);
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
            try
            {
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
                            if (!hasIPv4 || record.Value == ipv4)
                            {
                                continue;
                            }
                            updateRequest.Value = ipv4;
                            break;
                        case "AAAA":
                            if (!hasIPv6 || record.Value == ipv6)
                            {
                                continue;
                            }
                            updateRequest.Value = ipv6;
                            break;
                        default:
                            continue;
                    }
                    _ = await client.UpdateDomainRecordAsync(updateRequest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                continue;
            }
        }
    }
}

internal static partial class Program
{
    [GeneratedRegex("callback\\((.+)\\)")]
    internal static partial Regex DataRegex();
}
