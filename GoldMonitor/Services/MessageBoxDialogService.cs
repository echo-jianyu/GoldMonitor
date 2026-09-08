using System.Windows;

namespace GoldMonitor.Services;

/// <summary>
/// <see cref="IUserDialogService"/> 的真实实现：直接弹 MessageBox。
/// </summary>
public class MessageBoxDialogService : IUserDialogService
{
    public bool Confirm(string message, string title)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void Alert(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
