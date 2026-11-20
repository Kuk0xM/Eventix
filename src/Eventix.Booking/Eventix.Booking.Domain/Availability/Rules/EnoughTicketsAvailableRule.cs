using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Availability.Rules;

public sealed class EnoughTicketsAvailableRule(int available, int requested) : IBusinessRule
{
    public string Message => $"Not enough tickets available. Requested: {requested}, available: {available}.";

    public bool IsBroken() => requested > available;
}