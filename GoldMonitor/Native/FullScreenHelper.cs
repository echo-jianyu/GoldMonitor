using System;
using System.Runtime.InteropServices;

namespace GoldMonitor.Native;

public static class FullScreenHelper
{
    private enum QUERY_USER_NOTIFICATION_STATE
    {
        QUNS_NOT_PRESENT = 1,
        QUNS_BUSY = 2,                      // 全屏独占 / 忙碌
        QUNS_RUNNING_D3D_FULL_SCREEN = 3,  // 正在运行全屏 3D 游戏 / D3D 应用
        QUNS_PRESENTATION_MODE = 4,        // PPT 演示全屏
        QUNS_ACCEPTS_NOTIFICATIONS = 5,    // 正常桌面状态
        QUNS_QUIET_TIME = 6,
        QUNS_APP = 7                       // Win10/11 全屏应用
    }

    private const int MONITOR_DEFAULTTONULL = 0;
    private const int GWL_STYLE = -16;
    private const long WS_CAPTION = 0x00C00000L;

    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out QUERY_USER_NOTIFICATION_STATE pquns);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    /// <summary>
    /// 检测当前是否有全屏游戏、全屏播放视频或PPT全屏
    /// </summary>
    public static bool IsFullScreenAppRunning()
    {
        // 1. 系统级全屏状态检测（精准识别独占全屏游戏、PPT 等）
        if (SHQueryUserNotificationState(out var state) == 0)
        {
            if (state == QUERY_USER_NOTIFICATION_STATE.QUNS_RUNNING_D3D_FULL_SCREEN
                || state == QUERY_USER_NOTIFICATION_STATE.QUNS_PRESENTATION_MODE
                || state == QUERY_USER_NOTIFICATION_STATE.QUNS_BUSY)
            {
                return true;
            }
        }

        // 2. 检测前台窗口（识别浏览器 F11 全屏、视频播放器网页全屏等无边框全屏）
        IntPtr fgWnd = GetForegroundWindow();
        if (fgWnd == IntPtr.Zero || fgWnd == GetDesktopWindow() || fgWnd == GetShellWindow())
            return false;

        // 获取前台窗口所在的物理显示器信息
        IntPtr hMonitor = MonitorFromWindow(fgWnd, MONITOR_DEFAULTTONULL);
        if (hMonitor == IntPtr.Zero)
            return false;

        var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(hMonitor, ref mi))
            return false;

        if (GetWindowRect(fgWnd, out RECT rect))
        {
            // 判断窗口外轮廓是否覆盖了整个显示器屏幕
            bool coversMonitor = rect.Left <= mi.rcMonitor.Left &&
                                 rect.Top <= mi.rcMonitor.Top &&
                                 rect.Right >= mi.rcMonitor.Right &&
                                 rect.Bottom >= mi.rcMonitor.Bottom;

            if (coversMonitor)
            {
                // 排除带有普通标题栏（WS_CAPTION）的普通最大化窗口（如常规最大化的文件资源管理器/浏览器）
                long style = GetWindowLongPtr(fgWnd, GWL_STYLE).ToInt64();
                if ((style & WS_CAPTION) == WS_CAPTION)
                {
                    return false;
                }

                return true;
            }
        }

        return false;
    }
}