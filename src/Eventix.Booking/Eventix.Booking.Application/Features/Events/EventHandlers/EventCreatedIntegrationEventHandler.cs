using Eventix.Booking.Application.Common.Interfaces;
using Eventix.Booking.Domain.Availability;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using MediatR;

namespace Eventix.Booking.Application.Features.Events.EventHandlers;

/// <summary>
/// Consumes EventCreatedIntegrationEvent from EventCatalog and creates the local
/// ticket-stock projection. (The RabbitMQ consumer host that feeds MediatR arrives in Fase 13.)
/// </summary>
public sealed class EventCreatedIntegrationEventHandler : INotificationHandler<EventCreatedIntegrationEvent>
{
    private const int DefaultTotalTickets = 100;
    private readonly IEventAvailabilityRepository _availabilityRepository;

    public EventCreatedIntegrationEventHandler(IEventAvailabilityRepository availabilityRepository)
    {
        _availabilityRepository = availabilityRepository;
    }

    public async Task Handle(EventCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var existing = await _availabilityRepository.GetByEventIdAsync(notification.EventId, cancellationToken);
        if (existing is not null) return; // idempotency

        await _availabilityRepository.AddAsync(
            EventAvailability.Create(notification.EventId, DefaultTotalTickets), cancellationToken);
    }
}