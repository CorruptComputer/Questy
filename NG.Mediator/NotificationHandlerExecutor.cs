using System;
using System.Threading;
using System.Threading.Tasks;

namespace NG.Mediator;

public record NotificationHandlerExecutor(object HandlerInstance, Func<INotification, CancellationToken, Task> HandlerCallback);