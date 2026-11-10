namespace Eventix.Chassis.Core.Messaging;

/// <summary>
/// High-level abstraction for asynchronous message publishing.
/// This is the ONLY interface microservices should use to publish messages.
/// 
/// The underlying implementation (RabbitMQ, Azure Service Bus, Kafka, etc.)
/// is hidden behind this contract, allowing infrastructure changes without
/// touching business code.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes an integration event to all interested subscribers.
    /// The message is routed based on the event type (fanout semantics).
    /// 
    /// This operation should be resilient:
    /// - Automatic retries with exponential backoff
    /// - Circuit breaker on repeated failures
    /// - Outbox pattern for guaranteed delivery
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Publishes multiple events atomically (best-effort).
    /// Useful when a single command produces multiple integration events.
    /// </summary>
    Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}