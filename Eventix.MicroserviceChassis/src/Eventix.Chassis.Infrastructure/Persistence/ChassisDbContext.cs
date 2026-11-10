using Eventix.Chassis.Core.Events;
using Eventix.Chassis.Core.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Chassis.Infrastructure.Persistence;

/// <summary>
/// Base DbContext that all microservices should inherit from.
/// 
/// Provides:
/// - Built-in OutboxMessages DbSet (required for the Outbox Pattern)
/// - Domain event collection from aggregate roots on SaveChanges
/// - Automatic interceptor registration via OnConfiguring
/// 
/// Usage in your microservice:
/// <code>
/// public class BookingDbContext : ChassisDbContext&lt;BookingDbContext&gt;
/// {
///     public BookingDbContext(DbContextOptions&lt;BookingDbContext&gt; options) : base(options) { }
///     
///     public DbSet&lt;Reservation&gt; Reservations => Set&lt;Reservation&gt;();
///     
///     protected override void OnModelCreating(ModelBuilder builder)
///     {
///         builder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
///         base.OnModelCreating(builder);
///     }
/// }
/// </code>
/// </summary>
public abstract class ChassisDbContext<TContext> : DbContext where TContext : DbContext
{
    /// <summary>
    /// The outbox table. Every microservice gets its own OutboxMessages table
    /// with identical schema but isolated data.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected ChassisDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the OutboxMessage table schema
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Error).HasMaxLength(4000);

            // Index for efficient polling by the OutboxWorker
            entity.HasIndex(e => e.ProcessedOnUtc);
            entity.HasIndex(e => e.OccurredOnUtc);
        });

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Collects all pending domain events from tracked aggregate roots.
    /// Called by the OutboxSaveChangesInterceptor before SaveChanges.
    /// </summary>
    public IReadOnlyList<IDomainEvent> ExtractDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is not null && e.Entity.GetType().GetProperties()
                         .Any(p => p.Name == "DomainEvents")))
        {
            var entity = entry.Entity;
            var domainEventsProperty = entity.GetType().GetProperty("DomainEvents");

            if (domainEventsProperty?.GetValue(entity) is IEnumerable<IDomainEvent> events)
            {
                domainEvents.AddRange(events);

                // Clear the events after extraction so they aren't re-published
                var clearMethod = domainEventsProperty.PropertyType.GetMethod("Clear");
                clearMethod?.Invoke(domainEventsProperty.GetValue(entity), null);
            }
        }

        return domainEvents;
    }
}