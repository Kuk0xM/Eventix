using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Reservations.Rules;

public sealed class ReservationCanBeCancelledRule(ReservationStatus status) : IBusinessRule
{
    public string Message => "Cancelled reservations cannot be cancelled again.";

    public bool IsBroken() => status == ReservationStatus.Cancelled;
}