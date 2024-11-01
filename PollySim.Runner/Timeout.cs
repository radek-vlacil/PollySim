using Polly;
using Microsoft.Extensions.Http.Resilience;
using Polly.Simmy;
using Polly.Simmy.Latency;
using PollySim.Runner.Utility;

namespace PollySim.Runner
{
    public static class Timeout
    {
        private static readonly string ClientName = "Timeout";

        public static async Task<HttpResponseMessage> Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            return await RequestSender.Send(clientFactory, ClientName, "http://timeout", startTime);
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

                    var chaosManager = new ChaosManager(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 1.0, TimeSpan.FromSeconds(10));
                    var chaosStrategy = new ChaosLatencyStrategyOptions()
                    {
                        EnabledGenerator = (args) => chaosManager.IsChaosEnabled(args.Context),
                        InjectionRateGenerator = (args) => chaosManager.GetInjectionRate(args.Context),
                        LatencyGenerator = (args) => chaosManager.GetLinearDelay(TimeSpan.FromSeconds(2), args.Context)
                    };

                    configure.AddTimeout(timeoutOptions)
                        .AddChaosLatency(chaosStrategy);
                });
        }
    }
}
