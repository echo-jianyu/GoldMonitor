using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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

    /// <summary>
    /// 单个行情模块的 UI 描述：设置/行情数据的取值器 + 对应的命名控件。
    /// 模块设置用选择器从"当前" Settings 上取值（而不是捕获实例引用），
    /// 避免 Settings 整体替换后模块描述还指向旧实例。
    /// </summary>
    private sealed class ModuleVisual
    {
        public ModuleVisual(Func<AppSettings, ModuleSettings?> selectSettings,
                            StackPanel panel,
                            TextBlock label, TextBlock price, TextBlock rate,
                            Func<GoldPriceInfo, double> selectPrice, Func<GoldPriceInfo, double> selectRate)
        {
            SelectSettings = selectSettings;
            Panel = panel;
            Label = label;
            Price = price;
            Rate = rate;
            SelectPrice = selectPrice;
            SelectRate = selectRate;
        }

        public Func<AppSettings, ModuleSettings?> SelectSettings { get; }
        public StackPanel Panel { get; }
        public TextBlock Label { get; }
        public TextBlock Price { get; }
        public TextBlock Rate { get; }
        public Func<GoldPriceInfo, double> SelectPrice { get; }
        public Func<GoldPriceInfo, double> SelectRate { get; }
    }

    private readonly ModuleVisual[] _modules;
    private readonly Rectangle[] _dividers;

    public CapsuleControl()
    {
        InitializeComponent();

        // 5 个行情模块的 UI 描述（顺序与 XAML 布局一致）
        _modules = new[]
        {
            new ModuleVisual(s => s.Xau,  XauPanel,  TxtXauLabel,  TxtXauPrice,  TxtXauRate,  p => p.XauUsd,        p => p.XauChangeRate),
            new ModuleVisual(s => s.Dom,  DomPanel,  TxtDomLabel,  TxtDomPrice,  TxtDomRate,  p => p.DomesticAu,     p => p.DomesticChangeRate),
            new ModuleVisual(s => s.Autd, AutdPanel, TxtAutdLabel, TxtAutdPrice, TxtAutdRate, p => p.AutdGoldPrice,  p => p.AutdChangeRate),
            new ModuleVisual(s => s.Ms,   MsPanel,   TxtMsLabel,   TxtMsPrice,   TxtMsRate,   p => p.MsGoldPrice,    p => p.MsChangeRate),
            new ModuleVisual(s => s.Zs,   ZsPanel,   TxtZsLabel,   TxtZsPrice,   TxtZsRate,   p => p.ZsGoldPrice,    p => p.ZsChangeRate),
        };

        // 4 根模块间分割微线（第 i 根位于模块 i 与 i+1 之间）
        _dividers = new[] { Divider1, Divider2, Divider3, Divider4 };
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

            // 数据变化时触发脉冲动画
            if (e.Property == PriceInfoProperty && e.NewValue is GoldPriceInfo info && info.UpdateTime > DateTime.MinValue)
            {
                control.PlayRefreshPulse();
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
        if (s == null) return;

        // 无行情数据时以全零对象占位：各模块直接显示 0.00，让用户直观看到数据未获取到
        var p = PriceInfo ?? new GoldPriceInfo();

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

        // 3. 各行情模块统一更新（数据驱动循环）。
        // 数据获取失败时价格显示为 0.00，让用户直观看到行情暂不可用
        bool[] visible = new bool[_modules.Length];
        for (int i = 0; i < _modules.Length; i++)
        {
            var mv = _modules[i];
            var m = mv.SelectSettings(s);

            // 模块开启且至少保留一项显示内容时才可见
            bool hasContent = m != null && m.Show && (m.ShowLabel || m.ShowPrice || m.ShowChangeRate);
            visible[i] = hasContent;
            mv.Panel.Visibility = hasContent ? Visibility.Visible : Visibility.Collapsed;

            if (m == null) continue;

            mv.Label.Visibility = m.ShowLabel ? Visibility.Visible : Visibility.Collapsed;
            mv.Label.Text = m.LabelText;
            mv.Label.Foreground = ParseBrush(m.LabelColor, "#8E8E93");

            int decimals = Math.Max(0, Math.Min(2, m.PriceDecimals));
            mv.Price.Visibility = m.ShowPrice ? Visibility.Visible : Visibility.Collapsed;
            mv.Price.Text = mv.SelectPrice(p).ToString($"F{decimals}", CultureInfo.InvariantCulture);
            mv.Price.Foreground = ParseBrush(m.PriceColor, "#F2F2F7");

            mv.Rate.Visibility = m.ShowChangeRate ? Visibility.Visible : Visibility.Collapsed;
            mv.Rate.Text = FormatRate(mv.SelectRate(p), m.ShowSign, m.ShowPercent);
            mv.Rate.Foreground = GetRateBrush(mv.SelectRate(p), s);
        }

        // 4. 分割线：全局开关开启、左侧模块可见、且右侧仍存在其它可见模块时才显示
        for (int i = 0; i < _dividers.Length; i++)
        {
            bool rightVisible = false;
            for (int j = i + 1; j < visible.Length; j++)
            {
                if (visible[j])
                {
                    rightVisible = true;
                    break;
                }
            }

            _dividers[i].Visibility = s.ShowDividers && visible[i] && rightVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
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
        return $"{sign}{rate.ToString("F2", CultureInfo.InvariantCulture)}{percent}";
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
    /// 画刷缓存：按 HEX 字符串缓存冻结后的画刷，避免每次刷新重复解析。
    /// 仅 UI 线程访问，无需加锁。
    /// </summary>
    private static readonly Dictionary<string, Brush> BrushCache = new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 解析 HEX 字符串为 Brush，解析失败时回退默认色
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="defaultHex"></param>
    /// <returns></returns>
    private static Brush ParseBrush(string? hex, string defaultHex)
    {
        // net48 的 BCL 未标注 [NotNullWhen]，编译器无法推断此分支 hex 非空，需手动断言
        string key = string.IsNullOrWhiteSpace(hex) ? defaultHex : hex!.Trim();

        // 命中缓存直接复用冻结画刷
        if (BrushCache.TryGetValue(key, out Brush? cached) && cached != null)
        {
            return cached;
        }

        // 解析失败时回退默认色（默认值为合法 HEX 常量，必然解析成功）
        if (!TryCreateBrush(key, out Brush? brush) || brush == null)
        {
            brush = ResolveFallback(defaultHex);
        }

        if (brush.CanFreeze) brush.Freeze();
        BrushCache[key] = brush;
        return brush;
    }

    /// <summary>
    /// 从缓存或直接解析获取默认色画刷
    /// </summary>
    private static Brush ResolveFallback(string defaultHex)
    {
        if (BrushCache.TryGetValue(defaultHex, out Brush? fallback) && fallback != null)
        {
            return fallback;
        }

        if (TryCreateBrush(defaultHex, out Brush? created) && created != null)
        {
            return created;
        }

        return Brushes.Transparent;
    }

    /// <summary>
    /// 尝试将 HEX 字符串解析为 Brush
    /// </summary>
    private static bool TryCreateBrush(string hex, out Brush? brush)
    {
        try
        {
            brush = (Brush)new BrushConverter().ConvertFromString(hex);
            return brush != null;
        }
        catch
        {
            brush = null;
            return false;
        }
    }
}
