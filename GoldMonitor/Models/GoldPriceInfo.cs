using System;
using System.Linq;

namespace GoldMonitor.Models;

/// <summary>
/// 行情数据聚合：5 个行情模块各自的现价、涨跌幅与更新时间
/// </summary>
public class GoldPriceInfo
{
    /// <summary>国际金价 (XAU)</summary>
    public GoldQuote Xau { get; } = new();

    /// <summary>国内金价 (AU9999)</summary>
    public GoldQuote Dom { get; } = new();

    /// <summary>黄金延期 Au(T+D)</summary>
    public GoldQuote Autd { get; } = new();

    /// <summary>京东积存金 - 民生金价</summary>
    public GoldQuote Ms { get; } = new();

    /// <summary>京东积存金 - 浙商金价</summary>
    public GoldQuote Zs { get; } = new();

    /// <summary>
    /// 最近更新时间：各模块时间戳的最大值（脉冲动画的守卫依据）
    /// </summary>
    public DateTime LatestUpdateTime =>
        new[] { Xau.UpdateTime, Dom.UpdateTime, Autd.UpdateTime, Ms.UpdateTime, Zs.UpdateTime }.Max();
}
