using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;

namespace Eventix.EventCatalog.Infrastructure.Messaging;

/// <summary>
/// Publishes integration events to RabbitMQ using a Fanout exchange.
/// Uses ResiliencePipeline from Microsoft.Extensions.Resilience for retry,
/// circuit breaker, and timeout policies.
/// </summary>
public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ILogger<RabbitMqMessagePublisher> _logger;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ResiliencePipeline _resiliencePipeline;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqMessagePublisher(
        ILogger<RabbitMqMessagePublisher> logger,
        IConnectionFactory connectionFactory,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;

        // Get the named resilience pipeline from DI
        _resiliencePipeline = pipelineProvider.GetPipeline(RabbitMqResilienceConfiguration.PipelineName);
    }

    public async Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default)
    {
        // Wrap the publish operation with the resilience pipeline
        await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            await PublishInternalAsync(integrationEvent, ct);
        }, cancellationToken);
    }

    private async Task PublishInternalAsync(object integrationEvent, CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);

        var eventType = integrationEvent.GetType();
        var exchangeName = GetExchangeName(eventType);
        var messageBody = JsonSerializer.Serialize(integrationEvent, eventType);
        var bodyBytes = Encoding.UTF8.GetBytes(messageBody);

        await _channel!.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
            Type = eventType.AssemblyQualifiedName,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            MessageId = Guid.NewGuid().ToString()
        };

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: true,
            basicProperties: properties,
            body: bodyBytes,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published message to exchange {Exchange}. MessageId: {MessageId}",
            exchangeName,
            properties.MessageId);
    }

    private static string GetExchangeName(Type eventType)
    {
        var name = eventType.Name.Replace("IntegrationEvent", "");
        var kebabCase = string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? $"-{char.ToLower(c)}" : char.ToLower(c).ToString()));

        return $"eventix.catalog.{kebabCase}";
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
                return;

            _logger.LogInformation("Connecting to RabbitMQ...");

            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Connected to RabbitMQ successfully.");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}