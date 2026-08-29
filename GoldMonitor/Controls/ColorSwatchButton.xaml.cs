using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GoldMonitor.Controls;

/// <summary>
/// 颜色选择按钮：展示当前颜色色块，点击弹出预设调色板，支持自定义 HEX 输入
/// </summary>
public partial class ColorSwatchButton : UserControl
{
    // 预设调色板（灰阶 + 常用行情色，在浅色设置窗口背景上均可辨识）
    private static readonly string[] Palette =
    {
        "#FFFFFF", "#F2F2F7", "#C7C7CC", "#8E8E93", "#636366", "#3A3A3C",
        "#FFD700", "#C07D00", "#FF9500", "#E53935", "#FF453A",
        "#43A047", "#4EAF50", "#1DB270", "#007AFF", "#64D2FF", "#BF5AF2", "#FF2D55"
    };

    public static readonly DependencyProperty ColorHexProperty =
        DependencyProperty.Register(nameof(ColorHex), typeof(string), typeof(ColorSwatchButton),
            new FrameworkPropertyMetadata("#8E8E93",
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColorHexChanged));

    public string ColorHex
    {
        get => (string)GetValue(ColorHexProperty);
        set => SetValue(ColorHexProperty, value);
    }

    public ColorSwatchButton()
    {
        InitializeComponent();

        // 构建预设调色板
        foreach (var hex in Palette)
        {
            var swatch = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(5),
                Background = ParseBrush(hex),
                BorderBrush = ParseBrush("#E5E5EA"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                Tag = hex,
                ToolTip = hex,
                Cursor = Cursors.Hand
            };
            swatch.MouseLeftButtonUp += PresetSwatch_Click;
            PaletteGrid.Children.Add(swatch);
        }

        UpdateSwatchVisual();
    }

    private void PresetSwatch_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border { Tag: string hex })
        {
            // 同步 HEX 输入框，避免弹窗关闭触发 LostFocus 时把旧值写回
            HexInput.Text = hex;
            SetCurrentValue(ColorHexProperty, hex);
            PalettePopup.IsOpen = false;
        }
    }

    private void SwatchButton_Click(object sender, RoutedEventArgs e)
    {
        HexInput.Text = ColorHex ?? string.Empty;
        PalettePopup.IsOpen = true;
        HexInput.Focus();
        HexInput.SelectAll();
    }

    private void HexInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitHexInput();
            PalettePopup.IsOpen = false;
        }
    }

    private void HexInput_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitHexInput();
    }

    /// <summary>
    /// 提交自定义 HEX 输入，非法值回退为当前颜色
    /// </summary>
    private void CommitHexInput()
    {
        string text = HexInput.Text?.Trim() ?? string.Empty;

        if (IsValidColor(text))
        {
            SetCurrentValue(ColorHexProperty, text);
        }
        else
        {
            HexInput.Text = ColorHex ?? string.Empty;
        }
    }

    private static void OnColorHexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSwatchButton control)
        {
            control.UpdateSwatchVisual();
        }
    }

    private void UpdateSwatchVisual()
    {
        // XAML 解析期属性可能早于子元素创建，做空值保护
        if (Fill != null)
        {
            Fill.Background = ParseBrush(ColorHex);
        }
        if (SwatchButton != null)
        {
            SwatchButton.ToolTip = $"当前颜色 {ColorHex}，点击修改";
        }
    }

    private static bool IsValidColor(string? hex)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(hex) && new BrushConverter().ConvertFromString(hex) != null;
        }
        catch
        {
            return false;
        }
    }

    private static Brush ParseBrush(string? hex)
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
        return Brushes.Transparent;
    }
}
