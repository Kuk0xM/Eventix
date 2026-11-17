using Eventix.Chassis.Core.Events;

namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Domain event indicating that an event has been published.
/// This is a LIGHTWEIGHT event because consumers already have the event data
/// from the EventCreatedEvent. They only need to know the state transition.
/// OccurredOn is inherited from the Chassis DomainEvent base.
/// </summary>
public sealed record EventPublishedEvent : DomainEvent
{
    public Guid EventId { get; init; }
    public EventStatus NewStatus { get; init; }

    public EventPublishedEvent(Guid eventId, EventStatus newStatus)
    {
        EventId = eventId;
        NewStatus = newStatus;
    }
}