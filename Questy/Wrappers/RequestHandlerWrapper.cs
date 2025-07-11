using Microsoft.Extensions.DependencyInjection;

namespace Questy.Wrappers;

/// <summary>
///   Base class for request handler wrappers.
/// </summary>
public abstract class RequestHandlerBase
{
    /// <summary>
    ///    Handles the request by resolving the appropriate handler from the service provider
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
///   Base class for request handler wrappers that return a response.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    /// <inheritdoc />
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
///   Base class for request handler wrappers that do not return a response.
/// </summary>
public abstract class RequestHandlerWrapper : RequestHandlerBase
{
    /// <inheritdoc />
    public abstract Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
///   Implementation of <see cref="RequestHandlerWrapper{TResponse}"/> for a specific request type.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken) =>
        await Handle((IRequest<TResponse>)request, serviceProvider, cancellationToken).ConfigureAwait(false);

    /// <summary>
    ///   Handles the <typeparamref name="TRequest" /> by resolving the appropriate handler from the service provider,
    ///   and returns the <typeparamref name="TResponse" />.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Task<TResponse> Handler(CancellationToken t = default) => serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
            .Handle((TRequest)request, t == default ? cancellationToken : t);

        return serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>)Handler,
                (next, pipeline) => (t) => pipeline.Handle((TRequest)request, next, t == default ? cancellationToken : t))();
    }
}

/// <summary>
///   Implementation of <see cref="RequestHandlerWrapper"/> for a specific request type.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    /// <inheritdoc />
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken) =>
        await Handle((IRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);

    /// <summary>
    ///   Handles the <typeparamref name="TRequest" /> by resolving the appropriate handler from the service provider
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        async Task<Unit> Handler(CancellationToken t = default)
        {
            await serviceProvider.GetRequiredService<IRequestHandler<TRequest>>()
                .Handle((TRequest)request, t == default ? cancellationToken : t);

            return Unit.Value;
        }

        return serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<Unit>)Handler,
                (next, pipeline) => (t) => pipeline.Handle((TRequest)request, next, t == default ? cancellationToken : t))();
    }
}