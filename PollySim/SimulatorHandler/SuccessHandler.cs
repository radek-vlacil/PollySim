using System.Net;

namespace PollySimulator.SimulatorHandler
{
    internal class SuccessHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Simulated success");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
