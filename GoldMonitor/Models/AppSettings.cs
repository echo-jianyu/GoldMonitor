using CommunityToolkit.Mvvm.ComponentModel;

namespace GoldMonitor.Models;

public partial class AppSettings : ObservableObject
{
    // 1. 行为与常规
    [ObservableProperty]
    private bool _autoStart = false;

    [ObservableProperty]
    private bool _autoHideOnFullScreen = true;

    [ObservableProperty]
    private int _refreshIntervalSeconds = 5;

    [ObservableProperty]
    private double? _windowLeft;

    [ObservableProperty]
    private double? _windowTop;

    // 2. 外观与透明度
    [ObservableProperty]
    private double _uiScale = 1.25;

    [ObservableProperty]
    private string _capsuleBackground = "#D918181A";

    [ObservableProperty]
    private string _capsuleBorderColor = "#25FFFFFF";

    [ObservableProperty]
    private double _idleOpacity = 0.20;

    [ObservableProperty]
    private double _hoverOpacity = 0.90;

    [ObservableProperty]
    private string _fontFamily = "Microsoft YaHei UI";

    [ObservableProperty]
    private bool _showDividers = true;

    // 3. 国际金价 (XAU)
    [ObservableProperty]
    private bool _showXau = true;

    [ObservableProperty]
    private bool _showXauLabel = true;

    [ObservableProperty]
    private string _xauLabelText = "XAU";

    [ObservableProperty]
    private string _xauLabelColor = "#8E8E93";

    [ObservableProperty]
    private bool _showXauPrice = true;

    [ObservableProperty]
    private int _xauPriceDecimals = 2;

    [ObservableProperty]
    private string _xauPriceColor = "#F2F2F7";

    [ObservableProperty]
    private bool _showXauChangeRate = true;

    [ObservableProperty]
    private bool _showXauSign = true;

    [ObservableProperty]
    private bool _showXauPercent = true;

    // 4. 国内金价 (AU)
    [ObservableProperty]
    private bool _showDom = false;

    [ObservableProperty]
    private bool _showDomLabel = true;

    [ObservableProperty]
    private string _domLabelText = "AU";

    [ObservableProperty]
    private string _domLabelColor = "#8E8E93";

    [ObservableProperty]
    private bool _showDomPrice = true;

    [ObservableProperty]
    private int _domPriceDecimals = 2;

    [ObservableProperty]
    private string _domPriceColor = "#F2F2F7";

    [ObservableProperty]
    private bool _showDomChangeRate = true;

    [ObservableProperty]
    private bool _showDomSign = true;

    [ObservableProperty]
    private bool _showDomPercent = true;

    // 5. 黄金延期 Au(T+D)
    [ObservableProperty]
    private bool _showAutd = false;

    [ObservableProperty]
    private bool _showAutdLabel = true;

    [ObservableProperty]
    private string _autdLabelText = "AuTD";

    [ObservableProperty]
    private string _autdLabelColor = "#8E8E93";

    [ObservableProperty]
    private bool _showAutdPrice = true;

    [ObservableProperty]
    private int _autdPriceDecimals = 2;

    [ObservableProperty]
    private string _autdPriceColor = "#F2F2F7";

    [ObservableProperty]
    private bool _showAutdChangeRate = true;

    [ObservableProperty]
    private bool _showAutdSign = true;

    [ObservableProperty]
    private bool _showAutdPercent = true;

    // 6. 京东积存金 - 民生金价
    [ObservableProperty]
    private bool _showMs = false;

    [ObservableProperty]
    private bool _showMsLabel = true;

    [ObservableProperty]
    private string _msLabelText = "民生";

    [ObservableProperty]
    private string _msLabelColor = "#8E8E93";

    [ObservableProperty]
    private bool _showMsPrice = true;

    [ObservableProperty]
    private int _msPriceDecimals = 2;

    [ObservableProperty]
    private string _msPriceColor = "#F2F2F7";

    [ObservableProperty]
    private bool _showMsChangeRate = true;

    [ObservableProperty]
    private bool _showMsSign = true;

    [ObservableProperty]
    private bool _showMsPercent = true;

    // 7. 京东积存金 - 浙商金价
    [ObservableProperty]
    private bool _showZs = true;

    [ObservableProperty]
    private bool _showZsLabel = true;

    [ObservableProperty]
    private string _zsLabelText = "浙商";

    [ObservableProperty]
    private string _zsLabelColor = "#8E8E93";

