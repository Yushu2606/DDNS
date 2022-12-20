using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SNDD.Utils;

public static partial class IPHelper
{
    public struct Data
    {
        public string ip { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
        public string via { get; set; }
        public string padding { get; set; }
    }
    public static async Task<string> GetIPv4()
    {
        using HttpClient client = new();
        string content = await client.GetStringAsync("https://ipv4.testipv6.cn/ip/");
        string json = IPRegex().Match(content).Groups[1].Value;
        Data result = JsonSerializer.Deserialize<Data>(json);
        return result.ip;
    }
    public static async Task<string> GetIPv6()
    {
        using HttpClient client = new();
        string content = await client.GetStringAsync("https://ipv6.testipv6.cn/ip/");
        string json = IPRegex().Match(content).Groups[1].Value;
        Data result = JsonSerializer.Deserialize<Data>(json);
        return result.ip;
    }

    [GeneratedRegex("callback\\((.+)\\)")]
    private static partial Regex IPRegex();
}
