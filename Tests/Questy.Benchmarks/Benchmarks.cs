using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Questy.Benchmarks
{
    [DotTraceDiagnoser]
    public class Benchmarks
    {
        private IMediator? _mediator;
        
        private readonly Ping _request = new() { Message = "Hello World" };
        private readonly Pinged _notification = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            ServiceCollection services = new();

            services.AddSingleton(TextWriter.Null);

            services.AddMediator(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining(typeof(Ping));
                cfg.AddOpenBehavior(typeof(GenericPipelineBehavior<,>));
            });

            ServiceProvider provider = services.BuildServiceProvider();

            _mediator = provider.GetRequiredService<IMediator>();
        }

        [Benchmark]
        public Task SendingRequests()
        {
            if (_mediator is null)
            {
                throw new InvalidOperationException("Mediator is not initialized.");
            }

            return _mediator.Send(_request);
        }

        [Benchmark]
        public Task PublishingNotifications()
        {
            if (_mediator is null)
            {
                throw new InvalidOperationException("Mediator is not initialized.");
            }

            return _mediator.Publish(_notification);
        }
    }
}
