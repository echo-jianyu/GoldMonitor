using System.Globalization;
using System.Text.Json;

namespace GoldMonitor.Services;

/// <summary>
/// 京东积存金响应解析器：输入为单个产品的响应 JSON 文本，
/// 返回 (现价 元/克, 涨跌幅 %)。
/// </summary>
public static class JdResponseParser
{
    public static (double price, double rate) Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);

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
