namespace Eventix.SharedKernel.Contracts.Catalog.Events;

/// <summary>
/// Integration event published to the message bus when an event is created.
/// This is a RICH event because consumers need all the initial data to build their projections.
/// </summary>
public sealed record EventCreatedIntegrationEvent
{
    public Guid EventId { get; init; }
    public string Name { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid VenueId { get; init; }
    public string Status { get; init; } = default!;
    public DateTime OccurredOn { get; init; }
}