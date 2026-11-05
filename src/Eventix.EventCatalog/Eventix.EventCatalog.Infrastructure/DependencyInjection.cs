using Eventix.EventCatalog.Application.Common.Interfaces;
using Eventix.EventCatalog.Infrastructure.Messaging;
using Eventix.EventCatalog.Infrastructure.Persistence;
using Eventix.EventCatalog.Infrastructure.Persistence.Interceptors;
using Eventix.EventCatalog.Infrastructure.Persistence.Outbox;
using Eventix.EventCatalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Eventix.EventCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Interceptor
        services.AddScoped<PublishDomainEventsInterceptor>();

        // 2. DbContext
        services.AddDbContext<EventCatalogDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("EventCatalogDatabase");
            var interceptor = serviceProvider.GetRequiredService<PublishDomainEventsInterceptor>();

            options.UseNpgsql(connectionString);
            options.AddInterceptors(interceptor);
        });

        // 3. Repositories
        services.AddScoped<IEventRepository, EventRepository>();

        // 4. RabbitMQ Connection Factory
        services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:User"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        });

        // 5. RabbitMQ Connection (Singleton - required by health check v9.0.0)
        // The connection is created lazily when first requested.
        // If RabbitMQ is down, the health check will catch the exception
        // and report "Degraded" instead of crashing the app.
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // 6. Resilience Pipeline (Microsoft.Extensions.Resilience)
        services.AddRabbitMqResilience();

        // 7. Message Publisher
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

        // 8. Outbox Background Worker
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}