using Eventix.Chassis.Core.Events;

namespace Eventix.Booking.Domain.Reservations.Events;

public sealed record ReservationCancelledEvent(
    Guid ReservationId,
    Guid EventId,
    int Quantity) : DomainEvent;