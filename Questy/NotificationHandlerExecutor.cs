namespace Questy;

/// <summary>
///   Represents a notification handler executor.
/// </summary>
/// <param name="HandlerInstance">The instance of the notification handler.</param>
/// <param name="HandlerCallback">The callback to invoke for handling notifications.</param>
public record NotificationHandlerExecutor(object HandlerInstance, Func<INotification, CancellationToken, Task> HandlerCallback);