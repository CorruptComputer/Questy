using System.Threading;

namespace Questy.Tests.Pipeline;

using System.Threading.Tasks;
using Questy.Pipeline;
using Shouldly;
using Lamar;
using Xunit;

public class RequestPostProcessorTests
{
    public class Ping : IRequest<Pong>
    {
        public string? Message { get; set; }
    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Pong { Message = request.Message + " Pong" });
        }
    }

    public class PingPongPostProcessor : IRequestPostProcessor<Ping, Pong>
    {
        public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
        {
            response.Message = response.Message + " " + request.Message;

            return Task.FromResult(0);
        }
    }

    [Fact]
    public async Task Should_run_postprocessors()
    {
        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator mediator = container.GetInstance<IMediator>();

        Pong response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong Ping");
    }

}