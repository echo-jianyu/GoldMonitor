using System;
using System.Globalization;
using GoldMonitor.Models;

namespace GoldMonitor.Services;

/// <summary>
/// 新浪行情纯文本解析器：输入为 GBK 解码后的完整响应文本，输出 SinaQuote。
/// 纯函数：不做网络 IO、不设置时间戳（由服务负责），便于单元测试直接喂字符串。
/// </summary>
public static class SinaResponseParser
{
    public static SinaQuote Parse(string decodedText)
    {
        var quote = new SinaQuote();
        if (string.IsNullOrEmpty(decodedText))
        {
            return quote;
        }

        // 按分号和换行符切分成单独的语句
        var statements = decodedText.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

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
                    quote.Xau.Price = p;
                    // parts[7] 是昨收价（仅用于计算涨跌幅，不落存储）
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        quote.Xau.ChangeRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
            // 2. 解析循环中匹配 AU9999
            else if (statement.Contains("gds_AU9999", StringComparison.OrdinalIgnoreCase))
            {
                // parts[0] 为 Au99.99 最新价
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p))
                {
                    quote.Dom.Price = p;
                    // parts[7] 是昨收价（仅用于计算涨跌幅，不落存储）
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        quote.Dom.ChangeRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
            // 3. 解析黄金延期 Au(T+D)
            // gds_AUTD 字段结构与 gds_AU9999 相同：0 最新价, 7 昨结算
            else if (statement.Contains("gds_AUTD", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p))
                {
                    quote.Autd.Price = p;
                    // parts[7] 是昨结算价（仅用于计算涨跌幅，不落存储）
                    if (double.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out double lastClose) && lastClose > 0)
                    {
                        quote.Autd.ChangeRate = (p - lastClose) / lastClose * 100;
                    }
                }
            }
        }

        return quote;
    }
}
