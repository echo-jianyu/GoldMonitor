using GoldMonitor.Models;

namespace GoldMonitor.Tests;

[TestClass]
public class AppSettingsTests
{
    [TestMethod]
    public void Clone_EditingCopyDoesNotAffectOriginal()
    {
        // Arrange：准备
        var original = new AppSettings();
        original.UiScale = 2.0;

        // Act：执行
        var copy = original.Clone();
        copy.UiScale = 1.0;
        copy.Xau.LabelText = "已修改";

        // Assert：断言
        Assert.AreEqual(2.0, original.UiScale);
        Assert.AreEqual("XAU", original.Xau.LabelText);
    }

    [TestMethod]
    public void ModulePropertyChanged_ForwardsWithModuleName()
    {
        // Arrange：订阅 PropertyChanged 收集属性名
        var settings = new AppSettings();
        var raised = new List<string?>();
        settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Act：修改模块内部属性
        settings.Xau.LabelText = "伦敦金";

        // Assert：以模块属性名（"Xau"）向上转发
        CollectionAssert.Contains(raised, nameof(AppSettings.Xau));
    }

    [TestMethod]
    public void ModuleAssignedNull_NormalizesToDefaultAndSubscribed()
    {
        // Arrange
        var settings = new AppSettings();
        var raised = new List<string?>();
        settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Act：赋 null → hook 归一化为该模块的默认实例
        settings.Xau = null!;

        // Assert：默认值正确（XAU 默认开启、标签 "XAU"）
        Assert.IsNotNull(settings.Xau);
        Assert.IsTrue(settings.Xau.Show);
        Assert.AreEqual("XAU", settings.Xau.LabelText);

        // Assert：归一化后的新实例事件已订阅，改动能被转发
        settings.Xau.ShowPrice = false;
        CollectionAssert.Contains(raised, nameof(AppSettings.Xau));
    }

    [TestMethod]
    public void ModuleInstanceReplaced_OldInstanceUnsubscribed()
    {
        // Arrange
        var settings = new AppSettings();
        var raised = new List<string?>();
        settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        var oldXau = settings.Xau;

        // Act：整体替换模块实例（替换本身会转发一次 "Xau"，先清空记录再验证解绑）
        settings.Xau = new ModuleSettings();
        raised.Clear();

        // Assert：旧实例已解绑——改旧实例不再转发
        oldXau.LabelText = "旧实例";
        Assert.AreEqual(0, raised.Count);

        // Assert：新实例已订阅——改新实例正常转发
        settings.Xau.LabelText = "新实例";
        CollectionAssert.Contains(raised, nameof(AppSettings.Xau));
    }
}
