using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Domain event indicating that a new event has been created in the catalog.
/// This is a RICH event because consumers have no prior knowledge of this entity.
/// </summary>
public sealed record EventCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; }
    public string Name { get; init; }
    public EventDate StartDate { get; init; }
    public EventDate EndDate { get; init; }
    public Guid VenueId { get; init; }
    public EventStatus Status { get; init; }
    public DateTime OccurredOn { get; init; }

    public EventCreatedEvent(
        Guid eventId,
        string name,
        EventDate startDate,
        EventDate endDate,
        Guid venueId,
        EventStatus status)
    {
        EventId = eventId;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        VenueId = venueId;
        Status = status;
        OccurredOn = DateTime.UtcNow;
    }
}