using GoldMonitor.Services;
using System.Text.Json;

namespace GoldMonitor.Tests.Services;

[TestClass]
public class JdResponseParserTests
{
    /// <summary>
    /// 正常响应样例
    /// </summary>
    private const string JdFixture = """{"resultCode":0,"resultMsg":"success","resultData":{"data":{"priceValue":"990.46","raisePercent100":"-0.48%"}}}""";

    [TestMethod]
    public void Parse_ValidJson_ReturnsPriceAndRate()
    {
        // Act
        var (price, rate) = JdResponseParser.Parse(JdFixture);

        // Assert
        Assert.AreEqual(990.46, price, 1e-9);
        Assert.AreEqual(-0.48, rate, 1e-9);
    }

    [TestMethod]
    public void Parse_RateWithoutPercent_Parsed()
    {
        // Arrange
        string json = """{"resultData":{"data":{"priceValue":"990.46","raisePercent100":"0.35"}}}""";

        // Act
        var (price, rate) = JdResponseParser.Parse(json);

        // Assert
        Assert.AreEqual(990.46, price, 1e-9);
        Assert.AreEqual(0.35, rate, 1e-9);
    }

    [TestMethod]
    public void Parse_MissingResultData_ReturnsZero()
    {
        // Act
        var (price, rate) = JdResponseParser.Parse("""{"resultCode":404}""");

        // Assert
        Assert.AreEqual(0.0, price, 1e-9);
        Assert.AreEqual(0.0, rate, 1e-9);
    }

    [TestMethod]
    public void Parse_MissingData_ReturnsZero()
    {
        // Act
        var (price, rate) = JdResponseParser.Parse("""{"resultData":{}}""");

        // Assert
        Assert.AreEqual(0.0, price, 1e-9);
        Assert.AreEqual(0.0, rate, 1e-9);
    }

    [TestMethod]
    public void Parse_NonNumericPrice_PriceZeroRateOk()
    {
        // Arrange
        string json = """{"resultData":{"data":{"priceValue":"abc","raisePercent100":"-0.48%"}}}""";

        // Act
        var (price, rate) = JdResponseParser.Parse(json);

        // Assert
        Assert.AreEqual(0.0, price, 1e-9);
        Assert.AreEqual(-0.48, rate, 1e-9);
    }

    [TestMethod]
    public void Parse_NonNumericRate_RateZeroPriceOk()
    {
        // Arrange
        string json = """{"resultData":{"data":{"priceValue":"990.46","raisePercent100":"x%"}}}""";

        // Act
        var (price, rate) = JdResponseParser.Parse(json);

        // Assert
        Assert.AreEqual(990.46, price, 1e-9);
        Assert.AreEqual(0.0, rate, 1e-9);
    }

    [TestMethod]
    public async Task Parse_MalformedJson_ThrowsJsonException()
    {
        // Act / Assert：畸形 JSON 保持抛出，由服务的独立容错捕获（MSTest 4 统一用 ThrowsAsync）
        await Assert.ThrowsAsync<JsonException>(() => Task.Run(() => JdResponseParser.Parse("{")));
    }
}
