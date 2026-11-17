using Eventix.Chassis.Infrastructure.Persistence;
using Eventix.EventCatalog.Domain.Events;
using Eventix.EventCatalog.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Eventix.EventCatalog.Infrastructure.Persistence;

/// <summary>
/// EventCatalog DbContext that inherits from ChassisDbContext.
/// The Chassis provides:
/// - OutboxMessages DbSet (for the Outbox Pattern)
/// - DomainEventDispatcherInterceptor (dispatches domain events via MediatR)
/// - OutboxWorker (publishes pending messages to RabbitMQ)
/// </summary>
public class EventCatalogDbContext : ChassisDbContext<EventCatalogDbContext>
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();

    public EventCatalogDbContext(DbContextOptions<EventCatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventCatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}