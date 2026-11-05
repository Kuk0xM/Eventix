namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Base contract for all domain events in the system.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier of the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Date and time when the event occurred (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}