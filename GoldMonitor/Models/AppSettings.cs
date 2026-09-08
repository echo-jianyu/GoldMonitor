using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GoldMonitor.Models;

public partial class AppSettings : ObservableObject
{
    // 1. 行为与常规
    [ObservableProperty]
    private bool _autoStart = false;

    [ObservableProperty]
    private bool _autoHideOnFullScreen = true;

    [ObservableProperty]
    private int _refreshIntervalSeconds = 5;

    [ObservableProperty]
    private double? _windowLeft;

    [ObservableProperty]
    private double? _windowTop;

    // 2. 外观与透明度
    [ObservableProperty]
    private double _uiScale = 1.25;

    [ObservableProperty]
    private string _capsuleBackground = "#D918181A";

    [ObservableProperty]
    private string _capsuleBorderColor = "#25FFFFFF";

    [ObservableProperty]
    private double _idleOpacity = 0.20;

    [ObservableProperty]
    private double _hoverOpacity = 0.90;

    [ObservableProperty]
    private string _fontFamily = "Microsoft YaHei UI";

    [ObservableProperty]
    private bool _showDividers = true;

    // 3. 行情模块
    [ObservableProperty]
    private ModuleSettings _xau = new() { Show = true, LabelText = "XAU" };

    [ObservableProperty]
    private ModuleSettings _dom = new() { Show = false, LabelText = "AU" };

    [ObservableProperty]
    private ModuleSettings _autd = new() { Show = false, LabelText = "AuTD" };

    [ObservableProperty]
    private ModuleSettings _ms = new() { Show = false, LabelText = "民生" };

    [ObservableProperty]
    private ModuleSettings _zs = new() { Show = true, LabelText = "浙商" };

    // 4. 涨跌配色
    [ObservableProperty]
    private string _upColor = "#C07D00";

    [ObservableProperty]
    private string _downColor = "#4EAF50";

    [ObservableProperty]
    private string _flatColor = "#8E8E93";

    public AppSettings()
    {
        // 字段初始化器直接赋值字段，不经过 setter 也不会触发 OnXxxChanged，需手动订阅初始模块实例
        Xau.PropertyChanged += OnModulePropertyChanged;
        Dom.PropertyChanged += OnModulePropertyChanged;
        Autd.PropertyChanged += OnModulePropertyChanged;
        Ms.PropertyChanged += OnModulePropertyChanged;
        Zs.PropertyChanged += OnModulePropertyChanged;
    }

    /// <summary>
    /// 模块属性变化 → 以模块属性名向上转发。
    /// </summary>
    /// <param name="sender">来源模块</param>
    /// <param name="e">变化属性</param>
    private void OnModulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 同一处理器挂在 5 个模块上，用引用相等判断来源模块
        if (ReferenceEquals(sender, Xau)) OnPropertyChanged(nameof(Xau));
        else if (ReferenceEquals(sender, Dom)) OnPropertyChanged(nameof(Dom));
        else if (ReferenceEquals(sender, Autd)) OnPropertyChanged(nameof(Autd));
        else if (ReferenceEquals(sender, Ms)) OnPropertyChanged(nameof(Ms));
        else if (ReferenceEquals(sender, Zs)) OnPropertyChanged(nameof(Zs));
    }

    /// <summary>
    /// 模块对象被整体替换时（JSON 反序列化 / Clone 赋值）重新订阅事件
    /// </summary>
    /// <param name="oldValue">旧模块实例（可能为 null）</param>
    /// <param name="newValue">新模块实例（可能为 null）</param>
    /// <param name="assign">把归一化后的模块写回对应属性</param>
    /// <param name="createDefault">该模块的默认实例工厂</param>
    private void RewireModule(ModuleSettings? oldValue, ModuleSettings? newValue, Action<ModuleSettings> assign, Func<ModuleSettings> createDefault)
    {
        if (ReferenceEquals(oldValue, newValue)) return;

        if (oldValue != null)
        {
            oldValue.PropertyChanged -= OnModulePropertyChanged;
        }

        if (newValue == null)
        {
            // null 归一化为默认实例：assign 会再次进入对应 hook（oldValue 为 null），完成订阅后返回，递归一次即终止
            assign(createDefault());
            return;
        }

        newValue.PropertyChanged += OnModulePropertyChanged;
    }

    partial void OnXauChanged(ModuleSettings? oldValue, ModuleSettings? newValue)
        => RewireModule(oldValue, newValue, m => Xau = m, () => new ModuleSettings { Show = true, LabelText = "XAU" });

    partial void OnDomChanged(ModuleSettings? oldValue, ModuleSettings? newValue)
        => RewireModule(oldValue, newValue, m => Dom = m, () => new ModuleSettings { Show = false, LabelText = "AU" });

    partial void OnAutdChanged(ModuleSettings? oldValue, ModuleSettings? newValue)
        => RewireModule(oldValue, newValue, m => Autd = m, () => new ModuleSettings { Show = false, LabelText = "AuTD" });

    partial void OnMsChanged(ModuleSettings? oldValue, ModuleSettings? newValue)
        => RewireModule(oldValue, newValue, m => Ms = m, () => new ModuleSettings { Show = false, LabelText = "民生" });

    partial void OnZsChanged(ModuleSettings? oldValue, ModuleSettings? newValue)
        => RewireModule(oldValue, newValue, m => Zs = m, () => new ModuleSettings { Show = true, LabelText = "浙商" });

    /// <summary>
    /// 深拷贝配置副本：模块逐个克隆，与源实例完全隔离
    /// </summary>
    public AppSettings Clone()
    {
        var c = new AppSettings
        {
            AutoStart = AutoStart,
            AutoHideOnFullScreen = AutoHideOnFullScreen,
            RefreshIntervalSeconds = RefreshIntervalSeconds,
            WindowLeft = WindowLeft,
            WindowTop = WindowTop,
            UiScale = UiScale,
            CapsuleBackground = CapsuleBackground,
            CapsuleBorderColor = CapsuleBorderColor,
            IdleOpacity = IdleOpacity,
            HoverOpacity = HoverOpacity,
            FontFamily = FontFamily,
            ShowDividers = ShowDividers,
            UpColor = UpColor,
            DownColor = DownColor,
            FlatColor = FlatColor
        };

        // 模块经 setter 赋值触发 OnXxxChanged，新实例自动完成事件订阅
        c.Xau = Xau.Clone();
        c.Dom = Dom.Clone();
        c.Autd = Autd.Clone();
        c.Ms = Ms.Clone();
        c.Zs = Zs.Clone();

        return c;
    }
}
