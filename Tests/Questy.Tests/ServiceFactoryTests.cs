using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Questy.Tests;

public class ServiceFactoryTests
{
    public class Ping : IRequest<Pong>
    {

    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    [Fact]
    public async Task Should_throw_given_no_handler()
    {
        ServiceCollection serviceCollection = new();
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        Mediator mediator = new(serviceProvider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new Ping())
        );
    }
}