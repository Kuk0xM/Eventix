namespace Eventix.Chassis.Core.Messaging;

/// <summary>
/// Marker interface for all integration events published across microservice boundaries.
/// Integration events represent facts that have occurred in one bounded context
/// and may be of interest to other bounded contexts.
/// 
/// IMPORTANT: These events form a PUBLIC CONTRACT between services.
/// Breaking changes to integration events will break consumers.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this specific event occurrence.
    /// Used for idempotency and deduplication.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Name of the event type. Used for routing and deserialization.
    /// Convention: "{ServiceName}.{EventName}" e.g., "EventCatalog.EventCreated"
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base record implementation with sensible defaults.
/// Inherit from this for your integration events.
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public virtual string EventType => GetType().Name;
}