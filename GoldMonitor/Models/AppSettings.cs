using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GoldMonitor.Models;

public class AppSettings : ObservableObject
{
    // 1. 行为与常规
    private bool _autoStart = false;
    public bool AutoStart
    {
        get => _autoStart;
        set => SetProperty(ref _autoStart, value);
    }

    private bool _autoHideOnFullScreen = true;
    public bool AutoHideOnFullScreen
    {
        get => _autoHideOnFullScreen;
        set => SetProperty(ref _autoHideOnFullScreen, value);
    }

    private int _refreshIntervalSeconds = 5;
    public int RefreshIntervalSeconds
    {
        get => _refreshIntervalSeconds;
        set => SetProperty(ref _refreshIntervalSeconds, value);
    }

    private double? _windowLeft;
    public double? WindowLeft
    {
        get => _windowLeft;
        set => SetProperty(ref _windowLeft, value);
    }

    private double? _windowTop;
    public double? WindowTop
    {
        get => _windowTop;
        set => SetProperty(ref _windowTop, value);
    }

    // 2. 外观与透明度
    private double _uiScale = 1.25;
    public double UiScale
    {
        get => _uiScale;
        set => SetProperty(ref _uiScale, value);
    }

    private string _capsuleBackground = "#D918181A";
    public string CapsuleBackground
    {
        get => _capsuleBackground;
        set => SetProperty(ref _capsuleBackground, value);
    }

    private string _capsuleBorderColor = "#25FFFFFF";
    public string CapsuleBorderColor
    {
        get => _capsuleBorderColor;
        set => SetProperty(ref _capsuleBorderColor, value);
    }

    private double _idleOpacity = 0.20;
    public double IdleOpacity
    {
        get => _idleOpacity;
        set => SetProperty(ref _idleOpacity, value);
    }

    private double _hoverOpacity = 0.90;
    public double HoverOpacity
    {
        get => _hoverOpacity;
        set => SetProperty(ref _hoverOpacity, value);
    }

    private string _fontFamily = "Microsoft YaHei UI";
    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    private bool _showDividers = true;
    public bool ShowDividers
    {
        get => _showDividers;
        set => SetProperty(ref _showDividers, value);
    }

    // 3. 国际金价 (XAU)
    private bool _showXau = true;
    public bool ShowXau
    {
        get => _showXau;
        set => SetProperty(ref _showXau, value);
    }

    private bool _showXauLabel = true;
    public bool ShowXauLabel
    {
        get => _showXauLabel;
        set => SetProperty(ref _showXauLabel, value);
    }

    private string _xauLabelText = "XAU";
    public string XauLabelText
    {
        get => _xauLabelText;
        set => SetProperty(ref _xauLabelText, value);
    }

    private string _xauLabelColor = "#8E8E93";
    public string XauLabelColor
    {
        get => _xauLabelColor;
        set => SetProperty(ref _xauLabelColor, value);
    }

    private bool _showXauPrice = true;
    public bool ShowXauPrice
    {
        get => _showXauPrice;
        set => SetProperty(ref _showXauPrice, value);
    }

    private int _xauPriceDecimals = 2;
    public int XauPriceDecimals
    {
        get => _xauPriceDecimals;
        set => SetProperty(ref _xauPriceDecimals, value);
    }

    private string _xauPriceColor = "#F2F2F7";
    public string XauPriceColor
    {
        get => _xauPriceColor;
        set => SetProperty(ref _xauPriceColor, value);
    }

    private bool _showXauChangeRate = true;
    public bool ShowXauChangeRate
    {
        get => _showXauChangeRate;
        set => SetProperty(ref _showXauChangeRate, value);
    }

    private bool _showXauSign = true;
    public bool ShowXauSign
    {
        get => _showXauSign;
        set => SetProperty(ref _showXauSign, value);
    }

    private bool _showXauPercent = true;
    public bool ShowXauPercent
    {
        get => _showXauPercent;
        set => SetProperty(ref _showXauPercent, value);
    }

    // 4. 国内金价 (AU)
    private bool _showDom = false;
    public bool ShowDom
    {
        get => _showDom;
        set => SetProperty(ref _showDom, value);
    }

    private bool _showDomLabel = true;
    public bool ShowDomLabel
    {
        get => _showDomLabel;
        set => SetProperty(ref _showDomLabel, value);
    }

    private string _domLabelText = "AU";
    public string DomLabelText
    {
        get => _domLabelText;
        set => SetProperty(ref _domLabelText, value);
    }

    private string _domLabelColor = "#8E8E93";
    public string DomLabelColor
    {
        get => _domLabelColor;
        set => SetProperty(ref _domLabelColor, value);
    }

