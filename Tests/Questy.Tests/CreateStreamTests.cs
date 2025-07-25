using System.Threading;

namespace Questy.Tests;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Shouldly;
using Lamar;
using Xunit;

public class CreateStreamTests
{

    public class Ping : IStreamRequest<Pong>
    {
        public string? Message { get; set; }
    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    public class PingStreamHandler : IStreamRequestHandler<Ping, Pong>
    {
        public async IAsyncEnumerable<Pong> Handle(Ping request, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            yield return await Task.Run(() => new Pong { Message = request.Message + " Pang" });
        }
    }

    [Fact]
    public async Task Should_resolve_main_handler()
    {
        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CreateStreamTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator mediator = container.GetInstance<IMediator>();

        IAsyncEnumerable<Pong> response = mediator.CreateStream(new Ping { Message = "Ping" });
        int i = 0;
        await foreach (Pong result in response)
        {
            if (i == 0)
            {
                result.Message.ShouldBe("Ping Pang");
            }

            i++;
        }

        i.ShouldBe(1);
    }

    [Fact]
    public async Task Should_resolve_main_handler_via_dynamic_dispatch()
    {
        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CreateStreamTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator mediator = container.GetInstance<IMediator>();

        object request = new Ping { Message = "Ping" };
        IAsyncEnumerable<object?> response = mediator.CreateStream(request);
        int i = 0;
        await foreach (Pong? result in response)
        {
            if (i == 0)
            {
                result!.Message.ShouldBe("Ping Pang");
            }

            i++;
        }

        i.ShouldBe(1);
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_specific_interface()
    {
        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CreateStreamTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
            });
            cfg.For<ISender>().Use<Mediator>();
        });

        ISender mediator = container.GetInstance<ISender>();
        IAsyncEnumerable<Pong> response = mediator.CreateStream(new Ping { Message = "Ping" });
        int i = 0;
        await foreach (Pong result in response)
        {
            if (i == 0)
            {
                result.Message.ShouldBe("Ping Pang");
            }

            i++;
        }

        i.ShouldBe(1);
    }

    [Fact]
    public void Should_raise_execption_on_null_request()
    {
        Container container = new(cfg =>
        {
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator mediator = container.GetInstance<IMediator>();

        Should.Throw<ArgumentNullException>(() => mediator.CreateStream((Ping) null!));
    }

    [Fact]
    public void Should_raise_execption_on_null_request_via_dynamic_dispatch()
    {
        Container container = new(cfg =>
        {
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator mediator = container.GetInstance<IMediator>();

        Should.Throw<ArgumentNullException>(() => mediator.CreateStream((object) null!));
    }
}