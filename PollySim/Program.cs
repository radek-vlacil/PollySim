using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Polly;
using PollySimulator.SimulatorHandler;

namespace PollySimulator
{
    internal class Program
    {
        private static readonly TimeSpan _stepSize = TimeSpan.FromSeconds(1);
        private static readonly string _clientName = "testClient";

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            Configure(services);

            var syncContext = new LocalSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            var serviceProvider = services.BuildServiceProvider();
            var timeProvider = serviceProvider.GetRequiredService<FakeTimeProvider>();
            timeProvider.SetUtcNow(DateTime.Parse("2021-01-01 00:00:00"));

            var responseTask = SendRequest(serviceProvider.GetRequiredService<IHttpClientFactory>());


            Console.WriteLine("Running simulation ...");

            syncContext.Run();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Adding {_stepSize} ... {timeProvider.GetUtcNow()}");
                timeProvider.Advance(_stepSize);
                syncContext.Run();
            }

            await responseTask;

            Console.WriteLine(responseTask.Result.StatusCode);
        }

        private static void Configure(IServiceCollection services)
        {
            var timeProvider = new FakeTimeProvider();
            services.AddSingleton<TimeProvider>(timeProvider);
            services.AddSingleton(timeProvider);
            services.AddSingleton((b) => ActivatorUtilities.CreateInstance<DelayHandler>(b, TimeSpan.FromSeconds(2)));

            var builder = services.AddHttpClient(_clientName);
            builder.ConfigurePrimaryHttpMessageHandler<DelayHandler>()
                .AddStandardResilienceHandler();
        }

        private static async Task<HttpResponseMessage> SendRequest(IHttpClientFactory factory)
        {
            using var client = factory.CreateClient(_clientName);
            var ctx = ResilienceContextPool.Shared.Get(continueOnCapturedContext: true);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetResilienceContext(ctx);

            return await client.SendAsync(request);
        }
    }
}
