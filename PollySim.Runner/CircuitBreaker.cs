using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Simmy;
using Polly.Simmy.Outcomes;
using PollySim.Runner.Utility;
using System.Net;

namespace PollySim.Runner
{
    public class CircuitBreaker
    {
        private static readonly string ClientName = "CircuitBreaker";

        public static async Task<HttpResponseMessage> Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            return await RequestSender.Send(clientFactory, ClientName, "http://cb", startTime);
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler<SuccessHandler>()
                .AddResilienceHandler(ClientName, configure =>
                {
                    var cbOptions = new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.5,
                        MinimumThroughput = 60,
                        SamplingDuration = TimeSpan.FromSeconds(2),
                        BreakDuration = TimeSpan.FromSeconds(5)
                    };

                    var chaosManager = new ChaosManager(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(20), 0.6, TimeSpan.FromSeconds(15));
                    var chaosStrategy = new ChaosOutcomeStrategyOptions<HttpResponseMessage>()
                    {
                        EnabledGenerator = (args) => chaosManager.IsChaosEnabled(args.Context),
                        InjectionRateGenerator = (args) => chaosManager.GetLinearProgressionInjectionRate(args.Context),
                        OutcomeGenerator = ChaosUtility.FromStatusCode(HttpStatusCode.InternalServerError)
                    };

                    configure.AddCircuitBreaker(cbOptions)
                        .AddChaosOutcome(chaosStrategy);
                });
        }
    }
}
