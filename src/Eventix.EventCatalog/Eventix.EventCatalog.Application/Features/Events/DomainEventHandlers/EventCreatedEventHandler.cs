using Eventix.Chassis.Core.Messaging;
using Eventix.EventCatalog.Domain.Events;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventix.EventCatalog.Application.Features.Events.EventHandlers;

/// <summary>
/// Handles EventCreatedEvent by enqueuing an integration event into the Outbox.
/// The Chassis's OutboxWorker will later publish it to RabbitMQ.
/// </summary>
public sealed class EventCreatedEventHandler : INotificationHandler<EventCreatedEvent>
{
    private readonly IOutboxPublisher _outboxPublisher;
    private readonly ILogger<EventCreatedEventHandler> _logger;

    public EventCreatedEventHandler(
        IOutboxPublisher outboxPublisher,
        ILogger<EventCreatedEventHandler> logger)
    {
        _outboxPublisher = outboxPublisher;
        _logger = logger;
    }

    public async Task Handle(EventCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling EventCreatedEvent for event {EventId}",
            domainEvent.EventId);

        var integrationEvent = new EventCreatedIntegrationEvent
        {
            EventId = domainEvent.EventId,
            Name = domainEvent.Name,
            StartDate = domainEvent.StartDate.Value,
            EndDate = domainEvent.EndDate.Value,
            VenueId = domainEvent.VenueId,
            Status = domainEvent.Status.ToString(),
            OccurredOn = domainEvent.OccurredOn
        };

        await _outboxPublisher.EnqueueAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Enqueued EventCreatedIntegrationEvent for event {EventId}",
            domainEvent.EventId);
    }
}