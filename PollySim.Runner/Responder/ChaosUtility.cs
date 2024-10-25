using Polly;
using Polly.Simmy.Outcomes;
using System.Net;

namespace PollySim.Runner.Responder
{
    public static class ChaosUtility
    {
        public static Func<OutcomeGeneratorArguments, ValueTask<Outcome<TValue>?>> FromResult<TValue>(TValue value)
        {
            return (args) => ValueTask.FromResult<Outcome<TValue>?>(Outcome.FromResult(value));
        }

        public static Func<OutcomeGeneratorArguments, ValueTask<Outcome<HttpResponseMessage>?>> FromStatusCode(HttpStatusCode statusCode)
        {
            return FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
