using System;
using System.Net.Http;
using System.Threading;
using System.Windows;
using GoldMonitor.Services;
using GoldMonitor.ViewModels;
using GoldMonitor.Views;

namespace GoldMonitor;

public partial class App : Application
{
    private static Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. 单实例检查（Mutex）
        const string mutexName = "Global\\GoldMonitor_SingleInstance_Mutex_9988";
        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            //MessageBox.Show("GoldMonitor 已经在运行中！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // 2. 初始化新浪行情 HttpClient
        var sinaHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        // sina反防盗链
        sinaHttpClient.DefaultRequestHeaders.Add("Referer", "https://finance.sina.com.cn");
        sinaHttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        // 禁用缓存，确保拿到最新行情
        sinaHttpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

        // 3. 京东积存金 HttpClient
        var jdHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        jdHttpClient.DefaultRequestHeaders.Add("Referer", "https://jdjr.jd.com/");
        jdHttpClient.DefaultRequestHeaders.Add("Origin", "https://jdjr.jd.com");
        jdHttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        jdHttpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

        base.OnStartup(e);

        // 4. 依赖组装与启动
        var configService = new ConfigService();
        var goldService = new CompositeGoldService(new SinaGoldService(sinaHttpClient), new JdGoldService(jdHttpClient));
        var mainViewModel = new MainViewModel(goldService, configService);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
        catch
        {
            // 忽略异常
        }
        base.OnExit(e);
    }
}