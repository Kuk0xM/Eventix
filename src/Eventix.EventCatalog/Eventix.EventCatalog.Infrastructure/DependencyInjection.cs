using Eventix.Chassis.Infrastructure;
using Eventix.EventCatalog.Application.Common.Interfaces;
using Eventix.EventCatalog.Infrastructure.Persistence;
using Eventix.EventCatalog.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.EventCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL + Outbox + RabbitMQ + Polly
        services.AddEventixChassis<EventCatalogDbContext>(configuration, "eventcatalog");

        // Thin repositories
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}