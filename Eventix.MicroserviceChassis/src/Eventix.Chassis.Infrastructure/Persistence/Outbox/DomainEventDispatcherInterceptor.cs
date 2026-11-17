using Eventix.Chassis.Core.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;

namespace Eventix.Chassis.Infrastructure.Persistence.Outbox;

/// <summary>
/// EF Core interceptor that dispatches domain events via MediatR BEFORE the
/// SaveChanges commit, so handlers can enqueue Outbox messages within the
/// SAME transaction.
/// 
/// Flow:
/// 1. SavingChangesAsync fires
/// 3. We publish each via MediatR (handlers may add OutboxMessages to the context)
/// 4. base.SavingChangesAsync commits everything atomically
/// </summary>
public sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var domainEvents = CollectAndClear(context);

        if (domainEvents.Count == 0)
        {
            return;
        }

        var mediator = context.GetService<IMediator>();
        if (mediator is null)
        {
            Log.Warning(
                "IMediator not available in DbContext services. {Count} domain event(s) will NOT be dispatched.",
                domainEvents.Count);
            return;
        }

        foreach (var domainEvent in domainEvents)
        {
            Log.Debug("Dispatching domain event {EventType}", domainEvent.GetType().Name);
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }

    private static List<IDomainEvent> CollectAndClear(DbContext context)
    {
        var collected = new List<IDomainEvent>();

        foreach (var entry in context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            if (entry.Entity is IHasDomainEvents holder && holder.DomainEvents.Count > 0)
            {
                collected.AddRange(holder.DomainEvents);
                holder.ClearDomainEvents();
            }
        }

        return collected;
    }
}