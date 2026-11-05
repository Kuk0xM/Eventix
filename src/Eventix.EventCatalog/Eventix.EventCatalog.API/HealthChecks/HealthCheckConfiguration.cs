using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Eventix.EventCatalog.API.HealthChecks;

/// <summary>
/// Configures health checks for the Eventix.EventCatalog service.
/// </summary>
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddEventixHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Build the RabbitMQ connection factory
        var rabbitMqConnectionFactory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:User"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        };

        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("EventCatalogDatabase")!,
                name: "postgres",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "ready" })
            .AddRabbitMQ(
                name: "rabbitmq",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "messaging", "ready" });

        return services;
    }

    public static IEndpointRouteBuilder MapEventixHealthChecks(this IEndpointRouteBuilder app)
    {
        // Liveness check - is the app alive?
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });

        // Readiness check - is the app ready to serve traffic?
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}