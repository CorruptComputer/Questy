﻿using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Questy.Extensions.Microsoft.DependencyInjection.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pipeline;
using Shouldly;
using Xunit;

public class StreamPipelineTests
{
    public class OuterBehavior : IStreamPipelineBehavior<StreamPing, Pong>
    {
        private readonly Logger _output;

        public OuterBehavior(Logger output)
        {
            _output = output;
        }

        public async IAsyncEnumerable<Pong> Handle(StreamPing request, StreamHandlerDelegate<Pong> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _output.Messages.Add("Outer before");
            await foreach (Pong? response in next().WithCancellation(cancellationToken))
            {
                yield return response;
            }
            _output.Messages.Add("Outer after");
        }
    }

    public class InnerBehavior : IStreamPipelineBehavior<StreamPing, Pong>
    {
        private readonly Logger _output;

        public InnerBehavior(Logger output)
        {
            _output = output;
        }

        public async IAsyncEnumerable<Pong> Handle(StreamPing request, StreamHandlerDelegate<Pong> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _output.Messages.Add("Inner before");
            await foreach (Pong? response in next().WithCancellation(cancellationToken))
            {
                yield return response;
            }
            _output.Messages.Add("Inner after");
        }
    }

    [Fact]
    public async Task Should_wrap_with_behavior()
    {
        Logger output = new();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddTransient<IStreamPipelineBehavior<StreamPing, Pong>, OuterBehavior>();
        services.AddTransient<IStreamPipelineBehavior<StreamPing, Pong>, InnerBehavior>();
        services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        ServiceProvider provider = services.BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        IAsyncEnumerable<Pong> stream = mediator.CreateStream(new StreamPing { Message = "Ping" });

        await foreach (Pong? response in stream)
        {
            response.Message.ShouldBe("Ping Pang");
        }

        output.Messages.ShouldBe(new[]
        {
            "Outer before",
            "Inner before",
            "Handler",
            "Inner after",
            "Outer after"
        });
    }
   
    [Fact]
    public async Task Should_register_and_wrap_with_behavior()
    {
        Logger output = new();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
            cfg.AddStreamBehavior<IStreamPipelineBehavior<StreamPing, Pong>, OuterBehavior>();
            cfg.AddStreamBehavior<IStreamPipelineBehavior<StreamPing, Pong>, InnerBehavior>();
        });
        ServiceProvider provider = services.BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        IAsyncEnumerable<Pong> stream = mediator.CreateStream(new StreamPing { Message = "Ping" });

        await foreach (Pong? response in stream)
        {
            response.Message.ShouldBe("Ping Pang");
        }

        output.Messages.ShouldBe(new[]
        {
            "Outer before",
            "Inner before",
            "Handler",
            "Inner after",
            "Outer after"
        });
    }

}