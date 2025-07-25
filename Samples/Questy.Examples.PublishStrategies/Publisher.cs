namespace Questy.Examples.PublishStrategies;

public class Publisher
{
    private readonly IServiceProvider _serviceFactory;

    public Publisher(IServiceProvider serviceFactory)
    {
        _serviceFactory = serviceFactory;

        PublishStrategies[PublishStrategy.Async] = new CustomMediator(_serviceFactory, AsyncContinueOnException);
        PublishStrategies[PublishStrategy.ParallelNoWait] = new CustomMediator(_serviceFactory, ParallelNoWait);
        PublishStrategies[PublishStrategy.ParallelWhenAll] = new CustomMediator(_serviceFactory, ParallelWhenAll);
        PublishStrategies[PublishStrategy.ParallelWhenAny] = new CustomMediator(_serviceFactory, ParallelWhenAny);
        PublishStrategies[PublishStrategy.SyncContinueOnException] = new CustomMediator(_serviceFactory, SyncContinueOnException);
        PublishStrategies[PublishStrategy.SyncStopOnException] = new CustomMediator(_serviceFactory, SyncStopOnException);
    }

    public IDictionary<PublishStrategy, IMediator> PublishStrategies = new Dictionary<PublishStrategy, IMediator>();
    public PublishStrategy DefaultStrategy { get; set; } = PublishStrategy.SyncContinueOnException;

    public Task Publish<TNotification>(TNotification notification)
    {
        return Publish(notification, DefaultStrategy, default(CancellationToken));
    }

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy)
    {
        return Publish(notification, strategy, default(CancellationToken));
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
    {
        return Publish(notification, DefaultStrategy, cancellationToken);
    }

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
    {
        if (!PublishStrategies.TryGetValue(strategy, out IMediator? mediator))
        {
            throw new ArgumentException($"Unknown strategy: {strategy}");
        }
        
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification), "Notification cannot be null.");
        }

        return mediator.Publish(notification, cancellationToken);
    }

    private Task ParallelWhenAll(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Task> tasks = new();

        foreach (NotificationHandlerExecutor handler in handlers)
        {
            tasks.Add(Task.Run(() => handler.HandlerCallback(notification, cancellationToken)));
        }

        return Task.WhenAll(tasks);
    }

    private Task ParallelWhenAny(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Task> tasks = new();

        foreach (NotificationHandlerExecutor handler in handlers)
        {
            tasks.Add(Task.Run(() => handler.HandlerCallback(notification, cancellationToken)));
        }

        return Task.WhenAny(tasks);
    }

    private Task ParallelNoWait(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (NotificationHandlerExecutor handler in handlers)
        {
            Task.Run(() => handler.HandlerCallback(notification, cancellationToken));
        }

        return Task.CompletedTask;
    }

    private async Task AsyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Task> tasks = new();
        List<Exception> exceptions = new();

        foreach (NotificationHandlerExecutor handler in handlers)
        {
            try
            {
                tasks.Add(handler.HandlerCallback(notification, cancellationToken));
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (AggregateException ex)
        {
            exceptions.AddRange(ex.Flatten().InnerExceptions);
        }
        catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
        {
            exceptions.Add(ex);
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task SyncStopOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (NotificationHandlerExecutor handler in handlers)
        {
            await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        List<Exception> exceptions = new();

        foreach (NotificationHandlerExecutor handler in handlers)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }
}