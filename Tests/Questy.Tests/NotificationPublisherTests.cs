using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Questy.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Questy.Tests;

public class NotificationPublisherTests
{
    private readonly ITestOutputHelper _output;

    public NotificationPublisherTests(ITestOutputHelper output) => _output = output;

    public class Notification : INotification
    {
    }

    public class FirstHandler : INotificationHandler<Notification>
    {
        public async Task Handle(Notification notification, CancellationToken cancellationToken) 
            => await Task.Delay(500, cancellationToken);
    }
    public class SecondHandler : INotificationHandler<Notification>
    {
        public async Task Handle(Notification notification, CancellationToken cancellationToken) 
            => await Task.Delay(250, cancellationToken);
    }

    [Fact]
    public async Task Should_handle_sequentially_by_default()
    {
        ServiceCollection services = new();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Notification>();
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        Stopwatch timer = new();
        timer.Start();

        await mediator.Publish(new Notification());

        timer.Stop();

        long sequentialElapsed = timer.ElapsedMilliseconds;

        services = new ServiceCollection();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Notification>();
            cfg.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });
        serviceProvider = services.BuildServiceProvider();

        mediator = serviceProvider.GetRequiredService<IMediator>();

        timer.Restart();

        await mediator.Publish(new Notification());

        timer.Stop();

        long parallelElapsed = timer.ElapsedMilliseconds;

        sequentialElapsed.ShouldBeGreaterThan(parallelElapsed);
    }
}