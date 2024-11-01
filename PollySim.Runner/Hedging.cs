using Microsoft.Extensions.Http.Resilience;
using Polly.Simmy;
using Polly.Simmy.Latency;
using PollySim.Runner.Utility;

namespace PollySim.Runner
{
    public class Hedging
    {
        private static readonly string ClientName = "Hedging";

        public static async Task<HttpResponseMessage> Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            return await RequestSender.Send(clientFactory, ClientName, "http://hedging", startTime);
        }

        public static void Configure(IServiceCollection services)
        {
            var clientBuilder = services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler<SuccessHandler>();

            clientBuilder.AddStandardHedgingHandler(builder =>
                    {
                        builder.ConfigureOrderedGroups(static options =>
                            {
                                options.Groups = [
                                    new()
                                    {
                                        Endpoints = [ new() { Uri = new("https://hedging.primary") } ]
                                    },
                                    new()
                                    {
                                        Endpoints = [ new() { Uri = new("https://hedging.secondary") } ]
                                    }
                                ];
                            });
                    }
                ).Configure(options => {
                    options.Hedging.Delay = TimeSpan.FromSeconds(2);
                    options.Endpoint.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(6);
                    //options.Hedging.OnHedging = // you can change the request here
                    /*
                    options.Endpoint.CircuitBreaker.ShouldHandle = (args) => 
                        {
                            if (args.Outcome.Exception is TaskCanceledException)
                            {
                                // adding this will force circuit breaker to handle the cancelations as failures
                                return ValueTask.FromResult(true);
                            }

                            // it is recommended to put is transient as the last check
                            return ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome));
                        };
                    */
                    options.Endpoint.Timeout.Timeout = TimeSpan.FromSeconds(3);
                }).SelectPipelineByAuthority();
            clientBuilder.AddResilienceHandler("chaos", configure =>
                {
                    var chaosManager = new ChaosManager(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 1.0, TimeSpan.FromSeconds(10), "hedging.primary");
                    var chaosStrategy = new ChaosLatencyStrategyOptions()
                    {
                        EnabledGenerator = (args) => chaosManager.IsChaosEnabled(args.Context),
                        InjectionRateGenerator = (args) => chaosManager.GetInjectionRate(args.Context),
                        LatencyGenerator = (args) => chaosManager.GetLinearDelay(TimeSpan.FromSeconds(4), args.Context)
                    };

                    configure.AddChaosLatency(chaosStrategy);
                }
            );
        }
    }
}
