using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
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

    public JdGoldService(HttpClient? httpClient = null)
    {
        if (httpClient != null)
        {
            _httpClient = httpClient;
        }
        else
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            // 京东金融接口防盗链所需的请求头（与新浪不同，需独立的 HttpClient）
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://jdjr.jd.com/");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://jdjr.jd.com");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }
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

        using var doc = JsonDocument.Parse(response);

        // 响应结构: resultData.data 下的 priceValue(现价) / raisePercent100(涨跌幅，带 % 后缀)
        if (!doc.RootElement.TryGetProperty("resultData", out var resultData) ||
            !resultData.TryGetProperty("data", out var data))
        {
            return (0, 0);
        }

        double price = GetDouble(data, "priceValue");
        double rate = GetDouble(data, "raisePercent100", stripPercent: true);

        return (price, rate);
    }

    private static double GetDouble(JsonElement element, string name, bool stripPercent = false)
    {
        if (!element.TryGetProperty(name, out var prop) || prop.ValueKind != JsonValueKind.String)
            return 0;

        string text = prop.GetString() ?? string.Empty;
        if (text.Length == 0)
            return 0;

        if (stripPercent)
            text = text.TrimEnd('%');

        // 涨跌幅可能为负值，此处不做正负校验，有效性由调用方判断
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0;
    }
}
