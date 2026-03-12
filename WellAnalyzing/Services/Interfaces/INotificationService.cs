namespace WellAnalyzing.Services.Interfaces;

/// <summary>
/// Предоставляет методы уведомлений.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Показывает сообщение.
    /// </summary>
    /// <remarks>
    /// Уровень сообщений выводится как <see cref="System.Windows.MessageBoxImage.Information"/>
    /// </remarks>
    /// <param name="message">Сообщение.</param>
    void ShowMessage(string message);

    /// <summary>
    /// Показывает описание ошибки.
    /// </summary>
    /// <remarks>
    /// Уровень сообщений выводится как <see cref="System.Windows.MessageBoxImage.Error"/>
    /// </remarks>
    /// <param name="error">Сообщение об ошибке.</param>
    void ShowError(string error);
}
