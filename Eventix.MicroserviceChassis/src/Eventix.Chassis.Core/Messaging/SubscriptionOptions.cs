namespace Eventix.Chassis.Core.Messaging;

/// <summary>
/// Configuration for how a microservice subscribes to events.
/// </summary>
public sealed record SubscriptionOptions
{
    /// <summary>
    /// Unique name for this service's queue. 
    /// Convention: "{service-name}.{event-type}-queue"
    /// e.g., "booking.event-created-queue"
    /// </summary>
    public required string QueueName { get; init; }

    /// <summary>
    /// The exchange name to bind to.
    /// Convention: "{service-name}.{event-type}"
    /// e.g., "eventcatalog.event-created"
    /// </summary>
    public required string ExchangeName { get; init; }

    /// <summary>
    /// Whether messages should be persisted (durable queues).
    /// Default: true (recommended for production).
    /// </summary>
    public bool Durable { get; init; } = true;

    /// <summary>
    /// Maximum number of unacknowledged messages (prefetch count).
    /// Controls consumer throughput and memory usage.
    /// </summary>
    public ushort PrefetchCount { get; init; } = 10;

    /// <summary>
    /// Whether to automatically retry failed messages.
    /// </summary>
    public bool EnableRetry { get; init; } = true;

    /// <summary>
    /// Maximum retry attempts before sending to dead-letter queue.
    /// </summary>
    public int MaxRetries { get; init; } = 3;
}