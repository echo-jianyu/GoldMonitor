using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GoldMonitor.Models;

namespace GoldMonitor.Controls;

public partial class CapsuleControl : UserControl
{
    public static readonly DependencyProperty PriceInfoProperty =
        DependencyProperty.Register(nameof(PriceInfo), typeof(GoldPriceInfo), typeof(CapsuleControl),
            new PropertyMetadata(null, OnDataOrSettingsChanged));

    public static readonly DependencyProperty SettingsProperty =
        DependencyProperty.Register(nameof(Settings), typeof(AppSettings), typeof(CapsuleControl),
            new PropertyMetadata(null, OnSettingsObjectChanged));

    public GoldPriceInfo? PriceInfo
    {
        get => (GoldPriceInfo?)GetValue(PriceInfoProperty);
        set => SetValue(PriceInfoProperty, value);
    }

    public AppSettings? Settings
    {
        get => (AppSettings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public CapsuleControl()
    {
        InitializeComponent();
    }

    private static void OnSettingsObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CapsuleControl control)
        {
            if (e.OldValue is INotifyPropertyChanged oldNpc)
            {
                oldNpc.PropertyChanged -= control.OnSettingsPropertyChanged;
            }
            if (e.NewValue is INotifyPropertyChanged newNpc)
            {
                newNpc.PropertyChanged += control.OnSettingsPropertyChanged;
            }
            control.UpdateVisuals();
        }
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateVisuals();
    }

    private static void OnDataOrSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CapsuleControl control)
        {
            control.UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        var s = Settings;
        var p = PriceInfo ?? new GoldPriceInfo { XauUsd = 2938.5, XauChangeRate = 0.52, DomesticAu = 686.50, DomesticChangeRate = -0.15 };

        if (s == null) return;

        // 0. 更新矢量缩放比例 (限制在 0.5 到 3.0 之间)
        double scale = Math.Max(0.5, Math.Min(3.0, s.UiScale));
        CapsuleScaleTransform.ScaleX = scale;
        CapsuleScaleTransform.ScaleY = scale;

        // 1. 全局字体
        try
        {
            FontFamily = new FontFamily(s.FontFamily);
        }
        catch { }

        // 2. 背景与边框
        OuterBorder.Background = ParseBrush(s.CapsuleBackground, "#D918181A");
        OuterBorder.BorderBrush = ParseBrush(s.CapsuleBorderColor, "#25FFFFFF");
        OuterBorder.BorderThickness = new Thickness(1);

        // 3. 国际金价 (XAU) 模块控制
        bool hasXau = s.ShowXau && (s.ShowXauLabel || s.ShowXauPrice || s.ShowXauChangeRate);
        XauPanel.Visibility = hasXau ? Visibility.Visible : Visibility.Collapsed;

        if (hasXau)
        {
            TxtXauLabel.Visibility = s.ShowXauLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtXauLabel.Text = s.XauLabelText;
            TxtXauLabel.Foreground = ParseBrush(s.XauLabelColor, "#8E8E93");

            int xauDecimals = Math.Max(0, Math.Min(2, s.XauPriceDecimals));
            TxtXauPrice.Visibility = s.ShowXauPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtXauPrice.Text = p.XauUsd.ToString($"F{xauDecimals}", CultureInfo.InvariantCulture);
            TxtXauPrice.Foreground = ParseBrush(s.XauPriceColor, "#F2F2F7");

            TxtXauRate.Visibility = s.ShowXauChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtXauRate.Text = FormatRate(p.XauChangeRate, s.ShowXauSign, s.ShowXauPercent);
            TxtXauRate.Foreground = GetRateBrush(p.XauChangeRate, s);
        }

        // 4. 国内金价 (AU9999) 模块控制
        bool hasDom = s.ShowDom && (s.ShowDomLabel || s.ShowDomPrice || s.ShowDomChangeRate);
        DomPanel.Visibility = hasDom ? Visibility.Visible : Visibility.Collapsed;

        if (hasDom)
        {
            TxtDomLabel.Visibility = s.ShowDomLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtDomLabel.Text = s.DomLabelText;
            TxtDomLabel.Foreground = ParseBrush(s.DomLabelColor, "#8E8E93");

            int domDecimals = Math.Max(0, Math.Min(2, s.DomPriceDecimals));
            TxtDomPrice.Visibility = s.ShowDomPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtDomPrice.Text = p.DomesticAu.ToString($"F{domDecimals}", CultureInfo.InvariantCulture);
            TxtDomPrice.Foreground = ParseBrush(s.DomPriceColor, "#F2F2F7");

            TxtDomRate.Visibility = s.ShowDomChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtDomRate.Text = FormatRate(p.DomesticChangeRate, s.ShowDomSign, s.ShowDomPercent);
            TxtDomRate.Foreground = GetRateBrush(p.DomesticChangeRate, s);
        }

        // 5. 分割线：只有两者都同时存在有效内容时才显示
        DividerRect.Visibility = (hasXau && hasDom) ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string FormatRate(double rate, bool showSign, bool showPercent)
    {
        string sign = "";
        if (showSign)
        {
            if (rate > 0.0001) sign = "+";
            else if (rate < -0.0001) sign = ""; // 负数自带减号
        }
        else
        {
            rate = Math.Abs(rate);
        }

        string percent = showPercent ? "%" : "";
        return $"{sign}{rate:F2}{percent}";
    }

    private static Brush GetRateBrush(double rate, AppSettings s)
    {
        if (rate > 0.0001) return ParseBrush(s.UpColor, "#C07D00");
        if (rate < -0.0001) return ParseBrush(s.DownColor, "#4EAF50");
        return ParseBrush(s.FlatColor, "#8E8E93");
    }

    private static Brush ParseBrush(string? hex, string defaultHex)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                var brush = (Brush)new BrushConverter().ConvertFromString(hex)!;
                if (brush.CanFreeze) brush.Freeze();
                return brush;
            }
        }
        catch { }

        var fallback = (Brush)new BrushConverter().ConvertFromString(defaultHex)!;
        if (fallback.CanFreeze) fallback.Freeze();
        return fallback;
    }
}
