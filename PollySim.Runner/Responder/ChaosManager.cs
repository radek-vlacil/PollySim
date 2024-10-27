using Polly;
using System.Numerics;

namespace PollySim.Runner.Responder
{
    public class ChaosManager
    {
        public TimeSpan StartOffset { get; }
        public TimeSpan ChaosDuration { get; }
        public double InjectionRate { get; }
        public TimeSpan RumpUpDuration { get; }
        public string? Host { get; }

        public static readonly ChaosManager Default = new(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

        public ChaosManager(TimeSpan startOffset, TimeSpan chaosDuration)
            : this(startOffset, chaosDuration, 1.0, TimeSpan.Zero)
        {
        }
        
        public ChaosManager(TimeSpan startOffset, TimeSpan chaosDuration, double injectionRate)
            : this(startOffset, chaosDuration, injectionRate, TimeSpan.Zero)
        {
        }

        public ChaosManager(TimeSpan startOffset, TimeSpan chaosDuration, double injectionRate, TimeSpan rumpUpDuration, string? host = null)
        {
            StartOffset = startOffset;
            ChaosDuration = chaosDuration;
            InjectionRate = injectionRate;
            RumpUpDuration = rumpUpDuration;
            Host = host;
        }

        public ValueTask<bool> IsChaosEnabled(ResilienceContext context)
        {
            var testStartTime = context.GetTestStartTime();
            var curOffset = DateTime.Now - testStartTime;
            var beginOffset = StartOffset;
            var endOffset = beginOffset + RumpUpDuration + ChaosDuration + RumpUpDuration;
            var request = context.GetRequestMessage();

            if (Host != null && request != null && request.RequestUri?.Host != Host)
            {
                return ValueTask.FromResult(false);
            }

            if (curOffset > beginOffset && curOffset < endOffset)
            {
                return ValueTask.FromResult(true);
            }
            return ValueTask.FromResult(false);
        }

        public ValueTask<double> GetInjectionRate(ResilienceContext context)
        {
            return ValueTask.FromResult(InjectionRate);
        }

        public ValueTask<double> GetLinearProgressionInjectionRate(ResilienceContext context)
        {
            return ValueTask.FromResult(LinearProgressionGenerator(InjectionRate, context.GetTestStartTime() + StartOffset, ChaosDuration, RumpUpDuration));
        }

        public ValueTask<TimeSpan> GetLinearDelay(TimeSpan maxDelay, ResilienceContext context)
        {
            return ValueTask.FromResult(LinearDelayGenerator(maxDelay, context.GetTestStartTime() + StartOffset, ChaosDuration, RumpUpDuration));
        }

        public static double LinearProgressionGenerator(double value, DateTime beginTime, TimeSpan duration, TimeSpan rampUpDuration)
        {
            return LinearProgressionGeneratorImpl(value, beginTime, duration, rampUpDuration, (p, v) => p * v);
        }

        public static TimeSpan LinearDelayGenerator(TimeSpan value, DateTime beginTime, TimeSpan duration, TimeSpan rampUpDuration)
        {
            var milliseconds = value.TotalMilliseconds;
            var result = LinearProgressionGeneratorImpl(milliseconds, beginTime, duration, rampUpDuration, (p, v) => p * v);
            return TimeSpan.FromMilliseconds(result);
        }

        private static TValue LinearProgressionGeneratorImpl<TValue>(TValue value, DateTime beginTime, TimeSpan duration, TimeSpan rampUpDuration, Func<double, TValue, TValue> multiply) where TValue : INumber<TValue>
        {
            var curOffset = DateTime.Now - beginTime;
            var endFullOffset = rampUpDuration + duration;
            var endOffset = endFullOffset + rampUpDuration;

            if (curOffset <= TimeSpan.Zero)
            {
                return default;
            }

            if (curOffset >= endOffset)
            {
                return default;
            }

            if (curOffset >= rampUpDuration && curOffset <= endFullOffset)
            {
                return value;
            }

            if (curOffset > endFullOffset)
            {
                return value - multiply((curOffset - endFullOffset) / rampUpDuration, value);
            }

            return multiply(curOffset / rampUpDuration, value);
        }
    }
}
