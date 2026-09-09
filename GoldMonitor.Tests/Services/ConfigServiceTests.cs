using System;
using System.IO;
using GoldMonitor.Models;
using GoldMonitor.Services;

namespace GoldMonitor.Tests.Services;

[TestClass]
public class ConfigServiceTests
{
    /// <summary>
    /// 测试用 ConfigService 子类：覆盖注册表读写，屏蔽真实副作用并记录调用
    /// </summary>
    private sealed class ConfigServiceStub : ConfigService
    {
        public bool FakeAutoStart { get; set; } = true;
        public int SetAutoStartCallCount { get; private set; }
        public bool? LastAutoStartValue { get; private set; }

        public ConfigServiceStub(string path) : base(path) { }

        protected override bool IsAutoStartEnabled() => FakeAutoStart;

        protected override void SetAutoStart(bool enable)
        {
            SetAutoStartCallCount++;
            LastAutoStartValue = enable;
        }
    }

    /// <summary>
    /// 旧版扁平格式样例：含 null 字段（迁移后应落到默认值）
    /// </summary>
    private const string LegacyJson = """
{
  "AutoStart": true, "AutoHideOnFullScreen": false, "RefreshIntervalSeconds": 30,
  "WindowLeft": 100.5, "WindowTop": 200.5, "UiScale": 1.5,
  "CapsuleBackground": "#FF000000", "CapsuleBorderColor": null,
  "IdleOpacity": 0.3, "HoverOpacity": 0.8, "FontFamily": "SimSun",
  "ShowDividers": false, "UpColor": "#E53935", "DownColor": null, "FlatColor": "#8E8E93",
  "ShowXau": true, "ShowXauLabel": false, "XauLabelText": "金价", "XauLabelColor": null,
  "ShowXauPrice": true, "XauPriceDecimals": 1, "XauPriceColor": "#F2F2F7",
  "ShowXauChangeRate": true, "ShowXauSign": false, "ShowXauPercent": true,
  "ShowDom": true, "ShowDomLabel": true, "DomLabelText": "沪金", "DomLabelColor": null,
  "ShowDomPrice": true, "DomPriceDecimals": 2, "DomPriceColor": "#F2F2F7",
  "ShowDomChangeRate": false, "ShowDomSign": true, "ShowDomPercent": false,
  "ShowAutd": false, "ShowAutdLabel": true, "AutdLabelText": "AuTD", "AutdLabelColor": null,
  "ShowAutdPrice": true, "AutdPriceDecimals": 0, "AutdPriceColor": "#F2F2F7",
  "ShowAutdChangeRate": true, "ShowAutdSign": true, "ShowAutdPercent": true,
  "ShowMs": true, "ShowMsLabel": false, "MsLabelText": null, "MsLabelColor": null,
  "ShowMsPrice": true, "MsPriceDecimals": 1, "MsPriceColor": "#F2F2F7",
  "ShowMsChangeRate": true, "ShowMsSign": false, "ShowMsPercent": true,
  "ShowZs": false, "ShowZsLabel": true, "ZsLabelText": null, "ZsLabelColor": "#123456",
  "ShowZsPrice": false, "ZsPriceDecimals": 2, "ZsPriceColor": null,
  "ShowZsChangeRate": false, "ShowZsSign": false, "ShowZsPercent": false
}
""";

    private string _configPath = null!;
    private string _tempDir = null!;

