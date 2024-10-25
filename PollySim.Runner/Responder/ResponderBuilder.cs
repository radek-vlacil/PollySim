using System.Net;

namespace PollySim.Runner.Responder
{
    public class ResponderBuilder
    {
        private readonly List<(TimeSpan Leght, IResponder Responder)> _responders;
        private TimeSpan _currentOffset;

        public ResponderBuilder()
        {
            _responders = [];
            _currentOffset = TimeSpan.Zero;
        }

        public ResponderBuilder AddSuccess() => AddSuccess(TimeSpan.MaxValue);
        public ResponderBuilder AddSuccess(TimeSpan length)
        {
            AddResponder(length, new StatusCode(HttpStatusCode.OK));
            return this;
        }

        public ResponderBuilder AddFailure() => AddFailure(TimeSpan.MaxValue);
        public ResponderBuilder AddFailure(TimeSpan length)
        {
            AddResponder(length, new StatusCode(HttpStatusCode.InternalServerError));
            return this;
        }

        private ResponderBuilder AddResponder(TimeSpan length, IResponder responder)
        {
            _responders.Add((_currentOffset, responder));
            if (length == TimeSpan.MaxValue)
            {
                _currentOffset = TimeSpan.MaxValue;
            }
            else
            {
                _currentOffset += length;
            }

            return this;
        }

        public IResponder Build()
        {
            return new TimedResponder(_responders);
        }
    }
}
