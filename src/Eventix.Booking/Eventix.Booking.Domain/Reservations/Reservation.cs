using Eventix.Booking.Domain.Reservations.Events;
using Eventix.Booking.Domain.Reservations.Rules;
using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Reservations;

public class Reservation : AggregateRoot<Guid>
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Reservation() { }

    private Reservation(Guid id, Guid eventId, Guid userId, int quantity) : base(id)
    {
        CheckRule(new PositiveQuantityRule(quantity));

        EventId = eventId;
        UserId = userId;
        Quantity = quantity;
        Status = ReservationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ReservationCreatedEvent(id, eventId, userId, quantity));
    }

    public static Reservation Create(Guid eventId, Guid userId, int quantity)
        => new(Guid.NewGuid(), eventId, userId, quantity);

    public void Confirm()
    {
        CheckRule(new ReservationMustBePendingRule(Status));
        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        CheckRule(new ReservationCanBeCancelledRule(Status));
        Status = ReservationStatus.Cancelled;

        AddDomainEvent(new ReservationCancelledEvent(Id, EventId, Quantity));
    }
}