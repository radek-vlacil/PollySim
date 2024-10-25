
namespace PollySim.Runner.Responder
{
    public class TimedResponder : IResponder
    {
        private readonly List<(TimeSpan TimeOffset, IResponder Responder)> _responders;

        public TimedResponder(List<(TimeSpan, IResponder)> responders)
        {
            if (responders.Count == 0)
            {
                throw new ArgumentException("At least one responder must be provided.");
            }

            _responders = responders;
        }

        public HttpResponseMessage Respond(DateTime startTime)
        {
            var curTime = DateTime.Now;
            var elapsedTime = curTime - startTime;

            var responder = _responders[0].Item2;
            var i = 0;
            while (i < _responders.Count && elapsedTime >= _responders[i].TimeOffset)
            {
                responder = _responders[i].Responder;
                ++i;
            }

            return responder.Respond(startTime);
        }
    }
}
