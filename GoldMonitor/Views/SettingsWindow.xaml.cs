using CommunityToolkit.Mvvm.Input;
using GoldMonitor.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace GoldMonitor.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.RequestCloseSuccess += () =>
        {
            DialogResult = true;
            Close();
        };

        vm.RequestCloseCancel += () =>
        {
            DialogResult = false;
            Close();
        };
    }

    /// <summary>
    /// 侧边导航切换设置页，附带淡入过渡动画
    /// </summary>
    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // XAML 解析期 SelectedIndex=0 触发本事件时页面元素尚未全部创建，直接跳过
        if (PageGeneral == null || PageModules == null || PageColors == null || PageAbout == null)
            return;

        var pages = new[] { PageGeneral, PageModules, PageColors, PageAbout };
        int target = NavList.SelectedIndex;

        // Ctrl+点击可取消当前选中（SelectedIndex 变为 -1），此时保持现状不做切换
        if (target < 0 || target >= pages.Length)
            return;

        for (int i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            if (i == target)
            {
                page.Visibility = Visibility.Visible;
                // 淡入动画：0 → 1，120ms 缓出
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(120),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                page.BeginAnimation(OpacityProperty, fadeIn);
            }
            else
            {
                page.Visibility = Visibility.Collapsed;
                page.BeginAnimation(OpacityProperty, null); // 清除残留动画
                page.Opacity = 1;
            }
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        catch
        {
            // 忽略打开浏览器异常
        }
    }
}
