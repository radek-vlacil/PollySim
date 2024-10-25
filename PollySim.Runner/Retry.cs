using Microsoft.Extensions.DependencyInjection;
using PollySim.Runner.Responder;

namespace PollySim.Runner
{
    public static class Retry
    {
        private static readonly string ClientName = "Retry";

        public static async Task Run(IHttpClientFactory clientFactory, DateTime startTime)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://retry");
            request.SetResponderStartTime(startTime);

            using var client = clientFactory.CreateClient(ClientName);

            await client.SendAsync(request);
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddKeyedSingleton(
                ClientName,
                new ResponderBuilder()
                    .AddSuccess(TimeSpan.FromSeconds(5))
                    .AddFailure(TimeSpan.FromSeconds(5))
                    .AddSuccess()
                    .Build());

            services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler((services) =>
                {
                    var responder = services.GetRequiredKeyedService<IResponder>(ClientName);
                    return new ResponderHandler(responder);
                })
                .AddStandardResilienceHandler();
        }
    }
}