    [ObservableProperty]
    private bool _showZsPrice = true;

    [ObservableProperty]
    private int _zsPriceDecimals = 2;

    [ObservableProperty]
    private string _zsPriceColor = "#F2F2F7";

    [ObservableProperty]
    private bool _showZsChangeRate = true;

    [ObservableProperty]
    private bool _showZsSign = true;

    [ObservableProperty]
    private bool _showZsPercent = true;

    // 8. 涨跌配色
    [ObservableProperty]
    private string _upColor = "#C07D00";

    [ObservableProperty]
    private string _downColor = "#4EAF50";

    [ObservableProperty]
    private string _flatColor = "#8E8E93";

    /// <summary>
    /// 浅拷贝配置副本
    /// </summary>
    public AppSettings Clone()
    {
        return (AppSettings)this.MemberwiseClone();
    }

    /// <summary>
    /// 从另一个配置实例复制所有属性
    /// </summary>
    public void CopyFrom(AppSettings other)
    {
        if (other == null) return;

        AutoStart = other.AutoStart;
        AutoHideOnFullScreen = other.AutoHideOnFullScreen;
        RefreshIntervalSeconds = other.RefreshIntervalSeconds;
        WindowLeft = other.WindowLeft;
        WindowTop = other.WindowTop;

        UiScale = other.UiScale;
        CapsuleBackground = other.CapsuleBackground;
        CapsuleBorderColor = other.CapsuleBorderColor;
        IdleOpacity = other.IdleOpacity;
        HoverOpacity = other.HoverOpacity;
        FontFamily = other.FontFamily;
        ShowDividers = other.ShowDividers;

        ShowXau = other.ShowXau;
        ShowXauLabel = other.ShowXauLabel;
        XauLabelText = other.XauLabelText;
        XauLabelColor = other.XauLabelColor;
        ShowXauPrice = other.ShowXauPrice;
        XauPriceDecimals = other.XauPriceDecimals;
        XauPriceColor = other.XauPriceColor;
        ShowXauChangeRate = other.ShowXauChangeRate;
        ShowXauSign = other.ShowXauSign;
        ShowXauPercent = other.ShowXauPercent;

        ShowDom = other.ShowDom;
        ShowDomLabel = other.ShowDomLabel;
        DomLabelText = other.DomLabelText;
        DomLabelColor = other.DomLabelColor;
        ShowDomPrice = other.ShowDomPrice;
        DomPriceDecimals = other.DomPriceDecimals;
        DomPriceColor = other.DomPriceColor;
        ShowDomChangeRate = other.ShowDomChangeRate;
        ShowDomSign = other.ShowDomSign;
        ShowDomPercent = other.ShowDomPercent;

        ShowAutd = other.ShowAutd;
        ShowAutdLabel = other.ShowAutdLabel;
        AutdLabelText = other.AutdLabelText;
        AutdLabelColor = other.AutdLabelColor;
        ShowAutdPrice = other.ShowAutdPrice;
        AutdPriceDecimals = other.AutdPriceDecimals;
        AutdPriceColor = other.AutdPriceColor;
        ShowAutdChangeRate = other.ShowAutdChangeRate;
        ShowAutdSign = other.ShowAutdSign;
        ShowAutdPercent = other.ShowAutdPercent;

        ShowMs = other.ShowMs;
        ShowMsLabel = other.ShowMsLabel;
        MsLabelText = other.MsLabelText;
        MsLabelColor = other.MsLabelColor;
        ShowMsPrice = other.ShowMsPrice;
        MsPriceDecimals = other.MsPriceDecimals;
        MsPriceColor = other.MsPriceColor;
        ShowMsChangeRate = other.ShowMsChangeRate;
        ShowMsSign = other.ShowMsSign;
        ShowMsPercent = other.ShowMsPercent;

        ShowZs = other.ShowZs;
        ShowZsLabel = other.ShowZsLabel;
        ZsLabelText = other.ZsLabelText;
        ZsLabelColor = other.ZsLabelColor;
        ShowZsPrice = other.ShowZsPrice;
        ZsPriceDecimals = other.ZsPriceDecimals;
        ZsPriceColor = other.ZsPriceColor;
        ShowZsChangeRate = other.ShowZsChangeRate;
        ShowZsSign = other.ShowZsSign;
        ShowZsPercent = other.ShowZsPercent;

        UpColor = other.UpColor;
        DownColor = other.DownColor;
        FlatColor = other.FlatColor;
    }
}