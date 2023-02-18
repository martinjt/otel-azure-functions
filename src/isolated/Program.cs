using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(app =>
    {
        app.AddOpenTelemetry();
    })
    .ConfigureServices((builder, services) =>
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resBuilder => resBuilder.AddService("Isolated"))
            .WithTracing(tracerBuilder => tracerBuilder
                .AddSource(ActivityTrackingMiddleware.Source.Name)
                .SetSampler(new AlwaysOnSampler())
                .AddOtlpExporter()
            );
    })
    .Build();

host.Run();

public static class ActivityConfig
{
    public static ActivitySource Source = new ActivitySource("isolated-function");
}
