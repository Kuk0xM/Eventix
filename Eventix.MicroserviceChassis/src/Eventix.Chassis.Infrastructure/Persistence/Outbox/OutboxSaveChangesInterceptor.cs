using System.Text.Json;
using Eventix.Chassis.Core.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace Eventix.Chassis.Infrastructure.Persistence.Outbox;

/// <summary>
/// EF Core SaveChanges interceptor that automatically captures domain events
/// and stores them in the Outbox table WITHIN THE SAME TRANSACTION as the
/// business entities.
/// 
/// This is the heart of the Outbox Pattern:
/// 1. Business command modifies entities AND raises domain events
/// 2. Interceptor captures domain events BEFORE SaveChanges
/// 3. Interceptor serializes each event as an OutboxMessage
/// 4. SaveChanges persists everything in ONE atomic transaction
/// 5. OutboxWorker later publishes the messages to RabbitMQ asynchronously
/// 
/// Result: You cannot have "entity saved but event not queued" or vice versa.
/// </summary>
public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // Extract domain events from all tracked aggregate roots
        var domainEvents = ExtractDomainEventsFromContext(eventData.Context);

        // Convert each domain event into an OutboxMessage
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().AssemblyQualifiedName
                    ?? domainEvent.GetType().FullName
                    ?? domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                OccurredOnUtc = DateTime.UtcNow
            };

            eventData.Context.Set<OutboxMessage>().Add(outboxMessage);

            Log.Debug(
                "Queued domain event {EventType} to Outbox (Id: {OutboxId})",
                domainEvent.GetType().Name,
                outboxMessage.Id);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static IReadOnlyList<object> ExtractDomainEventsFromContext(DbContext context)
    {
        var domainEvents = new List<object>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var entity = entry.Entity;
            if (entity is null) continue;

            var domainEventsProperty = entity.GetType().GetProperty("DomainEvents");
            if (domainEventsProperty is null) continue;

            if (domainEventsProperty.GetValue(entity) is System.Collections.IEnumerable events)
            {
                foreach (var @event in events)
                {
                    if (@event is not null)
                    {
                        domainEvents.Add(@event);
                    }
                }

                // Clear the events to prevent re-publishing
                var clearMethod = domainEventsProperty.PropertyType.GetMethod("Clear");
                clearMethod?.Invoke(domainEventsProperty.GetValue(entity), null);
            }
        }

        return domainEvents;
    }
}