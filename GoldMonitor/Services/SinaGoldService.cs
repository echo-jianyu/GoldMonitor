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
    private const string ApiUrl = "https://hq.sinajs.cn/list=hf_XAU,gds_AU9999,gds_AUTD,fx_susdcny";

    // 1 金衡盎司 = 31.1034768 克，用于将「美元/盎司」换算为「元/克」
    private const double GramsPerTroyOunce = 31.1034768;

    public SinaGoldService(HttpClient httpClient)
    {
            _httpClient = httpClient;
        }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        string requestUrl = $"{ApiUrl}";

        using var responseMessage = await _httpClient.GetAsync(requestUrl, ct);
        responseMessage.EnsureSuccessStatusCode();
        byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
        string response = Encoding.GetEncoding("GBK").GetString(bytes);

        double xau = 0, xauLast = 0, xauRate = 0;
        double dom = 0, domLast = 0, domRate = 0;
        double autd = 0, autdLast = 0, autdRate = 0;
        double fx = 0, fxLast = 0;

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
            // 3. 解析黄金延期 Au(T+D)
            // gds_AUTD 字段结构与 gds_AU9999 相同：0 最新价, 7 昨结算
            else if (statement.Contains("gds_AUTD", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p))
                {
                    autd = p;
                    // parts[7] 是昨结算价
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        autdLast = lastClose;
                        autdRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
            // 4. 解析美元兑人民币汇率
            else if (statement.Contains("fx_susdcny", StringComparison.OrdinalIgnoreCase))
            {
                // fx_susdcny 字段：
                // 0 时间, 1 买入价, 2 卖出价, 3 昨收, 4 成交量, 5 最高, 6 最低, 7 开盘, 8 最新价, ...
                // 优先取 parts[8] (最新价)，缺失时回退到 parts[1] (买入价) / parts[2] (卖出价)
                foreach (int idx in new[] { 8, 1, 2 })
                {
                    if (idx < parts.Length &&
                        double.TryParse(parts[idx], NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v > 0)
                    {
                        fx = v;
                        break;
                    }
                }

                // 昨收价：用于换算金价涨跌幅
                if (parts.Length > 3 &&
                    double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                {
                    fxLast = lastClose;
                }
            }
        }

        // 5. 由国际金价 XAU 按汇率换算国内金价 (元/克)
        double cnyGold = 0, cnyGoldLast = 0, cnyGoldRate = 0;
        if (xau > 0 && fx > 0)
        {
            cnyGold = xau * fx / GramsPerTroyOunce;

            // 涨跌幅基准：昨收 XAU × 昨收汇率（汇率昨收缺失时回退为当前汇率）
            double fxRef = fxLast > 0 ? fxLast : fx;
            if (xauLast > 0)
            {
                cnyGoldLast = xauLast * fxRef / GramsPerTroyOunce;
            }
            if (cnyGoldLast > 0)
            {
                cnyGoldRate = (cnyGold - cnyGoldLast) / cnyGoldLast * 100;
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
            AutdGoldPrice = autd,
            AutdLastClose = autdLast,
            AutdChangeRate = autdRate,
            UsdCnyRate = fx,
            CnyGoldPrice = cnyGold,
            CnyGoldLastClose = cnyGoldLast,
            CnyGoldChangeRate = cnyGoldRate,
            UpdateTime = DateTime.Now
        };
    }
}