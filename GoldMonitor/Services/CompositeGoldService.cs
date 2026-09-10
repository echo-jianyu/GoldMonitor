using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;

namespace GoldMonitor.Services;

/// <summary>
/// 组合行情服务（聚合层）：并发拉取多个数据源并把各自的报价合并为统一快照。
/// 每个数据源返回自己实际提供的模块报价（SinaQuote/JdQuote），
/// 本服务负责映射合并，单一数据源异常不影响其余数据展示。
/// </summary>
public class CompositeGoldService
{
    private readonly SinaGoldService _sinaService;
    private readonly JdGoldService _jdService;

    public CompositeGoldService(SinaGoldService sinaService, JdGoldService jdService)
    {
        _sinaService = sinaService;
        _jdService = jdService;
    }

    public async Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default)
    {
        // 两个数据源并发请求，互不阻塞
        var sinaTask = _sinaService.FetchPricesAsync(ct);
        var jdTask = _jdService.FetchPricesAsync(ct);

        SinaQuote? sina = null;
        JdQuote? jd = null;

        // 各数据源独立容错：任一失败时其余数据照常展示
        try { sina = await sinaTask; } catch { }
        try { jd = await jdTask; } catch { }

        if (sina == null && jd == null)
        {
            throw new HttpRequestException("所有行情数据源均请求失败");
        }

        // 各数据源的报价映射进统一快照（CopyTo 保留各自的时间戳）
        var result = new GoldPriceInfo();
        if (sina != null)
        {
            sina.Xau.CopyTo(result.Xau);
            sina.Dom.CopyTo(result.Dom);
            sina.Autd.CopyTo(result.Autd);
        }
        if (jd != null)
        {
            jd.Ms.CopyTo(result.Ms);
            jd.Zs.CopyTo(result.Zs);
        }
        return result;
    }
}
