using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Reservations.Rules;

public sealed class PositiveQuantityRule(int quantity) : IBusinessRule
{
    public string Message => "Quantity must be greater than zero.";

    public bool IsBroken() => quantity <= 0;
}