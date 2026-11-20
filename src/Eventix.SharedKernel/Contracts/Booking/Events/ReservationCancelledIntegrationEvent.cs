using Eventix.Chassis.Core.Messaging;

namespace Eventix.SharedKernel.Contracts.Booking.Events;

/// <summary>
/// LIGHTWEIGHT integration event: consumers already have the reservation data.
/// </summary>
public sealed record ReservationCancelledIntegrationEvent : IntegrationEventBase
{
    public Guid ReservationId { get; init; }
    public int Quantity { get; init; }
    public DateTime OccurredOn { get; init; }
}