    [TestInitialize]
    public void Setup()
    {
        // 每个测试使用独立的临时目录，互不干扰
        _tempDir = Path.Combine(Path.GetTempPath(), "GoldMonitor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "config.json");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [TestMethod]
    public void LoadConfig_LegacyFormat_MigratesAllFields()
    {
        // Arrange：写入旧版扁平格式
        File.WriteAllText(_configPath, LegacyJson);
        var stub = new ConfigServiceStub(_configPath);

        // Act
        var s = stub.LoadConfig();

        // Assert：通用字段（null 字段落到默认值）
        Assert.AreEqual(stub.FakeAutoStart, s.AutoStart);  // LoadConfig 用注册表真实状态覆盖
        Assert.IsFalse(s.AutoHideOnFullScreen);
        Assert.AreEqual(30, s.RefreshIntervalSeconds);
        Assert.AreEqual(100.5, s.WindowLeft!.Value, 1e-9);
        Assert.AreEqual(200.5, s.WindowTop!.Value, 1e-9);
        Assert.AreEqual(1.5, s.UiScale, 1e-9);
        Assert.AreEqual("#FF000000", s.CapsuleBackground);
        Assert.AreEqual("#25FFFFFF", s.CapsuleBorderColor);  // null → 默认
        Assert.AreEqual(0.3, s.IdleOpacity, 1e-9);
        Assert.AreEqual(0.8, s.HoverOpacity, 1e-9);
        Assert.AreEqual("SimSun", s.FontFamily);
        Assert.IsFalse(s.ShowDividers);
        Assert.AreEqual("#E53935", s.UpColor);
        Assert.AreEqual("#4EAF50", s.DownColor);  // null → 默认
        Assert.AreEqual("#8E8E93", s.FlatColor);

        // Assert：XAU 模块
        Assert.IsTrue(s.Xau.Show);
        Assert.IsFalse(s.Xau.ShowLabel);
        Assert.AreEqual("金价", s.Xau.LabelText);
        Assert.AreEqual("#8E8E93", s.Xau.LabelColor);  // null → 默认
        Assert.IsTrue(s.Xau.ShowPrice);
        Assert.AreEqual(1, s.Xau.PriceDecimals);
        Assert.AreEqual("#F2F2F7", s.Xau.PriceColor);
        Assert.IsTrue(s.Xau.ShowChangeRate);
        Assert.IsFalse(s.Xau.ShowSign);
        Assert.IsTrue(s.Xau.ShowPercent);

        // Assert：其余模块抽查关键差异项
        Assert.IsTrue(s.Dom.Show);
        Assert.AreEqual("沪金", s.Dom.LabelText);
        Assert.IsFalse(s.Dom.ShowChangeRate);
        Assert.IsFalse(s.Dom.ShowPercent);
        Assert.IsFalse(s.Autd.Show);
        Assert.AreEqual(0, s.Autd.PriceDecimals);
        Assert.IsTrue(s.Ms.Show);
        Assert.AreEqual("民生", s.Ms.LabelText);  // null → 默认
        Assert.IsFalse(s.Ms.ShowLabel);
        Assert.IsFalse(s.Zs.Show);
        Assert.AreEqual("浙商", s.Zs.LabelText);  // null → 默认
        Assert.AreEqual("#123456", s.Zs.LabelColor);
        Assert.IsFalse(s.Zs.ShowPrice);
        Assert.AreEqual("#F2F2F7", s.Zs.PriceColor);  // null → 默认

        // Assert：迁移后文件已重写为新嵌套格式
        string rewritten = File.ReadAllText(_configPath);
        Assert.IsFalse(rewritten.Contains("ShowXau"), "迁移后不应残留旧扁平键");
        Assert.IsTrue(rewritten.Contains("\"Xau\""), "迁移后应包含新嵌套键");
    }

    [TestMethod]
    public void LoadConfig_RoundTrip_ValuesEqual()
    {
        // Arrange：构造一份修改过的配置并保存
        var stub = new ConfigServiceStub(_configPath);
        var original = new AppSettings
        {
            UiScale = 1.8,
            RefreshIntervalSeconds = 20,
            UpColor = "#E53935"
        };
        original.Xau.LabelText = "伦敦金";
        original.Ms.PriceDecimals = 1;
        original.Zs.Show = false;
        stub.SaveConfig(original);

        // Act：用新的实例重新加载
        var loaded = new ConfigServiceStub(_configPath).LoadConfig();

        // Assert：关键字段与模块字段一致
        Assert.AreEqual(1.8, loaded.UiScale, 1e-9);
        Assert.AreEqual(20, loaded.RefreshIntervalSeconds);
        Assert.AreEqual("#E53935", loaded.UpColor);
        Assert.AreEqual("伦敦金", loaded.Xau.LabelText);
        Assert.AreEqual(1, loaded.Ms.PriceDecimals);
        Assert.IsFalse(loaded.Zs.Show);
        Assert.AreEqual(stub.FakeAutoStart, loaded.AutoStart);
    }

    [TestMethod]
    public void LoadConfig_NoFile_ReturnsDefaultAndCreatesFile()
    {
        // Arrange
        var stub = new ConfigServiceStub(_configPath);

        // Act
        var s = stub.LoadConfig();

        // Assert：默认值 + 文件被创建 + 开机自启被写入一次
        Assert.AreEqual(1.25, s.UiScale, 1e-9);
        Assert.AreEqual(stub.FakeAutoStart, s.AutoStart);
        Assert.IsTrue(File.Exists(_configPath));
        Assert.AreEqual(1, stub.SetAutoStartCallCount);
        Assert.AreEqual(stub.FakeAutoStart, stub.LastAutoStartValue);
    }

    [TestMethod]
    public void LoadConfig_CorruptJson_FallsBackToDefault()
    {
        // Arrange
        File.WriteAllText(_configPath, "这不是JSON{");
        var stub = new ConfigServiceStub(_configPath);

        // Act：解析失败应静默回退默认配置
        var s = stub.LoadConfig();
        var defaultSettings = new AppSettings();

        // Assert
        Assert.AreEqual(defaultSettings.UiScale, s.UiScale, 1e-9);
        Assert.AreEqual(defaultSettings.RefreshIntervalSeconds, s.RefreshIntervalSeconds);
        Assert.AreEqual(defaultSettings.UpColor, s.UpColor);
        Assert.AreEqual(defaultSettings.DownColor, s.DownColor);
        Assert.AreEqual(defaultSettings.FlatColor, s.FlatColor);
    }
}
