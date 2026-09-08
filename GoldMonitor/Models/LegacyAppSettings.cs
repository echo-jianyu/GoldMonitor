namespace GoldMonitor.Models;

/// <summary>
/// 旧版扁平配置结构（纯 DTO，不参与 UI 绑定）。
/// 仅用于 v1.x 扁平 config.json 的一次性迁移：键名与旧版 AppSettings 的
/// JSON 序列化键完全一致，属性改名会导致旧配置无法识别，不要修改。
/// </summary>
public class LegacyAppSettings
{
    // 1. 行为与常规
    public bool AutoStart { get; set; }
    public bool AutoHideOnFullScreen { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }

    // 2. 外观与透明度
    public double UiScale { get; set; }
    public string? CapsuleBackground { get; set; }
    public string? CapsuleBorderColor { get; set; }
    public double IdleOpacity { get; set; }
    public double HoverOpacity { get; set; }
    public string? FontFamily { get; set; }
    public bool ShowDividers { get; set; }

    // 3. 国际金价 (XAU)
    public bool ShowXau { get; set; }
    public bool ShowXauLabel { get; set; }
    public string? XauLabelText { get; set; }
    public string? XauLabelColor { get; set; }
    public bool ShowXauPrice { get; set; }
    public int XauPriceDecimals { get; set; }
    public string? XauPriceColor { get; set; }
    public bool ShowXauChangeRate { get; set; }
    public bool ShowXauSign { get; set; }
    public bool ShowXauPercent { get; set; }

    // 4. 国内金价 (AU)
    public bool ShowDom { get; set; }
    public bool ShowDomLabel { get; set; }
    public string? DomLabelText { get; set; }
    public string? DomLabelColor { get; set; }
    public bool ShowDomPrice { get; set; }
    public int DomPriceDecimals { get; set; }
    public string? DomPriceColor { get; set; }
    public bool ShowDomChangeRate { get; set; }
    public bool ShowDomSign { get; set; }
    public bool ShowDomPercent { get; set; }

    // 5. 黄金延期 Au(T+D)
    public bool ShowAutd { get; set; }
    public bool ShowAutdLabel { get; set; }
    public string? AutdLabelText { get; set; }
    public string? AutdLabelColor { get; set; }
    public bool ShowAutdPrice { get; set; }
    public int AutdPriceDecimals { get; set; }
    public string? AutdPriceColor { get; set; }
    public bool ShowAutdChangeRate { get; set; }
    public bool ShowAutdSign { get; set; }
    public bool ShowAutdPercent { get; set; }

    // 6. 京东积存金 - 民生金价
    public bool ShowMs { get; set; }
    public bool ShowMsLabel { get; set; }
    public string? MsLabelText { get; set; }
    public string? MsLabelColor { get; set; }
    public bool ShowMsPrice { get; set; }
    public int MsPriceDecimals { get; set; }
    public string? MsPriceColor { get; set; }
    public bool ShowMsChangeRate { get; set; }
    public bool ShowMsSign { get; set; }
    public bool ShowMsPercent { get; set; }

    // 7. 京东积存金 - 浙商金价
    public bool ShowZs { get; set; }
    public bool ShowZsLabel { get; set; }
    public string? ZsLabelText { get; set; }
    public string? ZsLabelColor { get; set; }
    public bool ShowZsPrice { get; set; }
    public int ZsPriceDecimals { get; set; }
    public string? ZsPriceColor { get; set; }
    public bool ShowZsChangeRate { get; set; }
    public bool ShowZsSign { get; set; }
    public bool ShowZsPercent { get; set; }

    // 8. 涨跌配色
    public string? UpColor { get; set; }
    public string? DownColor { get; set; }
    public string? FlatColor { get; set; }
}
