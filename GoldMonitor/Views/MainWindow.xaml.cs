using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GoldMonitor.Native;
using GoldMonitor.ViewModels;

namespace GoldMonitor.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isMouseOver = false;

    // 用于检测全屏状态的定时器 (5秒检测一次)
    private DispatcherTimer? _fullScreenCheckTimer;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            this.HideFromAltTab(); // 从 Alt+Tab 中隐藏
            RestorePosition();     // 恢复记忆坐标
            ApplyInitialOpacity(); // 应用初始潜伏透明度

            // 启动全屏检测定时器
            _fullScreenCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // 每 5 秒轮询一次全屏状态
            };
            _fullScreenCheckTimer.Tick += (_, _) => CheckFullScreenState();
            _fullScreenCheckTimer.Start();
        };
    }

    // 核心全屏检测方法
    private void CheckFullScreenState()
    {
        if (ViewModel?.Settings == null) return;

        // 如果用户在设置里勾选了“全屏时自动隐藏”
        if (ViewModel.Settings.AutoHideOnFullScreen)
        {
            bool isFullScreen = FullScreenHelper.IsFullScreenAppRunning();
            var targetVisibility = isFullScreen ? Visibility.Collapsed : Visibility.Visible;

            // 状态变更时才切换，避免无效渲染
            if (Visibility != targetVisibility)
            {
                Visibility = targetVisibility;
            }
        }
        else
        {
            // 未开启该功能，确保窗口处于正常显示状态
            if (Visibility != Visibility.Visible)
            {
                Visibility = Visibility.Visible;
            }
        }
    }

    private void RestorePosition()
    {
        double left = ViewModel.Settings.WindowLeft ?? (SystemParameters.WorkArea.Right - 260);
        double top = ViewModel.Settings.WindowTop ?? (SystemParameters.WorkArea.Bottom - 60);

        // 校验位置是否超出屏幕可见区域
        if (left < SystemParameters.VirtualScreenLeft || left > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 50)
            left = SystemParameters.WorkArea.Right - 260;

        if (top < SystemParameters.VirtualScreenTop || top > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 30)
            top = SystemParameters.WorkArea.Bottom - 60;

        Left = left;
        Top = top;
    }

    private void ApplyInitialOpacity()
    {
        Opacity = ViewModel.Settings.IdleOpacity;
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        _isMouseOver = true;
        AnimateOpacity(ViewModel.Settings.HoverOpacity, TimeSpan.FromMilliseconds(120));
    }

    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        _isMouseOver = false;
        AnimateOpacity(ViewModel.Settings.IdleOpacity, TimeSpan.FromMilliseconds(300));
    }

    private void AnimateOpacity(double toValue, TimeSpan duration)
    {
        var anim = new DoubleAnimation
        {
            To = toValue,
            Duration = duration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(OpacityProperty, anim);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
                // 拖拽结束（鼠标释放后 DragMove 返回），一次性写入磁盘
                ViewModel.SaveWindowPosition(Left, Top);
            }
            catch
            {
            }
        }
    }

    private void Window_LocationChanged(object? sender, EventArgs e)
    {
        // 仅更新内存中的位置值，不触发磁盘写入（拖拽过程中每像素都会触发本事件）
        if (IsLoaded)
        {
            ViewModel.Settings.WindowLeft = Left;
            ViewModel.Settings.WindowTop = Top;
        }
    }

    private void MenuSettings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenSettings();

        // 设置保存返回后：
        // 1. 立即检测一次全屏状态（即时生效）
        CheckFullScreenState();

        // 2. 重新应用当前鼠标悬浮/潜伏透明度
        BeginAnimation(OpacityProperty, null); // 移除当前动画时钟
        Opacity = _isMouseOver ? ViewModel.Settings.HoverOpacity : ViewModel.Settings.IdleOpacity;

        // 3. 刷新胶囊 UI（字体、配色、缩放等）
        MainCapsule.UpdateVisuals();
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        _fullScreenCheckTimer?.Stop();
        Application.Current.Shutdown();
    }
}