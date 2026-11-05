using Eventix.EventCatalog.Domain.Events;

namespace Eventix.EventCatalog.Application.Common.Interfaces;

/// <summary>
/// Repository contract for Event aggregate.
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Adds a new event to the repository.
    /// </summary>
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}