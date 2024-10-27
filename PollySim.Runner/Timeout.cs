using Polly;
using PollySim.Runner.Responder;
using Microsoft.Extensions.Http.Resilience;
using Polly.Simmy;
using Polly.Simmy.Latency;

namespace PollySim.Runner
{
    public static class Timeout
    {
        private static readonly string ClientName = "Timeout";

        public static async Task<HttpResponseMessage> Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://timeout");

            var resilienceContext = ResilienceContextPool.Shared.Get();
            resilienceContext.SetTestStartTime(startTime);
            request.SetResilienceContext(resilienceContext);

            using var client = clientFactory.CreateClient(ClientName);

            return await client.SendAsync(request);
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler<SuccessHandler>()
                .AddResilienceHandler(ClientName, configure =>
                {
                    var timeoutOptions = new HttpTimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromMilliseconds(1500)
                    };

                    var chaosManager = new ChaosManager(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30));
                    var chaosStrategy = new ChaosLatencyStrategyOptions()
                    {
                        EnabledGenerator = (args) => chaosManager.IsChaosEnabled(args.Context),
                        InjectionRateGenerator = (args) => chaosManager.GetInjectionRate(args.Context),
                        LatencyGenerator = (args) => ValueTask.FromResult(ChaosManager.LinearDelayGenerator(TimeSpan.FromSeconds(2), args.Context.GetTestStartTime() + chaosManager.StartOffset, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)))
                    };

                    configure.AddTimeout(timeoutOptions)
                        .AddChaosLatency(chaosStrategy);
                });
        }
    }
}
