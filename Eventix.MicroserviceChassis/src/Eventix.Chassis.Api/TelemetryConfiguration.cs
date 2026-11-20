using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace Eventix.Chassis.Api;

public static class TelemetryConfiguration
{
    public static IServiceCollection AddEventixTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
        var azureMonitorConnectionString = configuration["ApplicationInsights:ConnectionString"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource($"Eventix.{serviceName}");

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));

                if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    tracing.AddAzureMonitorTraceExporter(options => options.ConnectionString = azureMonitorConnectionString);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter($"Eventix.{serviceName}");

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));

                if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    metrics.AddAzureMonitorMetricExporter(options => options.ConnectionString = azureMonitorConnectionString);
            });

        return services;
    }
}