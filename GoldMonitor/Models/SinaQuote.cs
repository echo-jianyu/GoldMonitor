namespace GoldMonitor.Models;

/// <summary>
/// 新浪行情快照：一次请求产出的三个模块（XAU / AU9999 / AuTD）。
/// 只包含新浪数据源实际提供的数据——接口契约诚实，不做无意义的 0 填充承诺。
/// </summary>
public class SinaQuote
{
    /// <summary>国际金价 (XAU)</summary>
    public GoldQuote Xau { get; } = new();

    /// <summary>国内金价 (AU9999)</summary>
    public GoldQuote Dom { get; } = new();

    /// <summary>黄金延期 Au(T+D)</summary>
    public GoldQuote Autd { get; } = new();
}
