using Questy.Pipeline;

namespace Questy.Examples;

public class GenericRequestPostProcessor<TRequest, TResponse>(TextWriter writer) : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        return writer.WriteLineAsync("- All Done");
    }
}