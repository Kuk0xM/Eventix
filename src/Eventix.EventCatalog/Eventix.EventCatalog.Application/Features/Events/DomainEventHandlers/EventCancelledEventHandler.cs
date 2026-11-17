using Eventix.Chassis.Core.Messaging;
using Eventix.EventCatalog.Domain.Events;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventix.EventCatalog.Application.Features.Events.EventHandlers;

/// <summary>
/// Handles EventCancelledEvent by enqueuing an integration event into the Outbox.
/// </summary>
public sealed class EventCancelledEventHandler : INotificationHandler<EventCancelledEvent>
{
    private readonly IOutboxPublisher _outboxPublisher;
    private readonly ILogger<EventCancelledEventHandler> _logger;

    public EventCancelledEventHandler(
        IOutboxPublisher outboxPublisher,
        ILogger<EventCancelledEventHandler> logger)
    {
        _outboxPublisher = outboxPublisher;
        _logger = logger;
    }

    public async Task Handle(EventCancelledEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling EventCancelledEvent for event {EventId}",
            domainEvent.EventId);

        var integrationEvent = new EventStatusChangedIntegrationEvent
        {
            EventId = domainEvent.EventId,
            NewStatus = domainEvent.NewStatus.ToString(),
            OccurredOn = domainEvent.OccurredOn
        };

        await _outboxPublisher.EnqueueAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Enqueued EventStatusChangedIntegrationEvent (Cancelled) for event {EventId}",
            domainEvent.EventId);
    }
}