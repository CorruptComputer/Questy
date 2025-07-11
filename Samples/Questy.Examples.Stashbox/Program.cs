using Stashbox;
using Stashbox.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Questy.Examples.Stashbox;

class Program
{
    static Task Main()
    {
        WrappingWriter writer = new(Console.Out);
        IMediator mediator = BuildMediator(writer);
        return Runner.Run(mediator, writer, "Stashbox", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        IStashboxContainer container = new StashboxContainer()
            .RegisterInstance<TextWriter>(writer)
            .RegisterAssemblies([typeof(Mediator).Assembly, typeof(Ping).Assembly], 
                serviceTypeSelector: Rules.ServiceRegistrationFilters.Interfaces, registerSelf: false);

        return container.GetRequiredService<IMediator>();
    }
}