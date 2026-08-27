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

    private bool _autoHideOnFullScreen = false;
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
    private double _uiScale = 1.0;
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

    private double _idleOpacity = 0.15;
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
    private bool _showDom = true;
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

    // 5. 涨跌配色
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

        UpColor = other.UpColor;
        DownColor = other.DownColor;
        FlatColor = other.FlatColor;
    }
}