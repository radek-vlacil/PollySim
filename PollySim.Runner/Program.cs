using PollySim.Runner.Responder;

namespace PollySim.Runner;

public class Program
{
    private static readonly TimeSpan _testDuration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan _testTick = TimeSpan.FromMilliseconds(100);
    private static readonly int _rps = 10;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        Configure(builder.Services);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.MapGet("/", () => "Supported endpoints: ['/retry', '/timeout']");
        app.MapGet("/retry", (IHttpClientFactory factory) =>
            {
                var _ = RunTest(factory, Retry.Run);
                return $"Retry Test Started for next {_testDuration.TotalSeconds} seconds with {_rps} RPS.";
            }
        );
        app.MapGet("/timeout", (IHttpClientFactory factory) =>
            {
                var _ = RunTest(factory, Timeout.Run);
                return $"Retry Test Started for next {_testDuration.TotalSeconds} seconds with {_rps} RPS.";
            }
        );

        app.Run();
    }

    private static void Configure(IServiceCollection services)
    {
        services.AddSingleton<SuccessHandler>();
        Retry.Configure(services);
        Timeout.Configure(services);
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
