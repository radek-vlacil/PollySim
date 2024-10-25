namespace PollySim.Runner.Responder
{
    public class ResponderHandler : HttpMessageHandler
    {
        private readonly IResponder _responder;

        public ResponderHandler(IResponder responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = request.GetResponderStartTime();            

            return Task.FromResult(_responder.Respond(startTime));
        }
    }
}
