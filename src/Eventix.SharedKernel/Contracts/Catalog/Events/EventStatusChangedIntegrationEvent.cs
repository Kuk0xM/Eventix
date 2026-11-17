using Eventix.Chassis.Core.Messaging;

namespace Eventix.SharedKernel.Contracts.Catalog.Events;

/// <summary>
/// Integration event published when an event changes its state (Published, Cancelled).
/// This is a LIGHTWEIGHT event. Consumers already have the event data.
/// Inherits from IntegrationEventBase to satisfy the IIntegrationEvent contract.
/// </summary>
public sealed record EventStatusChangedIntegrationEvent : IntegrationEventBase
{
    public string NewStatus { get; init; } = default!;

    /// <summary>
    /// Business timestamp (when the event occurred in domain time).
    /// Kept alongside the Chassis's OccurredAt for backward compatibility.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}