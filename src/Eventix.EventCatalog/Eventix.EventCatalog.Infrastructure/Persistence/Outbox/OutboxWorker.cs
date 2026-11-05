using System.Text.Json;
using Eventix.EventCatalog.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background service that polls the OutboxMessages table and publishes
/// pending messages to RabbitMQ.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxWorker> _logger;

    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public OutboxWorker(IServiceProvider serviceProvider, ILogger<OutboxWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.Type);
                        if (eventType is null)
                        {
                            message.Error = $"Type {message.Type} not found.";
                            message.ProcessedOnUtc = DateTime.UtcNow;
                            continue;
                        }

                        var integrationEvent = JsonSerializer.Deserialize(message.Content, eventType);

                        if (integrationEvent is not null)
                        {
                            await publisher.PublishAsync(integrationEvent, stoppingToken);
                            message.ProcessedOnUtc = DateTime.UtcNow;

                            _logger.LogInformation(
                                "Published Outbox Message {MessageId} of type {Type} to RabbitMQ",
                                message.Id,
                                eventType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;
                        _logger.LogError(ex, "Error processing Outbox Message {MessageId}", message.Id);
                    }
                }

                if (messages.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Outbox Worker polling loop.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
}