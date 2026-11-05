using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;

namespace Eventix.EventCatalog.Infrastructure.Messaging;

/// <summary>
/// Configures resilience pipelines for RabbitMQ operations using
/// </summary>
public static class RabbitMqResilienceConfiguration
{
    public const string PipelineName = "rabbitmq-publisher";

    public static IServiceCollection AddRabbitMqResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(PipelineName, builder =>
        {
            // 1. Retry: up to 3 attempts with exponential backoff + jitter
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex =>
                    ex is BrokerUnreachableException ||
                    ex is TimeoutException ||
                    ex is IOException),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(500),
                UseJitter = true
            });

            // 2. Circuit Breaker: open after 50% failure rate over 30s window
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            });

            // 3. Timeout: 5 seconds max per operation
            builder.AddTimeout(TimeSpan.FromSeconds(5));
        });

        return services;
    }
}