using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;
using GoldMonitor.Services;

namespace GoldMonitor.Tests.Services;

[TestClass]
public class CompositeGoldServiceTests
{
    /// <summary>
    /// 测试用假行情服务：可配置返回数据、抛出异常、延迟与调用计数
    /// </summary>
    private sealed class FakeGoldService : IGoldService
    {
        public GoldPriceInfo Result { get; set; } = new GoldPriceInfo();
        public Exception? Error { get; set; }
        public int CallCount { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
        {
            CallCount++;
            LastToken = ct;
            await Task.Yield();  // 模拟真实异步
            if (Error != null)
            {
                throw Error;
            }
            return Result;
        }
    }

    [TestMethod]
    public async Task Fetch_OneSourceFails_KeepsOtherData()
    {
        // Arrange：新浪抛异常，京东正常返回
        var sina = new FakeGoldService { Error = new HttpRequestException("模拟网络失败") };
        var jd = new FakeGoldService
        {
            Result = new GoldPriceInfo { MsGoldPrice = 990.46, MsChangeRate = -0.48, ZsGoldPrice = 990.79, ZsChangeRate = -0.47 }
        };
        var service = new CompositeGoldService(sina, jd);

        // Act
        var result = await service.FetchPricesAsync();

        // Assert：京东数据照常展示，新浪字段为 0
        Assert.AreEqual(990.46, result.MsGoldPrice, 1e-9);
        Assert.AreEqual(990.79, result.ZsGoldPrice, 1e-9);
        Assert.AreEqual(0.0, result.XauUsd, 1e-9);
        Assert.AreEqual(0.0, result.DomesticAu, 1e-9);
    }

    [TestMethod]
    public async Task Fetch_AllSourcesFail_ThrowsHttpRequestException()
    {
        // Arrange
        var sina = new FakeGoldService { Error = new HttpRequestException("模拟网络失败") };
        var jd = new FakeGoldService { Error = new HttpRequestException("模拟网络失败") };
        var service = new CompositeGoldService(sina, jd);

        // Act / Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.FetchPricesAsync());
    }

    [TestMethod]
    public async Task Fetch_MergesJdFieldsIntoSina_WithoutOverwrite()
    {
        // Arrange
        var sina = new FakeGoldService
        {
            Result = new GoldPriceInfo { XauUsd = 4637.73, XauChangeRate = -0.45, DomesticAu = 690.20, AutdGoldPrice = 680.40 }
        };
        var jd = new FakeGoldService
        {
            Result = new GoldPriceInfo { MsGoldPrice = 990.46, MsChangeRate = -0.48, ZsGoldPrice = 990.79, ZsChangeRate = -0.47 }
        };
        var service = new CompositeGoldService(sina, jd);

        // Act
        var result = await service.FetchPricesAsync();

        // Assert：四个品种字段齐全
        Assert.AreEqual(4637.73, result.XauUsd, 1e-9);
        Assert.AreEqual(690.20, result.DomesticAu, 1e-9);
        Assert.AreEqual(680.40, result.AutdGoldPrice, 1e-9);
        Assert.AreEqual(990.46, result.MsGoldPrice, 1e-9);
        Assert.AreEqual(990.79, result.ZsGoldPrice, 1e-9);
    }

    [TestMethod]
    public async Task Fetch_UpdateTime_RefreshedToNow()
    {
        // Arrange：两个数据源的 UpdateTime 都是 2000 年的旧值，合并后应被刷新为当前时间
        var oldTime = new DateTime(2000, 1, 1);
        var sina = new FakeGoldService { Result = new GoldPriceInfo { XauUsd = 4637.73, UpdateTime = oldTime } };
        var jd = new FakeGoldService { Result = new GoldPriceInfo { MsGoldPrice = 990.46, UpdateTime = oldTime } };
        var service = new CompositeGoldService(sina, jd);
        var before = DateTime.Now;

        // Act
        var result = await service.FetchPricesAsync();

        // Assert
        Assert.IsTrue(result.UpdateTime >= before, "UpdateTime 应被更新为当前时间");
    }

    [TestMethod]
    public async Task Fetch_EachSourceCalledOnce()
    {
        // Arrange
        var sina = new FakeGoldService { Result = new GoldPriceInfo { XauUsd = 4637.73 } };
        var jd = new FakeGoldService { Result = new GoldPriceInfo { MsGoldPrice = 990.46 } };
        var service = new CompositeGoldService(sina, jd);

        // Act
        await service.FetchPricesAsync();

        // Assert
        Assert.AreEqual(1, sina.CallCount);
        Assert.AreEqual(1, jd.CallCount);
    }
}
