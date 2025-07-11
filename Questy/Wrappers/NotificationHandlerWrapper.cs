using Microsoft.Extensions.DependencyInjection;

namespace Questy.Wrappers;

/// <summary>
///   Base class for notification handler wrappers.
/// </summary>
public abstract class NotificationHandlerWrapper
{
    /// <summary>
    ///   Handles the notification by resolving the appropriate handlers from the service factory
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="serviceFactory"></param>
    /// <param name="publish"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

/// <summary>
///   Implementation of <see cref="NotificationHandlerWrapper"/> for a specific notification type.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    /// <inheritdoc />
    public override Task Handle(INotification notification, IServiceProvider serviceFactory,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        IEnumerable<NotificationHandlerExecutor> handlers = serviceFactory
            .GetServices<INotificationHandler<TNotification>>()
            .Select(static x => new NotificationHandlerExecutor(x, (theNotification, theToken) => x.Handle((TNotification)theNotification, theToken)));

        return publish(handlers, notification, cancellationToken);
    }
}