using System.Text.Json;
using Eventix.Chassis.Core.Messaging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RabbitMQ.Client;
using Serilog;

namespace Eventix.Chassis.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IMessageBus with built-in resilience.
/// 
/// Resilience features:
/// - Automatic retries with exponential backoff (3 attempts)
/// - Circuit breaker opens after 50% failure rate over 10 seconds
/// - Dead letter routing on permanent failures (future enhancement)
/// </summary>
public sealed class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly string _exchangePrefix;
    private IChannel? _channel;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public RabbitMqMessageBus(
        IConnection connection,
        ResiliencePipeline resiliencePipeline,
        string exchangePrefix = "eventix")
    {
        _connection = connection;
        _resiliencePipeline = resiliencePipeline;
        _exchangePrefix = exchangePrefix;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            var channel = await GetOrCreateChannelAsync(ct);

            var exchangeName = BuildExchangeName(@event);

            // Declare the exchange (idempotent - safe to call multiple times)
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: ct);

            var body = JsonSerializer.SerializeToUtf8Bytes(@event);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = @event.EventId.ToString(),
                Timestamp = new AmqpTimestamp(new DateTimeOffset(@event.OccurredAt).ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            Log.Debug(
                "Published {EventType} ({EventId}) to exchange {Exchange}",
                @event.EventType,
                @event.EventId,
                exchangeName);
        }, cancellationToken);
    }

    public async Task PublishManyAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    private string BuildExchangeName<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        // Convention: "eventix.{service}.{event-type}"
        // Example: "eventix.eventcatalog.event-created"
        var eventType = @event.EventType;
        return $"{_exchangePrefix}.{eventType}".ToLowerInvariant().Replace("integration", "");
    }

    private async Task<IChannel> GetOrCreateChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _channelLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel?.Dispose();
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            return _channel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        _channelLock.Dispose();
    }
}