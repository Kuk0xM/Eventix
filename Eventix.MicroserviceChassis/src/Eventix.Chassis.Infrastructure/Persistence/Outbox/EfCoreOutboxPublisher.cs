using System.Text.Json;
using Eventix.Chassis.Core.Messaging;
using Eventix.Chassis.Core.Persistence.Outbox;

namespace Eventix.Chassis.Infrastructure.Persistence.Outbox;

/// <summary>
/// Generic IOutboxPublisher backed by EF Core.
/// Writes the OutboxMessage into the current DbContext change tracker,
/// so it commits atomically with the business entities.
/// </summary>
public sealed class EfCoreOutboxPublisher<TContext> : IOutboxPublisher
    where TContext : ChassisDbContext<TContext>
{
    private readonly TContext _dbContext;

    public EfCoreOutboxPublisher(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var message = new OutboxMessage
        {
            Id = @event.EventId,
            Type = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name,
            Content = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            OccurredOnUtc = @event.OccurredAt
        };

        _dbContext.OutboxMessages.Add(message);

        Serilog.Log.Debug(
            "Enqueued {EventType} ({EventId}) into Outbox",
            typeof(TEvent).Name,
            @event.EventId);

        return Task.CompletedTask;
    }
}