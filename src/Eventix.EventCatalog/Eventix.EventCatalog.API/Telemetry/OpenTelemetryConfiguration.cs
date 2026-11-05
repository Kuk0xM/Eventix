using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Eventix.EventCatalog.API.Telemetry;

/// <summary>
/// Configures OpenTelemetry for distributed tracing and metrics.
/// </summary>
public static class OpenTelemetryConfiguration
{
    public const string ServiceName = "Eventix.EventCatalog";

    public static IServiceCollection AddEventixTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otelEndpoint = configuration["OpenTelemetry:Endpoint"]
            ?? "http://localhost:4317"; // OTLP default (Aspire Dashboard, Jaeger, etc.)

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: "1.0.0",
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Filter out health checks and swagger from traces
                    options.Filter = httpContext =>
                    {
                        var path = httpContext.Request.Path.Value ?? "";
                        return !path.Contains("/health")
                            && !path.Contains("/swagger")
                            && !path.Contains("/openapi");
                    };
                })
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddSource(ServiceName) // Our custom activities
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otelEndpoint);
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(ServiceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otelEndpoint);
                }));

        return services;
    }
}