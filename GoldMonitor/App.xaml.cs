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
    public static HttpClient HttpClient { get; private set; } = null!;

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

        // 2. 初始化全局 HttpClient
        var handler = new HttpClientHandler();
        HttpClient = new HttpClient(handler);
        // sina反防盗链
        HttpClient.DefaultRequestHeaders.Add("Referer", "https://finance.sina.com.cn");
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        base.OnStartup(e);

        // 3. 依赖组装与启动
        var configService = new ConfigService();
        var goldService = new SinaGoldService(HttpClient);
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