using System;
using System.Threading;
using System.Threading.Tasks;

namespace Questy;

public record NotificationHandlerExecutor(object HandlerInstance, Func<INotification, CancellationToken, Task> HandlerCallback);