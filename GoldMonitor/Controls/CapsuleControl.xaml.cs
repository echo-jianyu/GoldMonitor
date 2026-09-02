using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

    /// <summary>
    /// Settings对象变化，换绑PropertyChanged事件，确保UI能够响应设置的动态变化
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnSettingsObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CapsuleControl control)
        {
            if (e.OldValue is INotifyPropertyChanged oldNpc)
            {
                oldNpc.PropertyChanged -= control.OnSettingsPropertyChanged;  // 解绑旧对象事件
            }
            if (e.NewValue is INotifyPropertyChanged newNpc)
            {
                newNpc.PropertyChanged += control.OnSettingsPropertyChanged;   // 绑定新对象事件
            }
            control.UpdateVisuals();
        }
    }

    /// <summary>
    /// Settings对象的属性变化时触发，更新UI显示
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateVisuals();
    }

    /// <summary>
    /// PriceInfo 或 Settings 变化时触发，更新UI显示
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnDataOrSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CapsuleControl control)
        {
            control.UpdateVisuals();

            // 数据变化时触发脉冲动画 + 更新时间 Tooltip
            if (e.Property == PriceInfoProperty && e.NewValue is GoldPriceInfo info && info.UpdateTime > DateTime.MinValue)
            {
                control.PlayRefreshPulse();
                control.OuterBorder.ToolTip = $"最后更新：{info.UpdateTime:HH:mm:ss}";
            }
        }
    }

    /// <summary>
    /// 数据刷新脉冲：短暂提亮边框后恢复，给用户"数据已更新"的视觉反馈 
    /// </summary>
    private void PlayRefreshPulse()
    {
        if (OuterBorder.BorderBrush is SolidColorBrush brush)
        {
            // UpdateVisuals 会对画刷 Freeze，冻结对象不可动画；克隆一个可写的副本再播放脉冲
            brush = brush.CloneCurrentValue();
            OuterBorder.BorderBrush = brush;
            var pulse = new ColorAnimation
            {
                From = Colors.White,
                To = brush.Color,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, pulse);
        }
    }

    /// <summary>
    /// 根据当前的 PriceInfo 和 Settings 更新 UI 显示
    /// </summary>
    public void UpdateVisuals()
    {
        var s = Settings;
        // 无行情数据时以全零对象占位：各模块直接显示 0.00，让用户直观看到数据未获取到
        var p = PriceInfo ?? new GoldPriceInfo();

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
        // 数据获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
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
        // 数据获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
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

        // 5. 黄金延期 Au(T+D) 模块控制
        // 数据获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
        bool hasAutd = s.ShowAutd && (s.ShowAutdLabel || s.ShowAutdPrice || s.ShowAutdChangeRate);
        AutdPanel.Visibility = hasAutd ? Visibility.Visible : Visibility.Collapsed;

        if (hasAutd)
        {
            TxtAutdLabel.Visibility = s.ShowAutdLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtAutdLabel.Text = s.AutdLabelText;
            TxtAutdLabel.Foreground = ParseBrush(s.AutdLabelColor, "#8E8E93");

            int autdDecimals = Math.Max(0, Math.Min(2, s.AutdPriceDecimals));
            TxtAutdPrice.Visibility = s.ShowAutdPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtAutdPrice.Text = p.AutdGoldPrice.ToString($"F{autdDecimals}", CultureInfo.InvariantCulture);
            TxtAutdPrice.Foreground = ParseBrush(s.AutdPriceColor, "#F2F2F7");

            TxtAutdRate.Visibility = s.ShowAutdChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtAutdRate.Text = FormatRate(p.AutdChangeRate, s.ShowAutdSign, s.ShowAutdPercent);
            TxtAutdRate.Foreground = GetRateBrush(p.AutdChangeRate, s);
        }

        // 6. 换算金价 (XAU→CNY) 模块控制
        // 上游数据缺失时换算结果为 0.00，让用户直观看到行情暂不可用
        bool hasCnv = s.ShowCnv && (s.ShowCnvLabel || s.ShowCnvPrice || s.ShowCnvChangeRate);
        CnvPanel.Visibility = hasCnv ? Visibility.Visible : Visibility.Collapsed;

        if (hasCnv)
        {
            TxtCnvLabel.Visibility = s.ShowCnvLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtCnvLabel.Text = s.CnvLabelText;
            TxtCnvLabel.Foreground = ParseBrush(s.CnvLabelColor, "#8E8E93");

            int cnvDecimals = Math.Max(0, Math.Min(2, s.CnvPriceDecimals));
            TxtCnvPrice.Visibility = s.ShowCnvPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtCnvPrice.Text = p.CnyGoldPrice.ToString($"F{cnvDecimals}", CultureInfo.InvariantCulture);
            TxtCnvPrice.Foreground = ParseBrush(s.CnvPriceColor, "#F2F2F7");

            TxtCnvRate.Visibility = s.ShowCnvChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtCnvRate.Text = FormatRate(p.CnyGoldChangeRate, s.ShowCnvSign, s.ShowCnvPercent);
            TxtCnvRate.Foreground = GetRateBrush(p.CnyGoldChangeRate, s);
        }

        // 7. 民生积存金模块控制
        // 京东数据源获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
        bool hasMs = s.ShowMs && (s.ShowMsLabel || s.ShowMsPrice || s.ShowMsChangeRate);
        MsPanel.Visibility = hasMs ? Visibility.Visible : Visibility.Collapsed;

        if (hasMs)
        {
            TxtMsLabel.Visibility = s.ShowMsLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtMsLabel.Text = s.MsLabelText;
            TxtMsLabel.Foreground = ParseBrush(s.MsLabelColor, "#8E8E93");

            int msDecimals = Math.Max(0, Math.Min(2, s.MsPriceDecimals));
            TxtMsPrice.Visibility = s.ShowMsPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtMsPrice.Text = p.MsGoldPrice.ToString($"F{msDecimals}", CultureInfo.InvariantCulture);
            TxtMsPrice.Foreground = ParseBrush(s.MsPriceColor, "#F2F2F7");

            TxtMsRate.Visibility = s.ShowMsChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtMsRate.Text = FormatRate(p.MsChangeRate, s.ShowMsSign, s.ShowMsPercent);
            TxtMsRate.Foreground = GetRateBrush(p.MsChangeRate, s);
        }

        // 8. 浙商积存金模块控制
        // 京东数据源获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
        bool hasZs = s.ShowZs && (s.ShowZsLabel || s.ShowZsPrice || s.ShowZsChangeRate);
        ZsPanel.Visibility = hasZs ? Visibility.Visible : Visibility.Collapsed;

        if (hasZs)
        {
            TxtZsLabel.Visibility = s.ShowZsLabel ? Visibility.Visible : Visibility.Collapsed;
            TxtZsLabel.Text = s.ZsLabelText;
            TxtZsLabel.Foreground = ParseBrush(s.ZsLabelColor, "#8E8E93");

            int zsDecimals = Math.Max(0, Math.Min(2, s.ZsPriceDecimals));
            TxtZsPrice.Visibility = s.ShowZsPrice ? Visibility.Visible : Visibility.Collapsed;
            TxtZsPrice.Text = p.ZsGoldPrice.ToString($"F{zsDecimals}", CultureInfo.InvariantCulture);
            TxtZsPrice.Foreground = ParseBrush(s.ZsPriceColor, "#F2F2F7");

            TxtZsRate.Visibility = s.ShowZsChangeRate ? Visibility.Visible : Visibility.Collapsed;
            TxtZsRate.Text = FormatRate(p.ZsChangeRate, s.ShowZsSign, s.ShowZsPercent);
            TxtZsRate.Foreground = GetRateBrush(p.ZsChangeRate, s);
        }

        // 9. 分割线：全局开关开启、左侧模块可见、且右侧仍存在其它可见模块时才显示
        bool showDivider = s.ShowDividers;
        // Divider1 位于 Xau 与 Dom 之间：当 Dom 隐藏但后续模块可见时，由 Divider1 承担分隔
        Divider1.Visibility = (showDivider && hasXau && (hasDom || hasAutd || hasCnv || hasMs || hasZs)) ? Visibility.Visible : Visibility.Collapsed;
        // Divider2 位于 Dom 与 Autd 之间
        Divider2.Visibility = (showDivider && hasDom && (hasAutd || hasCnv || hasMs || hasZs)) ? Visibility.Visible : Visibility.Collapsed;
        // Divider3 位于 Autd 与 Cnv 之间
        Divider3.Visibility = (showDivider && hasAutd && (hasCnv || hasMs || hasZs)) ? Visibility.Visible : Visibility.Collapsed;
        // Divider4 位于 Cnv 与 Ms 之间
        Divider4.Visibility = (showDivider && hasCnv && (hasMs || hasZs)) ? Visibility.Visible : Visibility.Collapsed;
        // Divider5 位于 Ms 与 Zs 之间
        Divider5.Visibility = (showDivider && hasMs && hasZs) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// 格式化涨跌幅文本，支持显示正负号和百分号
    /// </summary>
    /// <param name="rate">涨跌幅</param>
    /// <param name="showSign">是否显示正负号</param>
    /// <param name="showPercent">是否显示百分号</param>
    /// <returns></returns>
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

    /// <summary>
    /// 根据涨跌幅返回对应的画刷颜色
    /// </summary>
    /// <param name="rate">涨跌幅</param>
    /// <param name="s">应用设置</param>
    /// <returns></returns>
    private static Brush GetRateBrush(double rate, AppSettings s)
    {
        if (rate > 0.0001) return ParseBrush(s.UpColor, "#C07D00");
        if (rate < -0.0001) return ParseBrush(s.DownColor, "#4EAF50");
        return ParseBrush(s.FlatColor, "#8E8E93");  // 平盘
    }

    /// <summary>
    /// 解析 HEX 字符串为 Brush
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="defaultHex"></param>
    /// <returns></returns>
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
