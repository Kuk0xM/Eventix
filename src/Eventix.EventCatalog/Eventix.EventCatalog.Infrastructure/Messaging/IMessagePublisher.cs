namespace Eventix.EventCatalog.Infrastructure.Messaging;

/// <summary>
/// Contract for publishing integration events to the message broker.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes an integration event to RabbitMQ.
    /// </summary>
    Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default);
}