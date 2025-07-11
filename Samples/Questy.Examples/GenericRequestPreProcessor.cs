using Questy.Pipeline;

namespace Questy.Examples;

public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : IRequest
{
    private readonly TextWriter _writer;

    public GenericRequestPreProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        return _writer.WriteLineAsync("- Starting Up");
    }
}