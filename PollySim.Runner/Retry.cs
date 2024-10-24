using Microsoft.Extensions.DependencyInjection;
using PollySim.Runner.Responder;

namespace PollySim.Runner
{
    public static class Retry
    {
        private static readonly string ClientName = "Retry";

        public static async Task Run(IHttpClientFactory clientFactory)
        {
            using var client = clientFactory.CreateClient(ClientName);

            await client.GetAsync("http://retry");
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<Success>();
            services.AddHttpClient(ClientName)
                .ConfigurePrimaryHttpMessageHandler<Success>()
                .AddStandardResilienceHandler();
        }
    }
}
