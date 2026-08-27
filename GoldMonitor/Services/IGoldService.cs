using System.Threading;
using System.Threading.Tasks;
using GoldMonitor.Models;

namespace GoldMonitor.Services;

public interface IGoldService
{
    /// <summary>
    /// 异步拉取最新的金价数据
    /// </summary>
    Task<GoldPriceInfo> FetchPricesAsync(CancellationToken ct = default);
}