    private bool _showDomPrice = true;
    public bool ShowDomPrice
    {
        get => _showDomPrice;
        set => SetProperty(ref _showDomPrice, value);
    }

    private int _domPriceDecimals = 2;
    public int DomPriceDecimals
    {
        get => _domPriceDecimals;
        set => SetProperty(ref _domPriceDecimals, value);
    }

    private string _domPriceColor = "#F2F2F7";
    public string DomPriceColor
    {
        get => _domPriceColor;
        set => SetProperty(ref _domPriceColor, value);
    }

    private bool _showDomChangeRate = true;
    public bool ShowDomChangeRate
    {
        get => _showDomChangeRate;
        set => SetProperty(ref _showDomChangeRate, value);
    }

    private bool _showDomSign = true;
    public bool ShowDomSign
    {
        get => _showDomSign;
        set => SetProperty(ref _showDomSign, value);
    }

    private bool _showDomPercent = true;
    public bool ShowDomPercent
    {
        get => _showDomPercent;
        set => SetProperty(ref _showDomPercent, value);
    }

    // 5. 黄金延期 Au(T+D)
    private bool _showAutd = false;
    public bool ShowAutd
    {
        get => _showAutd;
        set => SetProperty(ref _showAutd, value);
    }

    private bool _showAutdLabel = true;
    public bool ShowAutdLabel
    {
        get => _showAutdLabel;
        set => SetProperty(ref _showAutdLabel, value);
    }

    private string _autdLabelText = "AuTD";
    public string AutdLabelText
    {
        get => _autdLabelText;
        set => SetProperty(ref _autdLabelText, value);
    }

    private string _autdLabelColor = "#8E8E93";
    public string AutdLabelColor
    {
        get => _autdLabelColor;
        set => SetProperty(ref _autdLabelColor, value);
    }

    private bool _showAutdPrice = true;
    public bool ShowAutdPrice
    {
        get => _showAutdPrice;
        set => SetProperty(ref _showAutdPrice, value);
    }

    private int _autdPriceDecimals = 2;
    public int AutdPriceDecimals
    {
        get => _autdPriceDecimals;
        set => SetProperty(ref _autdPriceDecimals, value);
    }

    private string _autdPriceColor = "#F2F2F7";
    public string AutdPriceColor
    {
        get => _autdPriceColor;
        set => SetProperty(ref _autdPriceColor, value);
    }

    private bool _showAutdChangeRate = true;
    public bool ShowAutdChangeRate
    {
        get => _showAutdChangeRate;
        set => SetProperty(ref _showAutdChangeRate, value);
    }

    private bool _showAutdSign = true;
    public bool ShowAutdSign
    {
        get => _showAutdSign;
        set => SetProperty(ref _showAutdSign, value);
    }

    private bool _showAutdPercent = true;
    public bool ShowAutdPercent
    {
        get => _showAutdPercent;
        set => SetProperty(ref _showAutdPercent, value);
    }

    // 6. 换算金价 (XAU→CNY)
    private bool _showCnv = false;
    public bool ShowCnv
    {
        get => _showCnv;
        set => SetProperty(ref _showCnv, value);
    }

    private bool _showCnvLabel = true;
    public bool ShowCnvLabel
    {
        get => _showCnvLabel;
        set => SetProperty(ref _showCnvLabel, value);
    }

    private string _cnvLabelText = "换算";
    public string CnvLabelText
    {
        get => _cnvLabelText;
        set => SetProperty(ref _cnvLabelText, value);
    }

    private string _cnvLabelColor = "#8E8E93";
    public string CnvLabelColor
    {
        get => _cnvLabelColor;
        set => SetProperty(ref _cnvLabelColor, value);
    }

    private bool _showCnvPrice = true;
    public bool ShowCnvPrice
    {
        get => _showCnvPrice;
        set => SetProperty(ref _showCnvPrice, value);
    }

    private int _cnvPriceDecimals = 2;
    public int CnvPriceDecimals
    {
        get => _cnvPriceDecimals;
        set => SetProperty(ref _cnvPriceDecimals, value);
    }

    private string _cnvPriceColor = "#F2F2F7";
    public string CnvPriceColor
    {
        get => _cnvPriceColor;
        set => SetProperty(ref _cnvPriceColor, value);
    }

    private bool _showCnvChangeRate = true;
    public bool ShowCnvChangeRate
    {
        get => _showCnvChangeRate;
        set => SetProperty(ref _showCnvChangeRate, value);
    }

    private bool _showCnvSign = true;
    public bool ShowCnvSign
    {
        get => _showCnvSign;
        set => SetProperty(ref _showCnvSign, value);
    }

    private bool _showCnvPercent = true;
    public bool ShowCnvPercent
    {
        get => _showCnvPercent;
        set => SetProperty(ref _showCnvPercent, value);
    }

