namespace Eventix.Chassis.Core.Messaging;

/// <summary>
/// Handler interface for processing integration events.
/// Implement this in your microservice to react to events from other services.
/// 
/// Example:
/// <code>
/// public class EventCreatedHandler : IIntegrationEventHandler&lt;EventCreatedIntegrationEvent&gt;
/// {
///     public async Task HandleAsync(EventCreatedIntegrationEvent @event, CancellationToken ct)
///     {
///         // Create ticket availability, send notifications, etc.
///     }
/// }
/// </code>
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}