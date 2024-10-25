
using System.Net;

namespace PollySim.Runner.Responder
{
    public class StatusCode : IResponder
    {
        private readonly HttpStatusCode _statusCode;

        public StatusCode(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public HttpResponseMessage Respond(DateTime startTime)
        {
            return new HttpResponseMessage(_statusCode);
        }
    }
}
