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

                // 旧版扁平格式检测：新版根节点是嵌套模块对象（Xau/Dom/...），
                // 不存在扁平键 ShowXau；存在即说明还是 v1.x 旧格式，迁移后立即写回新版
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ShowXau", out _))
                {
                    var legacy = JsonSerializer.Deserialize<LegacyAppSettings>(json);
                    var migrated = legacy != null ? ToModern(legacy) : new AppSettings();
                    migrated.AutoStart = IsAutoStartEnabled();
                    SaveConfig(migrated);
                    return migrated;
                }

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
    /// 旧版扁平配置 → 新版嵌套模块配置（一次性迁移逻辑）
    /// </summary>
    private static AppSettings ToModern(LegacyAppSettings legacy)
    {
        var s = new AppSettings
        {
            AutoStart = legacy.AutoStart,
            AutoHideOnFullScreen = legacy.AutoHideOnFullScreen,
            RefreshIntervalSeconds = legacy.RefreshIntervalSeconds,
            WindowLeft = legacy.WindowLeft,
            WindowTop = legacy.WindowTop,
            UiScale = legacy.UiScale,
            CapsuleBackground = legacy.CapsuleBackground ?? "#D918181A",
            CapsuleBorderColor = legacy.CapsuleBorderColor ?? "#25FFFFFF",
            IdleOpacity = legacy.IdleOpacity,
            HoverOpacity = legacy.HoverOpacity,
            FontFamily = legacy.FontFamily ?? "Microsoft YaHei UI",
            ShowDividers = legacy.ShowDividers,
            UpColor = legacy.UpColor ?? "#C07D00",
            DownColor = legacy.DownColor ?? "#4EAF50",
            FlatColor = legacy.FlatColor ?? "#8E8E93"
        };

        // 模块经 setter 赋值触发 OnXxxChanged，自动完成事件订阅
        s.Xau = new ModuleSettings
        {
            Show = legacy.ShowXau,
            ShowLabel = legacy.ShowXauLabel,
            LabelText = legacy.XauLabelText ?? "XAU",
            LabelColor = legacy.XauLabelColor ?? "#8E8E93",
            ShowPrice = legacy.ShowXauPrice,
            PriceDecimals = legacy.XauPriceDecimals,
            PriceColor = legacy.XauPriceColor ?? "#F2F2F7",
            ShowChangeRate = legacy.ShowXauChangeRate,
            ShowSign = legacy.ShowXauSign,
            ShowPercent = legacy.ShowXauPercent
        };

        s.Dom = new ModuleSettings
        {
            Show = legacy.ShowDom,
            ShowLabel = legacy.ShowDomLabel,
            LabelText = legacy.DomLabelText ?? "AU",
            LabelColor = legacy.DomLabelColor ?? "#8E8E93",
            ShowPrice = legacy.ShowDomPrice,
            PriceDecimals = legacy.DomPriceDecimals,
            PriceColor = legacy.DomPriceColor ?? "#F2F2F7",
            ShowChangeRate = legacy.ShowDomChangeRate,
            ShowSign = legacy.ShowDomSign,
            ShowPercent = legacy.ShowDomPercent
        };

        s.Autd = new ModuleSettings
        {
            Show = legacy.ShowAutd,
            ShowLabel = legacy.ShowAutdLabel,
            LabelText = legacy.AutdLabelText ?? "AuTD",
            LabelColor = legacy.AutdLabelColor ?? "#8E8E93",
            ShowPrice = legacy.ShowAutdPrice,
            PriceDecimals = legacy.AutdPriceDecimals,
            PriceColor = legacy.AutdPriceColor ?? "#F2F2F7",
            ShowChangeRate = legacy.ShowAutdChangeRate,
            ShowSign = legacy.ShowAutdSign,
            ShowPercent = legacy.ShowAutdPercent
        };

        s.Ms = new ModuleSettings
        {
            Show = legacy.ShowMs,
            ShowLabel = legacy.ShowMsLabel,
            LabelText = legacy.MsLabelText ?? "民生",
            LabelColor = legacy.MsLabelColor ?? "#8E8E93",
            ShowPrice = legacy.ShowMsPrice,
            PriceDecimals = legacy.MsPriceDecimals,
            PriceColor = legacy.MsPriceColor ?? "#F2F2F7",
            ShowChangeRate = legacy.ShowMsChangeRate,
            ShowSign = legacy.ShowMsSign,
            ShowPercent = legacy.ShowMsPercent
        };

        s.Zs = new ModuleSettings
        {
            Show = legacy.ShowZs,
            ShowLabel = legacy.ShowZsLabel,
            LabelText = legacy.ZsLabelText ?? "浙商",
            LabelColor = legacy.ZsLabelColor ?? "#8E8E93",
            ShowPrice = legacy.ShowZsPrice,
            PriceDecimals = legacy.ZsPriceDecimals,
            PriceColor = legacy.ZsPriceColor ?? "#F2F2F7",
            ShowChangeRate = legacy.ShowZsChangeRate,
            ShowSign = legacy.ShowZsSign,
            ShowPercent = legacy.ShowZsPercent
        };

        return s;
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