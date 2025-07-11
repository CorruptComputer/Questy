using Microsoft.Extensions.DependencyInjection;
using Questy.Examples.Streams;
using Questy.Pipeline;

namespace Questy.Examples.AspNetCore;

public static class Program
{
    public static Task Main()
    {
        WrappingWriter writer = new(Console.Out);
        IMediator mediator = BuildMediator(writer);
        return Runner.Run(mediator, writer, "ASP.NET Core DI", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        ServiceCollection services = new();

        services.AddSingleton<TextWriter>(writer);

        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Sing).Assembly);
        });

        services.AddScoped(typeof(IStreamRequestHandler<Sing, Song>), typeof(SingHandler));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
        services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
        services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));
        services.AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamPipelineBehavior<,>));

        ServiceProvider provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IMediator>();
    }
}