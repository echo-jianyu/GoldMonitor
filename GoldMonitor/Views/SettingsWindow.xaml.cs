using CommunityToolkit.Mvvm.Input;
using GoldMonitor.ViewModels;
using System.Diagnostics;
using System.Windows;
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