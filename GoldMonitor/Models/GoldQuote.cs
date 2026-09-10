using System;

namespace GoldMonitor.Models;

/// <summary>
/// 单个行情模块的报价（现价/涨跌幅/时间戳）
/// </summary>
public class GoldQuote
{
    /// <summary>
    /// 现价
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// 涨跌幅 (%)
    /// </summary>
    public double ChangeRate { get; set; }

    /// <summary>
    /// 该模块数据的时间戳（各数据源拉取后自行盖戳）
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 把本报价的字段拷贝到目标（聚合层合并各数据源时使用）
    /// </summary>
    public void CopyTo(GoldQuote target)
    {
        target.Price = Price;
        target.ChangeRate = ChangeRate;
        target.UpdateTime = UpdateTime;
    }
}
