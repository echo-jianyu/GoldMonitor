using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;
using GoldMonitor.Services;

namespace GoldMonitor.Tests.Services;

[TestClass]
public class CompositeGoldServiceTests
{
    /// <summary>
    /// 测试用假 HTTP 处理器：按请求返回预设内容或抛异常，并记录调用次数。
    /// 响应统一按 GBK 编码输出（与新浪服务端一致，京东 JSON 为 ASCII 不受影响）。
    /// </summary>
    private sealed class StubHttpHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, string> ResponseFactory{ get; set; } = _ => "";
        public Exception? Error { get; set; }
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (Error != null)
            {
                throw Error;
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseFactory(request), Encoding.GetEncoding("GBK"))
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// 新浪真实响应样例（三语句，与解析器测试共用同一格式）
    /// </summary>
    private const string SinaFixtureText =
        "var hq_str_hf_XAU = \"4637.73,4658.650,4637.73,4638.08,4673.66,4636.11,08:47:00,4658.65,4657.23,0,0,0,2026-08-26,伦敦金（现货黄金）\";\r\n" +
        "var hq_str_gds_AU9999 = \"690.20,700.00,690.20,690.20,700.00,689.00,15:00:00,700.00,700.00,0,0,0,2026-08-26,沪金9999\";\r\n" +
        "var hq_str_gds_AUTD = \"680.40,680.40,680.40,680.40,681.00,680.00,02:30:00,680.40,680.40,0,0,0,2026-08-26,黄金延期\";";

    /// <summary>
    /// 京东两个产品的响应：按 URL 中的 goldType 区分
    /// </summary>
    private const string JdMsJson = """{"resultData":{"data":{"priceValue":"990.46","raisePercent100":"-0.48%"}}}""";
    private const string JdZsJson = """{"resultData":{"data":{"priceValue":"990.79","raisePercent100":"-0.47%"}}}""";

    private static readonly Func<HttpRequestMessage, string> JdResponses = request =>
        request.RequestUri!.Query.Contains("goldType=1") ? JdMsJson : JdZsJson;

    private static SinaGoldService CreateSinaService(StubHttpHandler handler)
    {
        return new SinaGoldService(new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });
    }

    private static JdGoldService CreateJdService(StubHttpHandler handler)
    {
        return new JdGoldService(new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });
    }

    [TestMethod]
    public async Task Fetch_OneSourceFails_KeepsOtherData()
    {
        // Arrange：新浪抛异常，京东正常返回
        var sinaHandler = new StubHttpHandler { Error = new HttpRequestException("模拟网络失败") };
        var jdHandler = new StubHttpHandler { ResponseFactory = JdResponses };
        var service = new CompositeGoldService(CreateSinaService(sinaHandler), CreateJdService(jdHandler));

        // Act
        var result = await service.FetchPricesAsync();

        // Assert：京东数据照常展示，新浪模块为 0
        Assert.AreEqual(990.46, result.Ms.Price, 1e-9);
        Assert.AreEqual(990.79, result.Zs.Price, 1e-9);
        Assert.AreEqual(0.0, result.Xau.Price, 1e-9);
    }

    [TestMethod]
    public async Task Fetch_AllSourcesFail_ThrowsHttpRequestException()
    {
        // Arrange
        var sinaHandler = new StubHttpHandler { Error = new HttpRequestException("模拟网络失败") };
        var jdHandler = new StubHttpHandler { Error = new HttpRequestException("模拟网络失败") };
        var service = new CompositeGoldService(CreateSinaService(sinaHandler), CreateJdService(jdHandler));

        // Act / Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.FetchPricesAsync());
    }

    [TestMethod]
    public async Task Fetch_MergesJdFieldsIntoSina_WithoutOverwrite()
    {
        // Arrange：两个源都正常返回
        var sinaHandler = new StubHttpHandler { ResponseFactory = _ => SinaFixtureText };
        var jdHandler = new StubHttpHandler { ResponseFactory = JdResponses };
        var service = new CompositeGoldService(CreateSinaService(sinaHandler), CreateJdService(jdHandler));

        // Act
        var result = await service.FetchPricesAsync();

        // Assert：五个模块字段齐全，新浪模块不被京东覆盖
        Assert.AreEqual(4637.73, result.Xau.Price, 1e-9);
        Assert.AreEqual(690.20, result.Dom.Price, 1e-9);
        Assert.AreEqual(680.40, result.Autd.Price, 1e-9);
        Assert.AreEqual(990.46, result.Ms.Price, 1e-9);
        Assert.AreEqual(990.79, result.Zs.Price, 1e-9);
    }

    [TestMethod]
    public async Task Fetch_PreservesModuleTimestamps_LatestIsMax()
    {
        // Arrange
        var sinaHandler = new StubHttpHandler { ResponseFactory = _ => SinaFixtureText };
        var jdHandler = new StubHttpHandler { ResponseFactory = JdResponses };
        var service = new CompositeGoldService(CreateSinaService(sinaHandler), CreateJdService(jdHandler));
        var before = DateTime.Now;

        // Act
        var result = await service.FetchPricesAsync();

        // Assert：各模块携带各自服务盖的时间戳，LatestUpdateTime 取最大值
        Assert.IsTrue(result.Xau.UpdateTime >= before, "新浪模块应有本次拉取的时间戳");
        Assert.IsTrue(result.Ms.UpdateTime >= before, "京东模块应有本次拉取的时间戳");
        var expectedMax = new[]
        {
            result.Xau.UpdateTime, result.Dom.UpdateTime, result.Autd.UpdateTime, result.Ms.UpdateTime, result.Zs.UpdateTime
        }.Max();
        Assert.AreEqual(expectedMax, result.LatestUpdateTime);

        // Assert：全新对象的 LatestUpdateTime 为 MinValue（无任何真实数据）
        Assert.AreEqual(DateTime.MinValue, new GoldPriceInfo().LatestUpdateTime);
    }

    [TestMethod]
    public async Task Fetch_SinaOnceJdTwice_RequestsCorrect()
    {
        // Arrange
        var sinaHandler = new StubHttpHandler { ResponseFactory = _ => SinaFixtureText };
        var jdHandler = new StubHttpHandler { ResponseFactory = JdResponses };
        var service = new CompositeGoldService(CreateSinaService(sinaHandler), CreateJdService(jdHandler));

        // Act
        await service.FetchPricesAsync();

        // Assert：新浪 1 次请求，京东 2 次（民生/浙商两个产品各 1 次）
        Assert.AreEqual(1, sinaHandler.CallCount);
        Assert.AreEqual(2, jdHandler.CallCount);
    }
}
