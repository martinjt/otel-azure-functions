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
    public static ActivitySource Source = new ActivitySource("InProc Function");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(o => Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(
                ResourceBuilder
                    .CreateDefault()
                    .AddAzureAttributes()
                    .AddService("in-proc-function"))
            .AddSource(ActivityConfig.Source.Name)
            .AddAspNetCoreInstrumentation()
            .SetSampler(new AlwaysOnSampler())
            .AddOtlpExporter()
            .Build()
        );
    }
}
