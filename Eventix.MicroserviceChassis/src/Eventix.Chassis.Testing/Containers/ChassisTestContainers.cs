using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Eventix.Chassis.Testing.Containers;

/// <summary>
/// Manages Testcontainers for integration tests: real PostgreSQL and RabbitMQ in Docker.
/// </summary>
public sealed class ChassisTestContainers : IAsyncDisposable
{
    public PostgreSqlContainer Postgres { get; }
    public RabbitMqContainer RabbitMq { get; }

    public ChassisTestContainers()
    {
        Postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("eventix_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        RabbitMq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .Build();
    }

    public string PostgresConnectionString => Postgres.GetConnectionString();
    public string RabbitMqHost => RabbitMq.Hostname;
    public int RabbitMqPort => RabbitMq.GetMappedPublicPort(5672);
    public string RabbitMqUser => "guest";
    public string RabbitMqPassword => "guest";

    public async Task StartAsync()
    {
        await Task.WhenAll(Postgres.StartAsync(), RabbitMq.StartAsync());
    }

    public async Task StopAsync()
    {
        await Task.WhenAll(Postgres.StopAsync(), RabbitMq.StopAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await Postgres.DisposeAsync();
        await RabbitMq.DisposeAsync();
    }
}