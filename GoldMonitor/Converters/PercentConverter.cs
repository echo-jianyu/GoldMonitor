using System;
using System.Globalization;
using System.Windows.Data;

namespace GoldMonitor.Converters;

/// <summary>
/// 百分比数值与输入文本互转：0.15 ↔ "15"（小数如 0.125 ↔ "12.5"）。
/// 用于 Slider + TextBox 组合，让用户既能拖动滑块也能直接输入百分比数字。
/// ConverterParameter 传 "int" 时按整数百分比处理，并按 1% 步长就近取整
/// （配合滑块的 TickFrequency=0.01 使用，输入与拖动始终落在 1% 刻度上）。
/// 回写时容错：忽略 % 号与空白，解析失败不回写（不打断输入）；
/// 越界值不在此钳制，交由保存时的边界修正统一处理。
/// </summary>
public class PercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            double percent = d * 100;
            return IsIntegerMode(parameter)
                ? Math.Round(percent).ToString("0", culture)
                : percent.ToString("0.##", culture);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string text = (value?.ToString() ?? "").Replace("%", "").Trim();
        if (double.TryParse(text, NumberStyles.Float, culture, out double percent))
        {
            if (IsIntegerMode(parameter))
                percent = Math.Round(percent, MidpointRounding.AwayFromZero);
            return percent / 100.0;
        }

        return Binding.DoNothing;
    }

    private static bool IsIntegerMode(object parameter) =>
        string.Equals(parameter as string, "int", StringComparison.OrdinalIgnoreCase);
}
