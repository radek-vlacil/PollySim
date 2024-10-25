namespace PollySim.Runner.Responder
{
    public static class ResponderMessageExtensions
    {
        public static readonly HttpRequestOptionsKey<DateTime> ResponderStartTimeKey = new("ResponderStartTime");

        public static void SetResponderStartTime(this HttpRequestMessage request, DateTime startTime)
        {
            request.Options.Set(ResponderStartTimeKey, startTime);
        }

        public static DateTime GetResponderStartTime(this HttpRequestMessage request)
        {
            return request.Options.TryGetValue(ResponderStartTimeKey, out var startTime) ? startTime : DateTime.Now;
        }
    }
}
