using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using PollySimulator.SimulatorHandler;

namespace PollySimulator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            Configure(services);

            var syncContext = new LocalSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            var serviceProvider = services.BuildServiceProvider();
            var timeProvider = serviceProvider.GetRequiredService<FakeTimeProvider>();
            timeProvider.SetUtcNow(DateTime.Parse("2021-01-01 00:00:00"));



            Console.WriteLine("Running simulation ...");

            using var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("testClient");
            var responseTask = client.GetAsync("http://example.com");

            syncContext.Run();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Adding 10s ... {timeProvider.GetUtcNow()}");
                timeProvider.Advance(TimeSpan.FromSeconds(10));
                syncContext.Run();
            }

            Console.WriteLine(responseTask.Result.StatusCode);
        }

        private static void Configure(IServiceCollection services)
        {
            var timeProvider = new FakeTimeProvider();
            services.AddSingleton<TimeProvider>(timeProvider);
            services.AddSingleton(timeProvider);
            services.AddSingleton((b) => ActivatorUtilities.CreateInstance<DelayHandler>(b, TimeSpan.FromSeconds(19)));

            var builder = services.AddHttpClient("testClient");
            builder.ConfigurePrimaryHttpMessageHandler<DelayHandler>()
                .AddStandardResilienceHandler();
        }
    }
}
