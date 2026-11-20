using Eventix.Chassis.Core.Events;

namespace Eventix.Booking.Domain.Reservations.Events;

public sealed record ReservationCreatedEvent(
    Guid ReservationId,
    Guid EventId,
    Guid UserId,
    int Quantity) : DomainEvent;