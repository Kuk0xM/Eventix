using Eventix.EventCatalog.Application.Common.Interfaces;
using Eventix.EventCatalog.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for the Event aggregate.
/// This is NOT a generic repository - it's specific to the Event aggregate
/// and handles domain event dispatching through the SaveChanges interceptor.
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly EventCatalogDbContext _dbContext;

    public EventRepository(EventCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a new event to the database.
    /// The event is tracked by EF Core but not yet persisted.
    /// Call SaveChangesAsync to persist.
    /// </summary>
    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(@event, cancellationToken);
    }

    /// <summary>
    /// Gets an event by its unique identifier.
    /// Returns null if the event doesn't exist.
    /// </summary>
    public async Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    /// <summary>
    /// Updates an existing event.
    /// EF Core tracks the entity, so usually you don't need to call Update explicitly.
    /// But we include it for clarity and for cases where the entity is detached.
    /// </summary>
    public async Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Update(@event);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Saves all pending changes to the database.
    /// The PublishDomainEventsInterceptor will handle domain event dispatching
    /// before the changes are committed.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}