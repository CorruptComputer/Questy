namespace Questy.NotificationPublishers;

/// <summary>
///   Uses Task.WhenAll with the list of Handler tasks:
///   <code>
///     Task[] tasks = [.. handlerExecutors.Select(handler => handler.HandlerCallback(notification, cancellationToken))];
///     return Task.WhenAll(tasks);
///   </code>
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        Task[] tasks = [.. handlerExecutors.Select(handler => handler.HandlerCallback(notification, cancellationToken))];
        return Task.WhenAll(tasks);
    }
}