using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoldMonitor.Models;
using GoldMonitor.Services;
using GoldMonitor.Views;

namespace GoldMonitor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IGoldService _goldService;
    private readonly ConfigService _configService;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private GoldPriceInfo? _priceData;

    public MainViewModel(IGoldService goldService, ConfigService configService)
    {
        _goldService = goldService;
        _configService = configService;
        _settings = _configService.LoadConfig();

        // 初始化定时器
        _timer = new DispatcherTimer();
        UpdateTimerInterval();
        _timer.Tick += async (_, _) => await RefreshDataAsync();
        _timer.Start();

        // 立即刷新一次
        _ = RefreshDataAsync();
    }

    /// <summary>
    /// 根据配置更新定时器的刷新间隔
    /// </summary>
    public void UpdateTimerInterval()
    {
        int seconds = Math.Max(1, Math.Min(Settings.RefreshIntervalSeconds, 3600));  // 1~3600秒
        _timer.Interval = TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// 刷新获取一次数据
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task RefreshDataAsync()
    {
        try
        {
            var data = await _goldService.FetchPricesAsync();
            if (data != null)
            {
                PriceData = data;
            }
        }
        catch
        {
            // 网络异常静默跳过，界面保留当前有效数据
        }
    }

    /// <summary>
    /// 打开设置窗口
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        var settingsVm = new SettingsViewModel(Settings.Clone(), _configService, PriceData);
        var settingsWindow = new SettingsWindow(settingsVm)
        {
            Owner = Application.Current.MainWindow
        };

        if (settingsWindow.ShowDialog() == true)
        {
            // 成功保存：更新配置
            Settings.CopyFrom(settingsVm.Settings);
            _configService.SaveConfig(Settings);
            UpdateTimerInterval();
        }
    }

    /// <summary>
    /// 保存窗口位置到配置文件
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    public void SaveWindowPosition(double left, double top)
    {
        Settings.WindowLeft = left;
        Settings.WindowTop = top;
        _configService.SaveConfig(Settings);
    }
}