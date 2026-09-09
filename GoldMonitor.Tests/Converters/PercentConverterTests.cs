using System.Globalization;
using System.Windows.Data;
using GoldMonitor.Converters;

namespace GoldMonitor.Tests.Converters;

[TestClass]
public class PercentConverterTests
{
    private readonly PercentConverter _converter = new PercentConverter();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void Convert_Double_ReturnsPercentString()
    {
        // Act：0.15 → "15"
        var result = _converter.Convert(0.15, typeof(string), null, _culture);

        // Assert
        Assert.AreEqual("15", result);
    }

    [TestMethod]
    public void Convert_IntMode_RoundsToInteger()
    {
        // Act：整数模式下 12.5 按银行家舍入 → "12"
        var result = _converter.Convert(0.125, typeof(string), "int", _culture);

        // Assert
        Assert.AreEqual("12", result);
    }

    [TestMethod]
    public void Convert_NonDouble_ReturnsEmptyString()
    {
        // Act
        var result = _converter.Convert("不是数字", typeof(string), null, _culture);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertBack_ToleratesPercentAndWhitespace()
    {
        // Act："15%" 与 " 15 " 都应解析为 0.15
        var withPercent = _converter.ConvertBack("15%", typeof(double), null, _culture);
        var withSpace = _converter.ConvertBack(" 15 ", typeof(double), null, _culture);

        // Assert
        Assert.AreEqual(0.15, (double)withPercent, 1e-9);
        Assert.AreEqual(0.15, (double)withSpace, 1e-9);
    }

    [TestMethod]
    public void ConvertBack_IntMode_RoundsAwayFromZero()
    {
        // Act："12.6" 在整数模式下按远离零舍入 → 13 → 0.13
        var result = _converter.ConvertBack("12.6", typeof(double), "int", _culture);

        // Assert
        Assert.AreEqual(0.13, (double)result, 1e-9);
    }

    [TestMethod]
    public void ConvertBack_ParseFailure_ReturnsDoNothing()
    {
        // Act
        var result = _converter.ConvertBack("abc", typeof(double), null, _culture);

        // Assert：解析失败不回写（不打断用户输入）
        Assert.AreSame(Binding.DoNothing, result);
    }
}
