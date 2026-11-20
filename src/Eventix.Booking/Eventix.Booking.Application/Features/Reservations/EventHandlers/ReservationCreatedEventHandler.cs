using Eventix.Booking.Domain.Reservations.Events;
using Eventix.Chassis.Core.Messaging;
using Eventix.SharedKernel.Contracts.Booking.Events;
using MediatR;

namespace Eventix.Booking.Application.Features.Reservations.EventHandlers;

public sealed class ReservationCreatedEventHandler : INotificationHandler<ReservationCreatedEvent>
{
    private readonly IOutboxPublisher _outboxPublisher;

    public ReservationCreatedEventHandler(IOutboxPublisher outboxPublisher)
    {
        _outboxPublisher = outboxPublisher;
    }

    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _outboxPublisher.EnqueueAsync(new ReservationCreatedIntegrationEvent
        {
            ReservationId = notification.ReservationId,
            EventId = notification.EventId,
            UserId = notification.UserId,
            Quantity = notification.Quantity,
            OccurredOn = notification.OccurredOn
        }, cancellationToken);
    }
}