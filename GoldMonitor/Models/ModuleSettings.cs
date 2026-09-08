using CommunityToolkit.Mvvm.ComponentModel;

namespace GoldMonitor.Models;

/// <summary>
/// 单个行情模块的显示设置
/// </summary>
public partial class ModuleSettings : ObservableObject
{
    /// <summary>
    /// 模块总开关
    /// </summary>
    [ObservableProperty]
    private bool _show = true;

    /// <summary>
    /// 是否显示标签
    /// </summary>
    [ObservableProperty]
    private bool _showLabel = true;

    /// <summary>
    /// 标签文字
    /// </summary>
    [ObservableProperty]
    private string _labelText = "";

    /// <summary>
    /// 标签颜色（HEX 字符串）
    /// </summary>
    [ObservableProperty]
    private string _labelColor = "#8E8E93";

    /// <summary>
    /// 是否显示价格
    /// </summary>
    [ObservableProperty]
    private bool _showPrice = true;

    /// <summary>
    /// 价格小数位数（0~2）
    /// </summary>
    [ObservableProperty]
    private int _priceDecimals = 2;

    /// <summary>
    /// 价格颜色（HEX 字符串）
    /// </summary>
    [ObservableProperty]
    private string _priceColor = "#F2F2F7";

    /// <summary>
    /// 是否显示涨跌幅
    /// </summary>
    [ObservableProperty]
    private bool _showChangeRate = true;

    /// <summary>
    /// 是否显示涨跌符号（+/-）
    /// </summary>
    [ObservableProperty]
    private bool _showSign = true;

    /// <summary>
    /// 是否显示涨跌百分比（%）
    /// </summary>
    [ObservableProperty]
    private bool _showPercent = true;

    /// <summary>
    /// 浅拷贝副本：全部属性为值类型或 string，MemberwiseClone 即安全
    /// </summary>
    public ModuleSettings Clone()
    {
        return (ModuleSettings)MemberwiseClone();
    }
}
