using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

public static class OpenTelemetryFunctionWorkerExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddOpenTelemetry(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<ActivityTrackingMiddleware>();
        builder.Services.TryAddSingleton<AzureResourceDetector>();
        builder.Services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracerProvider) => 
            tracerProvider
                .ConfigureResource(resourceBuilder => resourceBuilder.AddDetector(
                    serviceProvider.GetRequiredService<AzureResourceDetector>()
                ))
                .AddSource(ActivityTrackingMiddleware.Source.Name)
        );

        return builder;
    }
}