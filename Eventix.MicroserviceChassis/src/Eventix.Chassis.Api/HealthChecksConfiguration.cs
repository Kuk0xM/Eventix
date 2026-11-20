using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Eventix.Chassis.Api;

public static class HealthChecksConfiguration
{
    public static IServiceCollection AddEventixHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "ready" })
            .AddRabbitMQ(
                sp => sp.GetRequiredService<IConnection>(),
                name: "rabbitmq",
                tags: new[] { "ready" });

        return services;
    }
}