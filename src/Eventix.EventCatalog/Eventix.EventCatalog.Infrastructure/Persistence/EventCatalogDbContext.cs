using Eventix.EventCatalog.Domain.Events;
using Eventix.EventCatalog.Domain.Venues;
using Eventix.EventCatalog.Infrastructure.Persistence.Interceptors;
using Eventix.EventCatalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Eventix.EventCatalog.Infrastructure.Persistence;

/// <summary>
/// Database context for the EventCatalog microservice.
/// Each microservice owns its own DbContext (Database-per-service pattern).
/// </summary>
public class EventCatalogDbContext : DbContext
{
    private readonly PublishDomainEventsInterceptor? _publishDomainEventsInterceptor;

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public EventCatalogDbContext(
        DbContextOptions<EventCatalogDbContext> options,
        PublishDomainEventsInterceptor? publishDomainEventsInterceptor = null)
        : base(options)
    {
        _publishDomainEventsInterceptor = publishDomainEventsInterceptor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventCatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add our custom interceptor for publishing domain events
        if (_publishDomainEventsInterceptor is not null)
        {
            optionsBuilder.AddInterceptors(_publishDomainEventsInterceptor);
        }

        base.OnConfiguring(optionsBuilder);
    }
}