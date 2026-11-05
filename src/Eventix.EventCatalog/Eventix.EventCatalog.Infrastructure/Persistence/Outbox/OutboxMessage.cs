namespace Eventix.EventCatalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Represents a message stored in the Outbox Pattern table.
/// Ensures reliable message publishing within the same database transaction.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }

    /// <summary>
    /// The fully qualified type name of the Integration Event (used for deserialization).
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// The JSON serialized content of the Integration Event.
    /// </summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// When the domain event originally occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// When the background worker successfully published this message.
    /// Null means it is still pending.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Error message if the publication to the message broker failed.
    /// </summary>
    public string? Error { get; set; }
}