using System.Collections.Generic;
using GoldMonitor.Models;
using GoldMonitor.Services;
using GoldMonitor.ViewModels;

namespace GoldMonitor.Tests.ViewModels;

[TestClass]
public class SettingsViewModelTests
{
    /// <summary>
    /// 测试用假对话框：记录调用，不弹真实 MessageBox
    /// </summary>
    private sealed class FakeDialogService : IUserDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public int ConfirmCallCount { get; private set; }
        public int AlertCallCount { get; private set; }
        public string? LastAlertMessage { get; private set; }

        public bool Confirm(string message, string title)
        {
            ConfirmCallCount++;
            return ConfirmResult;
        }

        public void Alert(string message, string title)
        {
            AlertCallCount++;
            LastAlertMessage = message;
        }
    }

    private FakeDialogService _dialog = null!;
    private SettingsViewModel _vm = null!;
    private bool _closeSuccessRaised;
    private bool _closeCancelRaised;

    [TestInitialize]
    public void Setup()
    {
        _dialog = new FakeDialogService();
        _closeSuccessRaised = false;
        _closeCancelRaised = false;
        _vm = CreateVm(new AppSettings());
    }

    /// <summary>
    /// 创建 VM 并挂接关闭事件标记
    /// </summary>
    private SettingsViewModel CreateVm(AppSettings settings, GoldPriceInfo? price = null)
    {
        var vm = new SettingsViewModel(settings, price, _dialog);
        vm.RequestCloseSuccess += () => _closeSuccessRaised = true;
        vm.RequestCloseCancel += () => _closeCancelRaised = true;
        return vm;
    }

    /// <summary>
    /// 把 5 个模块的所有显示内容全部关闭
    /// </summary>
    private static void TurnAllOff(AppSettings settings)
    {
        var modules = new[] { settings.Xau, settings.Dom, settings.Autd, settings.Ms, settings.Zs };
        foreach (var m in modules)
        {
            m.Show = false;
            m.ShowLabel = false;
            m.ShowPrice = false;
            m.ShowChangeRate = false;
        }
    }

    [TestMethod]
    public void Save_AllContentOff_AlertsAndStaysOpen()
    {
        // Arrange：所有模块的显示内容全部关闭
        TurnAllOff(_vm.Settings);

        // Act
        _vm.Save();

        // Assert：警告弹出一次，关闭事件未触发
        Assert.AreEqual(1, _dialog.AlertCallCount);
        Assert.IsNotNull(_dialog.LastAlertMessage);
        Assert.IsFalse(_closeSuccessRaised);
    }

    [TestMethod]
    public void Save_ValidConfig_ClosesAndClampsBounds()
    {
        // Arrange：各参数故意越界
        var s = _vm.Settings;
        s.RefreshIntervalSeconds = 99999;
        s.UiScale = 9.0;
        s.IdleOpacity = -1.0;
        s.HoverOpacity = 2.0;
        foreach (var m in new[] { s.Xau, s.Dom, s.Autd, s.Ms, s.Zs })
        {
            m.PriceDecimals = 9;
        }

        // Act
        _vm.Save();

        // Assert：关闭成功、无警告、所有越界值被钳制
        Assert.IsTrue(_closeSuccessRaised);
        Assert.AreEqual(0, _dialog.AlertCallCount);
        Assert.AreEqual(3600, s.RefreshIntervalSeconds);
        Assert.AreEqual(3.0, s.UiScale, 1e-9);
        Assert.AreEqual(0.05, s.IdleOpacity, 1e-9);
        Assert.AreEqual(1.0, s.HoverOpacity, 1e-9);
        foreach (var m in new[] { s.Xau, s.Dom, s.Autd, s.Ms, s.Zs })
        {
            Assert.AreEqual(2, m.PriceDecimals);
        }
    }

    [TestMethod]
    public void ResetToDefaults_Confirmed_ReplacesWithDefaultsAndRefreshesUnified()
    {
        // Arrange：先改乱几个值
        var original = _vm.Settings;
        original.UiScale = 2.0;
        original.Xau.ShowLabel = false;
        _dialog.ConfirmResult = true;

        // Act
        _vm.ResetToDefaults();

        // Assert：Settings 已换新实例且为默认值，Unified* 读的是新值
        Assert.IsFalse(ReferenceEquals(original, _vm.Settings));
        Assert.AreEqual(1.25, _vm.Settings.UiScale, 1e-9);
        Assert.IsTrue(_vm.UnifiedShowLabel);

        // Assert：新实例的事件已重挂——再改新实例，Unified* 依然联动
        _vm.Settings.Xau.ShowLabel = false;
        Assert.IsFalse(_vm.UnifiedShowLabel);
    }

    [TestMethod]
    public void ResetToDefaults_Cancelled_KeepsCurrentSettings()
    {
        // Arrange
        var original = _vm.Settings;
        original.UiScale = 2.0;
        _dialog.ConfirmResult = false;

        // Act
        _vm.ResetToDefaults();

        // Assert：同一实例、修改值保留
        Assert.IsTrue(ReferenceEquals(original, _vm.Settings));
        Assert.AreEqual(2.0, _vm.Settings.UiScale, 1e-9);
    }

    [TestMethod]
    public void Cancel_RaisesCancelEvent()
    {
        // Act
        _vm.Cancel();

        // Assert
        Assert.IsTrue(_closeCancelRaised);
    }

    [TestMethod]
    public void UnifiedProperties_SyncToAllFiveModules()
    {
        // Act
        _vm.UnifiedDecimals = 1;

        // Assert
        foreach (var m in new[] { _vm.Settings.Xau, _vm.Settings.Dom, _vm.Settings.Autd, _vm.Settings.Ms, _vm.Settings.Zs })
        {
            Assert.AreEqual(1, m.PriceDecimals);
        }
    }

    [TestMethod]
    public void ModulePropertyChanged_RefreshesUnifiedProperties()
    {
        // Arrange：收集 VM 抛出的属性名
        var raised = new List<string?>();
        _vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Act：修改某个模块的价格开关
        _vm.Settings.Xau.ShowPrice = false;

        // Assert：对应 Unified 属性被通知刷新
        CollectionAssert.Contains(raised, nameof(SettingsViewModel.UnifiedShowPrice));
    }

    [TestMethod]
    public void Ctor_NoPrice_UsesSimulatedPreview()
    {
        // Arrange：currentPrice 为 null
        var vm = new SettingsViewModel(new AppSettings(), null, _dialog);

        // Assert：使用拟真预览数据
        Assert.AreEqual(2938.69, vm.PreviewPrice.XauUsd, 1e-9);
    }

    [TestMethod]
    public void Ctor_ValidPrice_ReusesSameInstance()
    {
        // Arrange：currentPrice 有有效数据
        var price = new GoldPriceInfo { XauUsd = 4700.0 };

        // Act
        var vm = new SettingsViewModel(new AppSettings(), price, _dialog);

        // Assert：预览直接引用原实例，不拷贝
        Assert.IsTrue(ReferenceEquals(price, vm.PreviewPrice));
    }
}
