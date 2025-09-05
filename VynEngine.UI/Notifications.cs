namespace VynEngine.UI;

/// <summary>
/// Shortcut static class to <see cref="NotificationService"/>.
/// </summary>
public static class Notifications
{
    /// <inheritdoc cref="NotificationService.Show(string?, string?, NotificationType, float?)"/>
    public static void Show(string? title, string? message, NotificationType type = NotificationType.Info, float? durationSeconds = 4f)
        => Application.Instance?.Notifications.Show(title, message, type, durationSeconds);
    
    /// <inheritdoc cref="NotificationService.Clear"/>
    public static void Clear() => Application.Instance?.Notifications.Clear();
}