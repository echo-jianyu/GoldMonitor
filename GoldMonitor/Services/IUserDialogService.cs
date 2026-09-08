namespace GoldMonitor.Services;

/// <summary>
/// 用户对话框抽象：隔离 MessageBox，使 ViewModel 可单元测试（测试注入假实现）。
/// 真实实现见 <see cref="MessageBoxDialogService"/>。
/// </summary>
public interface IUserDialogService
{
    /// <summary>
    /// 确认对话框（是/否），返回用户是否确认
    /// </summary>
    bool Confirm(string message, string title);

    /// <summary>
    /// 提示/警告对话框（仅确定）
    /// </summary>
    void Alert(string message, string title);
}
