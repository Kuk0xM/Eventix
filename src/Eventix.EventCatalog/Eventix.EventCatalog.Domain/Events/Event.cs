using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Aggregate Root representing an Event in the catalog (concert, theater play, etc.).
/// </summary>
public class Event : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public EventDate StartDate { get; private set; }
    public EventDate EndDate { get; private set; }
    public Guid VenueId { get; private set; }
    public EventStatus Status { get; private set; }

    // Private constructor for EF Core (Hydration) - No domain events triggered
    private Event() { }

    /// <summary>
    /// Factory method to create a new event validating all business rules.
    /// </summary>
    public static Event Create(string name, EventDate startDate, EventDate endDate, Guid venueId)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            VenueId = venueId,
            Status = EventStatus.Draft
        };

        // Validate business rules using the CheckRule pattern
        @event.CheckRule(new EventMustHaveNameRule(name));
        @event.CheckRule(new EventMustHaveValidDatesRule(startDate, endDate));

        // Register the domain event that will be published after persistence
        @event.AddDomainEvent(new EventCreatedEvent(
            @event.Id,
            name,
            startDate,
            endDate,
            venueId,
            EventStatus.Draft));

        return @event;
    }

    /// <summary>
    /// Reconstitutes an event from persistence without triggering domain events.
    /// Used by EF Core when loading from database.
    /// </summary>
    internal static Event Reconstitute(
        Guid id,
        string name,
        EventDate startDate,
        EventDate endDate,
        Guid venueId,
        EventStatus status)
    {
        return new Event
        {
            Id = id,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            VenueId = venueId,
            Status = status
        };
    }

    /// <summary>
    /// Publishes the event, changing it to active status.
    /// </summary>
    public void Publish()
    {
        if (Status == EventStatus.Published)
            throw new InvalidOperationException("Event is already published.");

        Status = EventStatus.Published;

        // Lightweight event: only ID + new status
        AddDomainEvent(new EventPublishedEvent(Id, Status));
    }

    /// <summary>
    /// Cancels the event.
    /// </summary>
    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            return; // Idempotent

        Status = EventStatus.Cancelled;

        // Lightweight event: only ID + new status
        AddDomainEvent(new EventCancelledEvent(Id, Status));
    }
}

/// <summary>
/// Possible states of an event.
/// </summary>
public enum EventStatus
{
    Draft = 0,
    Published = 1,
    Cancelled = 2
}