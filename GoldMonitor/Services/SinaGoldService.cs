using GoldMonitor.Models;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMonitor.Services;

public class SinaGoldService : IGoldService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://hq.sinajs.cn/list=hf_XAU,gds_AU9999";

    public SinaGoldService(HttpClient? httpClient = null)
    {
        if (httpClient != null)
        {
            _httpClient = httpClient;
        }
        else
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://finance.sina.com.cn");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }
    }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        // 1. 拼接毫秒时间戳，防止代理缓存
        //long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string requestUrl = $"{ApiUrl}&_={timestamp}";
        string requestUrl = $"{ApiUrl}";

        using var responseMessage = await _httpClient.GetAsync(requestUrl, ct);
        responseMessage.EnsureSuccessStatusCode();
        byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
        string response = Encoding.GetEncoding("GBK").GetString(bytes);

        double xau = 0, xauLast = 0, xauRate = 0;
        double dom = 0, domLast = 0, domRate = 0;

        // 按分号和换行符切分成单独的语句 
        var statements = response.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var statement in statements)
        {
            // var hq_str_hf_XAU = "4637.73,4658.650,4637.73,4638.08,4673.66,4636.11,08:47:00,4658.65,4657.23,0,0,0,2026-08-26,伦敦金（现货黄金）";
            if (string.IsNullOrWhiteSpace(statement) || !statement.Contains('"'))
                continue;

            // 提取引号内的逗号分隔数据 
            var content = statement.Split('"')[1];
            var parts = content.Split(',');

            if (parts.Length < 8)
                continue;

            // 1. 解析国际黄金 XAU
            if (statement.Contains("hf_XAU", StringComparison.OrdinalIgnoreCase))
            {
                // parts[0] 为 XAU 最新价
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p))
                {
                    xau = p;
                    // parts[7] 是昨收价
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        xauLast = lastClose;
                        xauRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
            // 2. 解析循环中匹配 AU9999
            else if (statement.Contains("gds_AU9999", StringComparison.OrdinalIgnoreCase))
            {
                // parts[0] 为 Au99.99 最新价
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p))
                {
                    dom = p;
                    // parts[7] 是昨收价
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        domLast = lastClose;
                        domRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
        }

        return new GoldPriceInfo
        {
            XauUsd = xau,
            XauLastClose = xauLast,
            XauChangeRate = xauRate,
            DomesticAu = dom,
            DomLastClose = domLast,
            DomesticChangeRate = domRate,
            UpdateTime = DateTime.Now
        };
    }
}