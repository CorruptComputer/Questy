namespace Questy.Examples;

/// <summary>
///   Example of a generic pipeline behavior that writes to a TextWriter.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="writer"></param>
public class GenericPipelineBehavior<TRequest, TResponse>(TextWriter writer) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("-- Handling Request");
        TResponse? response = await next();
        await writer.WriteLineAsync("-- Finished Request");
        return response;
    }
}
