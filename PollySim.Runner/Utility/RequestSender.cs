using Polly;

namespace PollySim.Runner.Utility
{
    public static class RequestSender
    {
        public static async Task<HttpResponseMessage> Send(IHttpClientFactory clientFactory, string clientName, string url, DateTime testStartTime)
        {
            ResilienceContext? resilienceContext = null;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                resilienceContext = ResilienceContextPool.Shared.Get();
                resilienceContext.SetTestStartTime(testStartTime);
                request.SetResilienceContext(resilienceContext);

                using var client = clientFactory.CreateClient(clientName);

                return await client.SendAsync(request);
            }
            finally
            {
                if (resilienceContext != null)
                {
                    ResilienceContextPool.Shared.Return(resilienceContext);
                }
            }
        }
    }
}
