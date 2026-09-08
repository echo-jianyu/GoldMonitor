using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;

namespace GoldMonitor.Services;

/// <summary>
/// 京东金融积存金行情服务（goldType=1 民生金价 / goldType=2 浙商金价）
/// </summary>
public class JdGoldService : IGoldService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://ms.jr.jd.com/gw2/generic/CreatorSer/pc/m/pcQueryGoldProduct";

    public JdGoldService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        // 民生与浙商两个产品接口并发请求
        var msTask = FetchProductAsync(1, ct);
        var zsTask = FetchProductAsync(2, ct);

        double msPrice = 0, msRate = 0, zsPrice = 0, zsRate = 0;

        // 各产品独立容错：单一产品异常不影响另一个产品展示
        try { (msPrice, msRate) = await msTask; } catch { }
        try { (zsPrice, zsRate) = await zsTask; } catch { }

        if (msPrice <= 0 && zsPrice <= 0)
        {
            throw new HttpRequestException("京东积存金行情数据不可用");
        }

        return new GoldPriceInfo
        {
            MsGoldPrice = msPrice,
            MsChangeRate = msRate,
            ZsGoldPrice = zsPrice,
            ZsChangeRate = zsRate,
            UpdateTime = DateTime.Now
        };
    }

    /// <summary>
    /// 拉取单个积存金产品行情，返回 (现价 元/克, 涨跌幅 %)
    /// </summary>
    private async Task<(double price, double rate)> FetchProductAsync(int goldType, CancellationToken ct)
    {
        string requestUrl = $"{ApiUrl}?goldType={goldType}";

        using var responseMessage = await _httpClient.GetAsync(requestUrl, ct);
        responseMessage.EnsureSuccessStatusCode();
        string response = await responseMessage.Content.ReadAsStringAsync();

        // JSON 解析交给纯函数解析器
        return JdResponseParser.Parse(response);
    }
}
