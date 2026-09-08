using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldMonitor.Models;
using GoldMonitor.Services;

namespace GoldMonitor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public string AppVersion { get; }

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private GoldPriceInfo _previewPrice;

    public List<string> SystemFonts { get; }

    public event Action? RequestCloseSuccess;
    public event Action? RequestCloseCancel;

    public SettingsViewModel(AppSettings workingSettings, ConfigService configService, GoldPriceInfo? currentPrice)
    {
        _settings = workingSettings;

        // 统一显示规则依赖 Settings 的多个模块属性，任一属性变化时联动刷新界面绑定
        _settings.PropertyChanged += OnSettingsPropertyChanged;

        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = ver != null ? $"v{ver.Major}.{ver.Minor}.{ver.Build}" : "v1.0.0";

        // 如果主程序已有实时金价，用实时金价预览；否则提供拟真预览数据
        _previewPrice = (currentPrice != null && (currentPrice.XauUsd > 0 || currentPrice.MsGoldPrice > 0 || currentPrice.ZsGoldPrice > 0))
            ? currentPrice
            : new GoldPriceInfo
            {
                XauUsd = 2938.69,
                XauChangeRate = 0.68,
                DomesticAu = 688.55,
                DomesticChangeRate = -0.22,
                AutdGoldPrice = 688.30,
                AutdChangeRate = -0.18,
                MsGoldPrice = 990.46,
                MsChangeRate = -0.48,
                ZsGoldPrice = 990.79,
                ZsChangeRate = -0.47,
                UpdateTime = DateTime.Now
            };

        // 获取系统已安装字体（过滤空项并去重）
        SystemFonts = Fonts.SystemFontFamilies
            .Select(f => f.Source)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }

    /// <summary>
    /// 是否显示标签
    /// </summary>
    public bool UnifiedShowLabel
    {
        get => Settings.Xau.ShowLabel;
        set
        {
            Settings.Xau.ShowLabel = value;
            Settings.Dom.ShowLabel = value;
            Settings.Autd.ShowLabel = value;
            Settings.Ms.ShowLabel = value;
            Settings.Zs.ShowLabel = value;
        }
    }

    /// <summary>
    /// 是否显示价格
    /// </summary>
    public bool UnifiedShowPrice
    {
        get => Settings.Xau.ShowPrice;
        set
        {
            Settings.Xau.ShowPrice = value;
            Settings.Dom.ShowPrice = value;
            Settings.Autd.ShowPrice = value;
            Settings.Ms.ShowPrice = value;
            Settings.Zs.ShowPrice = value;
        }
    }

    /// <summary>
    /// 是否显示涨跌幅
    /// </summary>
    public bool UnifiedShowRate
    {
        get => Settings.Xau.ShowChangeRate;
        set
        {
            Settings.Xau.ShowChangeRate = value;
            Settings.Dom.ShowChangeRate = value;
            Settings.Autd.ShowChangeRate = value;
            Settings.Ms.ShowChangeRate = value;
            Settings.Zs.ShowChangeRate = value;
        }
    }

    /// <summary>
    /// 是否显示涨跌符号（+/-）
    /// </summary>
    public bool UnifiedSign
    {
        get => Settings.Xau.ShowSign;
        set
        {
            Settings.Xau.ShowSign = value;
            Settings.Dom.ShowSign = value;
            Settings.Autd.ShowSign = value;
            Settings.Ms.ShowSign = value;
            Settings.Zs.ShowSign = value;
        }
    }

    /// <summary>
    /// 是否显示涨跌百分比
    /// </summary>
    public bool UnifiedPercent
    {
        get => Settings.Xau.ShowPercent;
        set
        {
            Settings.Xau.ShowPercent = value;
            Settings.Dom.ShowPercent = value;
            Settings.Autd.ShowPercent = value;
            Settings.Ms.ShowPercent = value;
            Settings.Zs.ShowPercent = value;
        }
    }

    /// <summary>
    /// 小数位数（0~2）
    /// </summary>
    public int UnifiedDecimals
    {
        get => Settings.Xau.PriceDecimals;
        set
        {
            Settings.Xau.PriceDecimals = value;
            Settings.Dom.PriceDecimals = value;
            Settings.Autd.PriceDecimals = value;
            Settings.Ms.PriceDecimals = value;
            Settings.Zs.PriceDecimals = value;
        }
    }

    // Settings属性变化：
    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(UnifiedShowLabel));
        OnPropertyChanged(nameof(UnifiedShowPrice));
        OnPropertyChanged(nameof(UnifiedShowRate));
        OnPropertyChanged(nameof(UnifiedSign));
        OnPropertyChanged(nameof(UnifiedPercent));
        OnPropertyChanged(nameof(UnifiedDecimals));
    }

    /// <summary>
    /// Settings 整体被替换时（恢复默认）重新挂订阅并全量刷新 Unified* 联动状态。
    /// </summary>
    partial void OnSettingsChanged(AppSettings? oldValue, AppSettings? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= OnSettingsPropertyChanged;
        }
        if (newValue != null)
        {
            newValue.PropertyChanged += OnSettingsPropertyChanged;
        }

        OnPropertyChanged(nameof(UnifiedShowLabel));
        OnPropertyChanged(nameof(UnifiedShowPrice));
        OnPropertyChanged(nameof(UnifiedShowRate));
        OnPropertyChanged(nameof(UnifiedSign));
        OnPropertyChanged(nameof(UnifiedPercent));
        OnPropertyChanged(nameof(UnifiedDecimals));
    }

    /// <summary>
    /// 恢复默认设置
    /// </summary>
    [RelayCommand]
    public void ResetToDefaults()
    {
        var result = MessageBox.Show(
            "确定要将所有样式和配置恢复为程序初始默认值吗？",
            "恢复默认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // 整体替换为全新默认实例：触发 OnSettingsChanged 重挂事件并刷新 Unified* 联动状态
            Settings = new AppSettings();
        }
    }

    /// <summary>
    /// 应用预设配色方案
    /// </summary>
    /// <param name="preset"></param>
    [RelayCommand]
    public void ApplyColorPreset(string preset)
    {
        switch (preset)
        {
            case "GoldGreen": // 柔金 / 翡翠
                Settings.UpColor = "#C07D00";
                Settings.DownColor = "#4EAF50";
                break;
            case "RedGreen": // 国内常用 红涨 / 绿跌
                Settings.UpColor = "#E53935";
                Settings.DownColor = "#43A047";
                break;
            case "GreenRed": // 国际常用 绿涨 / 红跌
                Settings.UpColor = "#43A047";
                Settings.DownColor = "#E53935";
                break;
        }
    }

    /// <summary>
    /// 设置缩放比例
    /// </summary>
    /// <param name="scaleStr"></param>
    [RelayCommand]
    public void SetScale(string scaleStr)
    {
        if (double.TryParse(scaleStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            Settings.UiScale = Math.Max(0.5, Math.Min(scale, 3.0));
        }
    }

    /// <summary>
    /// 保存设置并关闭窗口
    /// </summary>
    [RelayCommand]
    public void Save()
    {
        // 1. 模块启用完整性校验：至少保留一项可见内容
        var modules = new[] { Settings.Xau, Settings.Dom, Settings.Autd, Settings.Ms, Settings.Zs };
        bool hasAnyContent = false;
        foreach (var m in modules)
        {
            if (m.Show && (m.ShowLabel || m.ShowPrice || m.ShowChangeRate))
            {
                hasAnyContent = true;
                break;
            }
        }

        if (!hasAnyContent)
        {
            MessageBox.Show(
                "请至少保留一项可见的内容（如标签、价格或涨跌幅）！\n不能将所有显示内容全部关闭。",
                "配置校验提示",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // 2. 参数边界自动修正（安全保护）
        Settings.RefreshIntervalSeconds = Math.Max(1, Math.Min(Settings.RefreshIntervalSeconds, 3600));
        Settings.UiScale = Math.Max(0.5, Math.Min(Settings.UiScale, 3.0));
        Settings.IdleOpacity = Math.Max(0.05, Math.Min(Settings.IdleOpacity, 1.0));
        Settings.HoverOpacity = Math.Max(0.05, Math.Min(Settings.HoverOpacity, 1.0));
        foreach (var m in modules)
        {
            m.PriceDecimals = Math.Max(0, Math.Min(m.PriceDecimals, 2));
        }

        RequestCloseSuccess?.Invoke();
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        RequestCloseCancel?.Invoke();
    }
}