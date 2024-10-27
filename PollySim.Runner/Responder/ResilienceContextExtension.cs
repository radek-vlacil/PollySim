using Polly;

namespace PollySim.Runner.Responder
{
    public static class ResilienceContextExtension
    {
        public static readonly ResiliencePropertyKey<DateTime> TestStartTimeKey = new("TestStartTime");
        public static readonly ResiliencePropertyKey<HttpRequestMessage?> RequestMessageKey = new("Resilience.Http.RequestMessage");

        public static void SetTestStartTime(this ResilienceContext request, DateTime startTime)
        {
            request.Properties.Set(TestStartTimeKey, startTime);
        }

        public static DateTime GetTestStartTime(this ResilienceContext request)
        {
            return request.Properties.TryGetValue(TestStartTimeKey, out var startTime) ? startTime : DateTime.Now;
        }

        public static HttpRequestMessage? GetRequestMessage(this ResilienceContext request)
        {
            return request.Properties.TryGetValue(RequestMessageKey, out var message) ? message : null;
        }
    }
}
