namespace Eventix.SharedKernel.Contracts.Catalog.Events;

/// <summary>
/// Integration event published when an event changes its state (Published, Cancelled).
/// This is a LIGHTWEIGHT event. Consumers already have the event data.
/// </summary>
public sealed record EventStatusChangedIntegrationEvent
{
    public Guid EventId { get; init; }
    public string NewStatus { get; init; } = default!;
    public DateTime OccurredOn { get; init; }
}