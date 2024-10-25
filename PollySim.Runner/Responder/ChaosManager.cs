using Polly;

namespace PollySim.Runner.Responder
{
    public class ChaosManager
    {
        public TimeSpan StartOffset { get; }
        public TimeSpan ChaosDuration { get; }

        public static readonly ChaosManager Default = new(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

        public ChaosManager(TimeSpan startOffset, TimeSpan chaosDuration)
        {
            StartOffset = startOffset;
            ChaosDuration = chaosDuration;
        }

        public ValueTask<bool> IsChaosEnabled(ResilienceContext context)
        {
            var testStartTime = context.GetTestStartTime();
            var curTime = DateTime.Now;
            var beginTime = testStartTime + StartOffset;
            var endTime = beginTime + ChaosDuration;

            if (curTime > beginTime && curTime < endTime)
            {
                return ValueTask.FromResult(true);
            }
            return ValueTask.FromResult(false);
        }

        public ValueTask<double> GetInjectionRate(ResilienceContext context)
        {
            return ValueTask.FromResult(1.0);
        }

        public static TimeSpan LinearDelayGenerator(TimeSpan delay, DateTime beginTime, TimeSpan duration, TimeSpan rampUpDuration)
        {
            var curTime = DateTime.Now;
            var startFullDelayTime = beginTime + rampUpDuration;
            var endFullDelayTime = startFullDelayTime + duration;
            var endTime = endFullDelayTime + rampUpDuration;


            if (curTime <= beginTime)
            {
                return TimeSpan.Zero;
            }

            if (curTime >= endTime)
            {
                return TimeSpan.Zero;
            }

            if (curTime >= startFullDelayTime && curTime <= endFullDelayTime)
            {
                return delay;
            }

            if (curTime > endFullDelayTime)
            {
                return (curTime - endFullDelayTime) / rampUpDuration * delay;
            }

            if (curTime > beginTime)
            {
                return (curTime - beginTime) / rampUpDuration * delay;
            }

            return TimeSpan.Zero;
        }
    }
}
