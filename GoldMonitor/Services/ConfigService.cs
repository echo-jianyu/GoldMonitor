using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using GoldMonitor.Models;
using Microsoft.Win32;

namespace GoldMonitor.Services;

public class ConfigService
{
    private const string AppName = "GoldMonitor";
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string _configFilePath;

    public ConfigService()
    {
        string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        _configFilePath = Path.Combine(appDataFolder, "config.json");
    }

    /// <summary>
    /// 读取配置，不存在则创建默认配置
    /// </summary>
    public AppSettings LoadConfig()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                string json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<AppSettings>(json);
                if (config != null)
                {
                    // 同步一次开机自启真实状态
                    config.AutoStart = IsAutoStartEnabled();
                    return config;
                }
            }
        }
        catch
        {
            // 读取异常时回退到默认
        }

        var defaultConfig = new AppSettings();
        SaveConfig(defaultConfig);
        return defaultConfig;
    }

    /// <summary>
    /// 保存配置到本地JSON与注册表开机自启
    /// </summary>
    public void SaveConfig(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_configFilePath, json);

            // 更新开机自启动
            SetAutoStart(settings.AutoStart);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
        }
    }

    private bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    private void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key == null) return;

            string? exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath)) return;

            if (enable)
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set autostart: {ex.Message}");
        }
    }
}