using System.Windows;
using WellAnalyzing.Services.Interfaces;

namespace WellAnalyzing.Services;

/// <inheritdoc />
public sealed class NotificationService : INotificationService
{
    /// <inheritdoc />
    public void ShowMessage(string message)
    {
        MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <inheritdoc />
    public void ShowError(string error)
    {
        MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
