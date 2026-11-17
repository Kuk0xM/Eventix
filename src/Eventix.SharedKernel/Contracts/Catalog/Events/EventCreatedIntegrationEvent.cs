using Eventix.Chassis.Core.Messaging;

namespace Eventix.SharedKernel.Contracts.Catalog.Events;

/// <summary>
/// Integration event published to the message bus when an event is created.
/// This is a RICH event because consumers need all the initial data to build their projections.
/// Inherits from IntegrationEventBase to satisfy the IIntegrationEvent contract.
/// </summary>
public sealed record EventCreatedIntegrationEvent : IntegrationEventBase
{
    public string Name { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid VenueId { get; init; }
    public string Status { get; init; } = default!;

    /// <summary>
    /// Business timestamp (when the event occurred in domain time).
    /// Kept alongside the Chassis's OccurredAt for backward compatibility.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}