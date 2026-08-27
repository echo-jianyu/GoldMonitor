using System;
using System.Runtime.InteropServices;

namespace GoldMonitor.Native;

public static class NativeMethods
{
    private const string User32 = "user32.dll";

    // 32 位系统的 API 声明
    [DllImport(User32, EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport(User32, EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    // 64 位系统的 API 声明
    [DllImport(User32, EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport(User32, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // 刷新窗口缓存与位置样式的 API
    [DllImport(User32, SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    /// <summary>
    /// 自动适配 32 位 / 64 位的 GetWindowLongPtr 实现
    /// </summary>
    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
        {
            return GetWindowLongPtr64(hWnd, nIndex);
        }
        else
        {
            return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }
    }

    /// <summary>
    /// 自动适配 32 位 / 64 位的 SetWindowLongPtr 实现
    /// </summary>
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }
        else
        {
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }
    }
}