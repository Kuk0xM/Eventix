namespace Eventix.Chassis.Core.Persistence.Outbox;

/// <summary>
/// Represents a message stored in the outbox table, waiting to be published
/// to the message broker.
/// 
/// This entity is SHARED across all microservices using the Chassis.
/// Each service has its own OutboxMessages table, but the schema is identical.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Fully qualified type name of the message (for deserialization).
    /// Example: "Eventix.Booking.Application.IntegrationEvents.ReservationCreatedIntegrationEvent"
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// JSON-serialized content of the message.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// When the message was created (when the business operation occurred).
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the message was successfully published to the broker.
    /// NULL means the message is still pending.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Error message if publishing failed (after all retries).
    /// NULL means no errors occurred.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Number of times we've attempted to publish this message.
    /// Used for backoff strategies and dead-letter routing.
    /// </summary>
    public int RetryCount { get; set; } = 0;
}