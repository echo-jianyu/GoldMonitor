using System;
using System.Collections.Generic;
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

        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = ver != null ? $"v{ver.Major}.{ver.Minor}.{ver.Build}" : "v1.0.0";

        // 如果主程序已有实时金价，用实时金价预览；否则提供拟真预览数据
        _previewPrice = (currentPrice != null && currentPrice.XauUsd > 0)
            ? currentPrice
            : new GoldPriceInfo
            {
                XauUsd = 2938.69,
                XauChangeRate = 0.68,
                DomesticAu = 688.55,
                DomesticChangeRate = -0.22,
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
            var def = new AppSettings();
            Settings.CopyFrom(def);
        }
    }

    // 快捷色彩预设方案
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

    // 设置缩放比例
    [RelayCommand]
    public void SetScale(string scaleStr)
    {
        if (double.TryParse(scaleStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            Settings.UiScale = Math.Max(0.5, Math.Min(scale, 3.0));
        }
    }

    // 保存配置
    [RelayCommand]
    public void Save()
    {
        // 1. 模块启用完整性校验
        bool hasXauContent = Settings.ShowXau && (Settings.ShowXauLabel || Settings.ShowXauPrice || Settings.ShowXauChangeRate);
        bool hasDomContent = Settings.ShowDom && (Settings.ShowDomLabel || Settings.ShowDomPrice || Settings.ShowDomChangeRate);

        if (!hasXauContent && !hasDomContent)
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
        Settings.XauPriceDecimals = Math.Max(0, Math.Min(Settings.XauPriceDecimals, 2));
        Settings.DomPriceDecimals = Math.Max(0, Math.Min(Settings.DomPriceDecimals, 2));
        Settings.IdleOpacity = Math.Max(0.05, Math.Min(Settings.IdleOpacity, 1.0));
        Settings.HoverOpacity = Math.Max(0.05, Math.Min(Settings.HoverOpacity, 1.0));

        RequestCloseSuccess?.Invoke();
    }

    [RelayCommand]
    public void Cancel()
    {
        RequestCloseCancel?.Invoke();
    }
}