using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PollySimulator.SimulatorHandler
{
    internal class DelayHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;
        private readonly TimeProvider _timeProvider;

        public DelayHandler(TimeSpan delay, TimeProvider provider)
            : base()
        {
            _delay = delay;
            _timeProvider = provider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Simulated delay started");
            await Task.Delay(_delay, _timeProvider, cancellationToken);
            Console.WriteLine("Simulated success");
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
