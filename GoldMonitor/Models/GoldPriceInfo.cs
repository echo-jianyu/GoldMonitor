using System;

namespace GoldMonitor.Models;

public class GoldPriceInfo
{
    public double XauUsd { get; set; }              // 伦敦金 (美元/盎司)
    public double XauLastClose { get; set; }         // 昨收
    public double XauChangeRate { get; set; }        // 涨跌幅 (%)

    public double DomesticAu { get; set; }          // 国内现货 Au99.99 (元/克)
    public double DomLastClose { get; set; }         // 昨收
    public double DomesticChangeRate { get; set; }   // 涨跌幅 (%)

    public double AutdGoldPrice { get; set; }        // 上海金交所 Au(T+D) 黄金延期 (元/克)
    public double AutdLastClose { get; set; }        // 昨结算
    public double AutdChangeRate { get; set; }       // 涨跌幅 (%)

    public double MsGoldPrice { get; set; }          // 京东金融-民生积存金 (元/克)
    public double MsChangeRate { get; set; }         // 涨跌幅 (%)

    public double ZsGoldPrice { get; set; }          // 京东金融-浙商积存金 (元/克)
    public double ZsChangeRate { get; set; }         // 涨跌幅 (%)

    public DateTime UpdateTime { get; set; } = DateTime.Now;
}