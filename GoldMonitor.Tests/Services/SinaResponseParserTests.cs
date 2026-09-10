using GoldMonitor.Services;

namespace GoldMonitor.Tests.Services;

[TestClass]
public class SinaResponseParserTests
{
    /// <summary>
    /// 完整三语句样例（首行取自真实响应），三行以 CRLF 分隔
    /// </summary>
    private const string SinaFixture =
        "var hq_str_hf_XAU = \"4637.73,4658.650,4637.73,4638.08,4673.66,4636.11,08:47:00,4658.65,4657.23,0,0,0,2026-08-26,伦敦金（现货黄金）\";\r\n" +
        "var hq_str_gds_AU9999 = \"690.20,700.00,690.20,690.20,700.00,689.00,15:00:00,700.00,700.00,0,0,0,2026-08-26,沪金9999\";\r\n" +
        "var hq_str_gds_AUTD = \"680.40,680.40,680.40,680.40,681.00,680.00,02:30:00,680.40,680.40,0,0,0,2026-08-26,黄金延期\";";

    [TestMethod]
    public void Parse_ThreeStatements_ParsesAllMarketsAndRates()
    {
        // Act
        var quote = SinaResponseParser.Parse(SinaFixture);

        // Assert：parts[0] 为最新价，parts[7] 为昨收/昨结，涨跌幅 = (最新-昨收)/昨收*100
        Assert.AreEqual(4637.73, quote.Xau.Price, 1e-9);
        Assert.AreEqual(-0.4490571, quote.Xau.ChangeRate, 1e-4);

        Assert.AreEqual(690.20, quote.Dom.Price, 1e-9);
        Assert.AreEqual(-1.4, quote.Dom.ChangeRate, 1e-9);

        Assert.AreEqual(680.40, quote.Autd.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Autd.ChangeRate, 1e-9);  // 平盘
    }

    [TestMethod]
    public void Parse_SingleXauStatement_OthersZero()
    {
        // Arrange
        string input = "var hq_str_hf_XAU = \"4637.73,4658.650,4637.73,4638.08,4673.66,4636.11,08:47:00,4658.65,4657.23,0,0,0,2026-08-26,伦敦金（现货黄金）\";";

        // Act
        var quote = SinaResponseParser.Parse(input);

        // Assert
        Assert.AreEqual(4637.73, quote.Xau.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Dom.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Autd.Price, 1e-9);
    }

    [TestMethod]
    public void Parse_MalformedWithoutQuotes_AllZero()
    {
        // Arrange：无引号 → 整句被跳过
        string input = "var hq_str_hf_XAU = 123;";

        // Act
        var quote = SinaResponseParser.Parse(input);

        // Assert
        Assert.AreEqual(0.0, quote.Xau.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Xau.ChangeRate, 1e-9);
    }

    [TestMethod]
    public void Parse_FewerThan8Fields_Skipped()
    {
        // Arrange：引号内只有 3 个字段 → 不满足 parts.Length >= 8
        string input = "var hq_str_hf_XAU = \"1,2,3\";";

        // Act
        var quote = SinaResponseParser.Parse(input);

        // Assert
        Assert.AreEqual(0.0, quote.Xau.Price, 1e-9);
    }

    [TestMethod]
    public void Parse_NonNumericPrice_MarketZero()
    {
        // Arrange：parts[0] 非数字 → 整个品种保持 0
        string input = "var hq_str_hf_XAU = \"abc,1,2,3,4,5,6,4658.65\";";

        // Act
        var quote = SinaResponseParser.Parse(input);

        // Assert
        Assert.AreEqual(0.0, quote.Xau.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Xau.ChangeRate, 1e-9);
    }

    [TestMethod]
    public void Parse_ZeroLastClose_PriceKeptRateZero()
    {
        // Arrange：parts[7] = 0 → 不满足 lastClose > 0，只保留最新价
        string input = "var hq_str_hf_XAU = \"4637.73,1,2,3,4,5,6,0\";";

        // Act
        var quote = SinaResponseParser.Parse(input);

        // Assert
        Assert.AreEqual(4637.73, quote.Xau.Price, 1e-9);
        Assert.AreEqual(0.0, quote.Xau.ChangeRate, 1e-9);
    }

    [TestMethod]
    public void Parse_BlankInput_AllZero()
    {
        // Act：空串与纯空白都应安全返回全 0
        var empty = SinaResponseParser.Parse("");
        var whitespace = SinaResponseParser.Parse("   ");

        // Assert
        Assert.AreEqual(0.0, empty.Xau.Price, 1e-9);
        Assert.AreEqual(0.0, empty.Dom.Price, 1e-9);
        Assert.AreEqual(0.0, whitespace.Autd.Price, 1e-9);
    }
}
