using System.Runtime.CompilerServices;

namespace Questy.Examples.Streams;

public class SingHandler : IStreamRequestHandler<Sing, Song>
{
    private readonly TextWriter _writer;

    public SingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Sing: {request.Message}, Song");
        yield return await Task.Run(() => new Song { Message = request.Message + "ing do" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing re" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing mi" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing fa" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing so" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing la" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing ti" });
        yield return await Task.Run(() => new Song { Message = request.Message + "ing do" });
    }
}