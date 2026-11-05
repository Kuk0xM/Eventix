using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Domain event indicating that an event has been cancelled.
/// This is a LIGHTWEIGHT event because consumers already have the event data
/// from the EventCreatedEvent. They only need to know the state transition.
/// </summary>
public sealed record EventCancelledEvent : IDomainEvent
{
    public Guid EventId { get; init; }
    public EventStatus NewStatus { get; init; }
    public DateTime OccurredOn { get; init; }

    public EventCancelledEvent(Guid eventId, EventStatus newStatus)
    {
        EventId = eventId;
        NewStatus = newStatus;
        OccurredOn = DateTime.UtcNow;
    }
}