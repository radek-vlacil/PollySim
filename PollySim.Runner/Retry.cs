using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Simmy;
using Polly.Simmy.Outcomes;
using PollySim.Runner.Utility;
using System.Net;

namespace PollySim.Runner
{
    public static class Retry
    {
        private static readonly string ClientName = "Retry";

        public static async Task<HttpResponseMessage> Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            return await RequestSender.Send(clientFactory, ClientName, "http://retry", startTime);
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler<SuccessHandler>()
                .AddResilienceHandler(ClientName, configure =>
                {
                    var retryOptions = new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromSeconds(2)
                    };

                    var chaosStrategy = new ChaosOutcomeStrategyOptions<HttpResponseMessage>()
                    {
                        EnabledGenerator = (args) => ChaosManager.Default.IsChaosEnabled(args.Context),
                        InjectionRateGenerator = (args) => ChaosManager.Default.GetInjectionRate(args.Context),
                        OutcomeGenerator = ChaosUtility.FromStatusCode(HttpStatusCode.InternalServerError)
                    };

                    configure.AddRetry(retryOptions)
                        .AddChaosOutcome(chaosStrategy);
                });
        }
    }
}
