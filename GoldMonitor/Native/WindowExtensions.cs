using System;
using System.Windows;
using System.Windows.Interop;

namespace GoldMonitor.Native;

public static class WindowExtensions
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;   // 不在 Alt+Tab 中显示
    private const int WS_EX_TRANSPARENT = 0x00000020;  // 鼠标点击穿透

    // SetWindowPos 相关控制标记
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020; // 强制系统重绘边框并刷新窗口缓存

    // 组合标记：仅刷新样式缓存，不改变窗口尺寸、位置和 Z 序
    private const uint RefreshFlags = SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED;

    /// <summary>
    /// 将窗口样式设为 ToolWindow，彻底从 Alt+Tab 任务切换栏中隐藏（兼容 x86 / x64）
    /// </summary>
    public static void HideFromAltTab(this Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        long currentStyle = NativeMethods.GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt64();
        long newStyle = currentStyle | WS_EX_TOOLWINDOW;

        NativeMethods.SetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(newStyle));

        // 通知系统刷新缓存，让 Alt+Tab 隐藏立即生效
        NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, RefreshFlags);
    }

    /// <summary>
    /// 开启/关闭鼠标点击穿透（兼容 x86 / x64）
    /// </summary>
    public static void SetClickThrough(this Window window, bool enable)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        long currentStyle = NativeMethods.GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt64();
        long newStyle = enable ? (currentStyle | WS_EX_TRANSPARENT) : (currentStyle & ~WS_EX_TRANSPARENT);

        NativeMethods.SetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(newStyle));

        // 通知系统刷新缓存，使鼠标穿透状态立即生效
        NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, RefreshFlags);
    }
}