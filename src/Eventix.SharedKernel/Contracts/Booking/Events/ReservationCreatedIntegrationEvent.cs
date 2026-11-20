using Eventix.Chassis.Core.Messaging;

namespace Eventix.SharedKernel.Contracts.Booking.Events;

/// <summary>
/// RICH integration event: consumers (Payment, Notification) need the full reservation data.
/// </summary>
public sealed record ReservationCreatedIntegrationEvent : IntegrationEventBase
{
    public Guid ReservationId { get; init; }
    public Guid UserId { get; init; }
    public int Quantity { get; init; }
    public DateTime OccurredOn { get; init; }
}