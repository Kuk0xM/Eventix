using Eventix.Chassis.Core.Messaging;
using Eventix.Chassis.Infrastructure.Messaging;
using Eventix.Chassis.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RabbitMQ.Client;
using Serilog;

namespace Eventix.Chassis.Infrastructure;

/// <summary>
/// Extension methods for configuring the Eventix Microservice Chassis.
/// This is the ONE-LINE setup that each microservice uses in Program.cs:
/// 
/// <code>
/// builder.Services.AddEventixChassis&lt;BookingDbContext&gt;(builder.Configuration);
/// </code>
/// </summary>
public static class EventixChassisExtensions
{
    /// <summary>
    /// Configures the complete Eventix Chassis for a microservice:
    /// - EF Core with PostgreSQL + Outbox interceptor
    /// - Outbox worker for publishing messages
    /// - RabbitMQ with Polly resilience (retry + circuit breaker)
    /// </summary>
    public static IServiceCollection AddEventixChassis<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "default")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration");

        // 1. Configure Polly resilience pipeline (shared between RabbitMQ and EF Core)
        var resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnRetry = args =>
                {
                    Log.Warning(
                        "Retry {Attempt}/3 after {Delay}ms. Error: {Error}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    Log.Error(
                        "Circuit breaker OPENED after {Error}. Breaking for 30s.",
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    Log.Information("Circuit breaker CLOSED. Service recovered.");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        services.AddSingleton(resiliencePipeline);

        // 2. Configure EF Core with Outbox interceptor
        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

            // Register the Outbox interceptor so domain events are captured automatically
            var outboxInterceptor = new OutboxSaveChangesInterceptor();
            options.AddInterceptors(outboxInterceptor);
        });

        // 3. Register Outbox worker
        services.AddHostedService<OutboxWorker<TContext>>();

        // 4. Configure RabbitMQ
        ConfigureRabbitMq(services, configuration, resiliencePipeline);

        Log.Information(
            "Eventix Chassis configured for service '{ServiceName}'",
            serviceName);

        return services;
    }

    private static void ConfigureRabbitMq(
        IServiceCollection services,
        IConfiguration configuration,
        ResiliencePipeline resiliencePipeline)
    {
        var host = configuration["RabbitMq:Host"] ?? "localhost";
        var user = configuration["RabbitMq:User"] ?? "guest";
        var password = configuration["RabbitMq:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        // Singleton connection (shared across the app)
        services.AddSingleton<IConnection>(sp =>
        {
            return resiliencePipeline.Execute(() =>
            {
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });
        });

        // Singleton message bus
        services.AddSingleton(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var pipeline = sp.GetRequiredService<ResiliencePipeline>();
            return new RabbitMqMessageBus(connection, resiliencePipeline);
        });

        services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<RabbitMqMessageBus>());

        Log.Information("RabbitMQ configured for host: {Host}", host);
    }
}