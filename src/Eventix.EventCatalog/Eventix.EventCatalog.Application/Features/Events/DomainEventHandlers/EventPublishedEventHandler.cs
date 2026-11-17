using Eventix.Chassis.Core.Messaging;
using Eventix.EventCatalog.Domain.Events;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventix.EventCatalog.Application.Features.Events.EventHandlers;

/// <summary>
/// Handles EventPublishedEvent by enqueuing an integration event into the Outbox.
/// </summary>
public sealed class EventPublishedEventHandler : INotificationHandler<EventPublishedEvent>
{
    private readonly IOutboxPublisher _outboxPublisher;
    private readonly ILogger<EventPublishedEventHandler> _logger;

    public EventPublishedEventHandler(
        IOutboxPublisher outboxPublisher,
        ILogger<EventPublishedEventHandler> logger)
    {
        _outboxPublisher = outboxPublisher;
        _logger = logger;
    }

    public async Task Handle(EventPublishedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling EventPublishedEvent for event {EventId}",
            domainEvent.EventId);

        var integrationEvent = new EventStatusChangedIntegrationEvent
        {
            EventId = domainEvent.EventId,
            NewStatus = domainEvent.NewStatus.ToString(),
            OccurredOn = domainEvent.OccurredOn
        };

        await _outboxPublisher.EnqueueAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Enqueued EventStatusChangedIntegrationEvent (Published) for event {EventId}",
            domainEvent.EventId);
    }
}