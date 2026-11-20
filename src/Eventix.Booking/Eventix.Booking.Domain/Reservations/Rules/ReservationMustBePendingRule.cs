using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Reservations.Rules;

public sealed class ReservationMustBePendingRule(ReservationStatus status) : IBusinessRule
{
    public string Message => "Only pending reservations can be confirmed.";

    public bool IsBroken() => status != ReservationStatus.Pending;
}