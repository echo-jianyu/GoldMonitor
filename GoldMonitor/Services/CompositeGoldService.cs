using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;

namespace GoldMonitor.Services;

/// <summary>
/// 组合行情服务：并发拉取多个数据源并合并结果，单一数据源异常不影响其余数据展示
/// </summary>
public class CompositeGoldService : IGoldService
{
    private readonly IGoldService _sinaService;
    private readonly IGoldService _jdService;

    public CompositeGoldService(IGoldService sinaService, IGoldService jdService)
    {
        _sinaService = sinaService;
        _jdService = jdService;
    }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        // 两个数据源并发请求，互不阻塞
        var sinaTask = _sinaService.FetchPricesAsync(ct);
        var jdTask = _jdService.FetchPricesAsync(ct);

        GoldPriceInfo? sina = null;
        GoldPriceInfo? jd = null;

        // 各数据源独立容错：任一失败时其余数据照常展示
        try { sina = await sinaTask; } catch { }
        try { jd = await jdTask; } catch { }

        if (sina == null && jd == null)
        {
            throw new HttpRequestException("所有行情数据源均请求失败");
        }

        var result = sina ?? new GoldPriceInfo();  // 新浪金融
        if (jd != null)
        {
            // 京东积存金
            result.MsGoldPrice = jd.MsGoldPrice;
            result.MsChangeRate = jd.MsChangeRate;
            result.ZsGoldPrice = jd.ZsGoldPrice;
            result.ZsChangeRate = jd.ZsChangeRate;
        }
        result.UpdateTime = DateTime.Now;
        return result;
    }
}
