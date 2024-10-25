using PollySim.Runner.Responder;

namespace PollySim.Runner;

public class Program
{
    private static readonly TimeSpan _testDuration = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan _testTick = TimeSpan.FromMilliseconds(100);
    private static readonly int _rps = 100;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        Configure(builder.Services);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.MapGet("/", () => "Supported endpoints: ['/retry', '/timeout', '/cb']");
        app.MapGet("/retry", (IHttpClientFactory factory) =>
            {
                var _ = RunTest(factory, Retry.Run);
                return $"Retry test started for next {_testDuration.TotalSeconds} seconds with {_rps} RPS.";
            }
        );
        app.MapGet("/timeout", (IHttpClientFactory factory) =>
            {
                var _ = RunTest(factory, Timeout.Run);
                return $"Timeout test started for next {_testDuration.TotalSeconds} seconds with {_rps} RPS.";
            }
        );
        app.MapGet("/cb", (IHttpClientFactory factory) =>
            {
                var _ = RunTest(factory, CircuitBreaker.Run);
                return $"CircuitBreaker test started for next {_testDuration.TotalSeconds} seconds with {_rps} RPS.";
            }
        );

        app.Run();
    }

    private static void Configure(IServiceCollection services)
    {
        services.AddSingleton<SuccessHandler>();
        Retry.Configure(services);
        Timeout.Configure(services);
        CircuitBreaker.Configure(services);
    }

    private static async Task RunTest(IHttpClientFactory factory, Func<IHttpClientFactory, DateTime, Task> test)
    {
        var periodicTimer = new PeriodicTimer(_testTick);
        var tickCount = (int) (_testDuration / _testTick);
        var ticksPerSecond = (int) (TimeSpan.FromSeconds(1) / _testTick);
        var requestPerTick = _rps / ticksPerSecond;
        var startTime = DateTime.Now;

        int i = 0;
        while (await periodicTimer.WaitForNextTickAsync() && i < tickCount)
        {
            foreach (var j in Enumerable.Range(0, requestPerTick))
            {
                var _ = test(factory, startTime);
            }
            ++i;
        }
    }
}
