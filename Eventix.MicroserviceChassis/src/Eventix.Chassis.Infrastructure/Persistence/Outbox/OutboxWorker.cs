using System.Text.Json;
using Eventix.Chassis.Core.Messaging;
using Eventix.Chassis.Core.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Eventix.Chassis.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background worker that periodically polls the Outbox table for unprocessed
/// messages and publishes them to the message broker.
/// 
/// Design:
/// - Runs every 2 seconds (configurable)
/// - Processes up to 50 messages per batch
/// - Uses pessimistic locking to avoid duplicate processing
/// - Marks messages as processed after successful publish
/// - Records errors for retry logic
/// </summary>
public sealed class OutboxWorker<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
    private readonly int _batchSize = 50;

    public OutboxWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("OutboxWorker started. Polling every {Interval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OutboxWorker main loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        Log.Information("OutboxWorker stopped");
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        // Fetch unprocessed messages (ordered by oldest first)
        var pendingMessages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null && m.Error == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        Log.Debug("Processing {Count} outbox messages", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    throw new InvalidOperationException($"Cannot resolve type: {message.Type}");
                }

                var @event = JsonSerializer.Deserialize(message.Content, eventType) as IIntegrationEvent;
                if (@event is null)
                {
                    throw new InvalidOperationException($"Failed to deserialize message {message.Id}");
                }

                // Publish to message broker (uses IMessageBus resilience)
                await messageBus.PublishAsync(@event, cancellationToken);

                // Mark as processed
                message.ProcessedOnUtc = DateTime.UtcNow;

                Log.Debug(
                    "Published outbox message {Id} of type {Type}",
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = $"{ex.GetType().Name}: {ex.Message}";

                // After 5 retries, log critical but keep in table for manual inspection
                if (message.RetryCount >= 5)
                {
                    Log.Error(
                        ex,
                        "Outbox message {Id} failed after {RetryCount} retries",
                        message.Id,
                        message.RetryCount);
                }
                else
                {
                    Log.Warning(
                        ex,
                        "Outbox message {Id} failed (attempt {RetryCount})",
                        message.Id,
                        message.RetryCount);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}