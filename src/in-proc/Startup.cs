using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

[assembly: FunctionsStartup(typeof(Startup))]

public class Startup : FunctionsStartup
{
    public static ActivitySource source = new ActivitySource("InProc Function");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(o => Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("in-proc-function"))
            .AddSource(source.Name)
            .AddOtlpExporter(otlpOptions => {
                otlpOptions.Endpoint = new Uri($"http://{Environment.GetEnvironmentVariable("COLLECTOR_IP")}:4318/v1/traces");
                otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
            .AddProcessor(new BatchActivityExportProcessor(new TestExporter(o.GetRequiredService<ILogger<TestExporter>>())))
            .SetSampler(new AlwaysOnSampler())
            .Build()
        );
    }
}

public class TestExporter : BaseExporter<Activity>
{
    private readonly ILogger _logger;

    public TestExporter(ILogger<TestExporter> logger)
    {
        _logger = logger;
        _logger.LogInformation("Here");
        Console.WriteLine(typeof(TestExporter).AssemblyQualifiedName);
    }
    public override ExportResult Export(in Batch<Activity> batch)
    {
        _logger.LogInformation("Started Export");
        Thread.Sleep(1000);
        _logger.LogInformation($"SendingData for {batch.Count} events");
        return ExportResult.Success;
    }
}
