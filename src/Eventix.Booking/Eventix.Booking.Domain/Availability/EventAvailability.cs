using Eventix.Booking.Domain.Availability.Rules;
using Eventix.Booking.Domain.Reservations.Rules;
using Eventix.SharedKernel.Domain;

namespace Eventix.Booking.Domain.Availability;

/// <summary>
/// Local projection of an EventCatalog event + ticket stock.
/// Id equals the EventId from EventCatalog (direct lookup, no joins across services).
/// </summary>
public class EventAvailability : AggregateRoot<Guid>
{
    public int TotalTickets { get; private set; }
    public int AvailableTickets { get; private set; }

    private EventAvailability() { }

    private EventAvailability(Guid eventId, int totalTickets) : base(eventId)
    {
        CheckRule(new PositiveQuantityRule(totalTickets));

        TotalTickets = totalTickets;
        AvailableTickets = totalTickets;
    }

    public static EventAvailability Create(Guid eventId, int totalTickets)
        => new(eventId, totalTickets);

    public void Reserve(int quantity)
    {
        CheckRule(new PositiveQuantityRule(quantity));
        CheckRule(new EnoughTicketsAvailableRule(AvailableTickets, quantity));

        AvailableTickets -= quantity;
    }

    public void Release(int quantity)
    {
        CheckRule(new PositiveQuantityRule(quantity));
        AvailableTickets = Math.Min(TotalTickets, AvailableTickets + quantity);
    }
}