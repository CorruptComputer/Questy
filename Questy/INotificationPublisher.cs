namespace Questy;

/// <summary>
///   Interface for custom notifications publish strategies.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    ///   Publishes a notification to the appropriate handlers.
    /// </summary>
    /// <param name="handlerExecutors"></param>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken);
}