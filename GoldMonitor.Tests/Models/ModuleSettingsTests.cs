using GoldMonitor.Models;

namespace GoldMonitor.Tests.Models;

[TestClass]
public class ModuleSettingsTests
{
    [TestMethod]
    public void Clone_ShallowCopyIsolated()
    {
        // Arrange
        var original = new ModuleSettings { Show = true, LabelText = "XAU" };

        // Act
        var copy = original.Clone();
        copy.Show = false;
        copy.LabelText = "已修改";

        // Assert：不同实例，修改副本不影响原对象
        Assert.IsFalse(ReferenceEquals(original, copy));
        Assert.IsTrue(original.Show);
        Assert.AreEqual("XAU", original.LabelText);
        Assert.IsFalse(copy.Show);
        Assert.AreEqual("已修改", copy.LabelText);
    }
}
