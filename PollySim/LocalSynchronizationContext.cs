using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollySimulator
{
    internal class LocalSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback, object)> _workQueue;

        public LocalSynchronizationContext()
        {
             _workQueue = new ConcurrentQueue<(SendOrPostCallback, object)>();
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            _workQueue.Enqueue((d, state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            d(state);
        }

        public void Run()
        {
            bool workDone = true;

            while (workDone)
            {
                workDone = false;

                while (_workQueue.TryDequeue(out var item))
                {
                    item.Item1(item.Item2);
                    workDone = true;
                }
            }
        }
    }
}
