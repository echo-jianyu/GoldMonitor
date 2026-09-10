namespace GoldMonitor.Models;

/// <summary>
/// 京东积存金快照：两个产品模块（民生 / 浙商）。
/// 只包含京东数据源实际提供的数据——接口契约诚实，不做无意义的 0 填充承诺。
/// </summary>
public class JdQuote
{
    /// <summary>京东积存金 - 民生金价</summary>
    public GoldQuote Ms { get; } = new();

    /// <summary>京东积存金 - 浙商金价</summary>
    public GoldQuote Zs { get; } = new();
}
