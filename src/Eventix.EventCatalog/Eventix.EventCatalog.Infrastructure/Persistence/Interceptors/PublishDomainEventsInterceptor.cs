using System.Text.Json;
using Eventix.EventCatalog.Domain.Events;
using Eventix.EventCatalog.Infrastructure.Persistence.Outbox;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using Eventix.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Interceptors;

public class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var aggregateRoots = eventData.Context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        // Save domain events to OutboxMessages table (same transaction as entity)
        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                var integrationEvent = MapToIntegrationEvent(domainEvent);

                if (integrationEvent is not null)
                {
                    var outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = integrationEvent.GetType().AssemblyQualifiedName!,
                        Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                        OccurredOnUtc = domainEvent.OccurredOn
                    };

                    eventData.Context.Set<OutboxMessage>().Add(outboxMessage);
                }
            }

            aggregateRoot.ClearDomainEvents();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static object? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            EventCreatedEvent e => new EventCreatedIntegrationEvent
            {
                EventId = e.EventId,
                Name = e.Name,
                StartDate = e.StartDate.Value,
                EndDate = e.EndDate.Value,
                VenueId = e.VenueId,
                Status = e.Status.ToString(),
                OccurredOn = e.OccurredOn
            },
            EventPublishedEvent e => new EventStatusChangedIntegrationEvent
            {
                EventId = e.EventId,
                NewStatus = e.NewStatus.ToString(),
                OccurredOn = e.OccurredOn
            },
            EventCancelledEvent e => new EventStatusChangedIntegrationEvent
            {
                EventId = e.EventId,
                NewStatus = e.NewStatus.ToString(),
                OccurredOn = e.OccurredOn
            },
            _ => null
        };
    }
}