using GoldMonitor.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMonitor.Services;

public class SinaGoldService : IGoldService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://hq.sinajs.cn/list=hf_XAU,gds_AU9999,gds_AUTD";

    public SinaGoldService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        string requestUrl = ApiUrl;

        using var responseMessage = await _httpClient.GetAsync(requestUrl, ct);
        responseMessage.EnsureSuccessStatusCode();
        byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
        string response = Encoding.GetEncoding("GBK").GetString(bytes);

        // 文本解析交给纯函数解析器，时间戳由服务负责
        var info = SinaResponseParser.Parse(response);
        info.UpdateTime = DateTime.Now;
        return info;
    }
}