    // 7. 京东积存金 - 民生金价
    private bool _showMs = false;
    public bool ShowMs
    {
        get => _showMs;
        set => SetProperty(ref _showMs, value);
    }

    private bool _showMsLabel = true;
    public bool ShowMsLabel
    {
        get => _showMsLabel;
        set => SetProperty(ref _showMsLabel, value);
    }

    private string _msLabelText = "民生";
    public string MsLabelText
    {
        get => _msLabelText;
        set => SetProperty(ref _msLabelText, value);
    }

    private string _msLabelColor = "#8E8E93";
    public string MsLabelColor
    {
        get => _msLabelColor;
        set => SetProperty(ref _msLabelColor, value);
    }

    private bool _showMsPrice = true;
    public bool ShowMsPrice
    {
        get => _showMsPrice;
        set => SetProperty(ref _showMsPrice, value);
    }

    private int _msPriceDecimals = 2;
    public int MsPriceDecimals
    {
        get => _msPriceDecimals;
        set => SetProperty(ref _msPriceDecimals, value);
    }

    private string _msPriceColor = "#F2F2F7";
    public string MsPriceColor
    {
        get => _msPriceColor;
        set => SetProperty(ref _msPriceColor, value);
    }

    private bool _showMsChangeRate = true;
    public bool ShowMsChangeRate
    {
        get => _showMsChangeRate;
        set => SetProperty(ref _showMsChangeRate, value);
    }

    private bool _showMsSign = true;
    public bool ShowMsSign
    {
        get => _showMsSign;
        set => SetProperty(ref _showMsSign, value);
    }

    private bool _showMsPercent = true;
    public bool ShowMsPercent
    {
        get => _showMsPercent;
        set => SetProperty(ref _showMsPercent, value);
    }

    // 8. 京东积存金 - 浙商金价
    private bool _showZs = true;
    public bool ShowZs
    {
        get => _showZs;
        set => SetProperty(ref _showZs, value);
    }

    private bool _showZsLabel = true;
    public bool ShowZsLabel
    {
        get => _showZsLabel;
        set => SetProperty(ref _showZsLabel, value);
    }

    private string _zsLabelText = "浙商";
    public string ZsLabelText
    {
        get => _zsLabelText;
        set => SetProperty(ref _zsLabelText, value);
    }

    private string _zsLabelColor = "#8E8E93";
    public string ZsLabelColor
    {
        get => _zsLabelColor;
        set => SetProperty(ref _zsLabelColor, value);
    }

    private bool _showZsPrice = true;
    public bool ShowZsPrice
    {
        get => _showZsPrice;
        set => SetProperty(ref _showZsPrice, value);
    }

    private int _zsPriceDecimals = 2;
    public int ZsPriceDecimals
    {
        get => _zsPriceDecimals;
        set => SetProperty(ref _zsPriceDecimals, value);
    }

    private string _zsPriceColor = "#F2F2F7";
    public string ZsPriceColor
    {
        get => _zsPriceColor;
        set => SetProperty(ref _zsPriceColor, value);
    }

    private bool _showZsChangeRate = true;
    public bool ShowZsChangeRate
    {
        get => _showZsChangeRate;
        set => SetProperty(ref _showZsChangeRate, value);
    }

    private bool _showZsSign = true;
    public bool ShowZsSign
    {
        get => _showZsSign;
        set => SetProperty(ref _showZsSign, value);
    }

    private bool _showZsPercent = true;
    public bool ShowZsPercent
    {
        get => _showZsPercent;
        set => SetProperty(ref _showZsPercent, value);
    }

    // 9. 涨跌配色
    private string _upColor = "#C07D00";
    public string UpColor
    {
        get => _upColor;
        set => SetProperty(ref _upColor, value);
    }

    private string _downColor = "#4EAF50";
    public string DownColor
    {
        get => _downColor;
        set => SetProperty(ref _downColor, value);
    }

    private string _flatColor = "#8E8E93";
    public string FlatColor
    {
        get => _flatColor;
        set => SetProperty(ref _flatColor, value);
    }

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

        ShowCnv = other.ShowCnv;
        ShowCnvLabel = other.ShowCnvLabel;
        CnvLabelText = other.CnvLabelText;
        CnvLabelColor = other.CnvLabelColor;
        ShowCnvPrice = other.ShowCnvPrice;
        CnvPriceDecimals = other.CnvPriceDecimals;
        CnvPriceColor = other.CnvPriceColor;
        ShowCnvChangeRate = other.ShowCnvChangeRate;
        ShowCnvSign = other.ShowCnvSign;
        ShowCnvPercent = other.ShowCnvPercent;

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