using Eventix.Chassis.Infrastructure;
using Eventix.Chassis.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eventix.Chassis.Testing.Containers;

/// <summary>
/// Base fixture for integration tests. Manages Testcontainers lifecycle
/// and configures the Chassis for testing.
/// </summary>
public class IntegrationTestFixture<TContext> : IAsyncLifetime
    where TContext : ChassisDbContext<TContext>
{
    private ChassisTestContainers? _containers;
    private ServiceProvider? _serviceProvider;

    public IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("Fixture not initialized");

    public string PostgresConnectionString =>
        _containers?.PostgresConnectionString ?? throw new InvalidOperationException("Fixture not initialized");

    public async Task InitializeAsync()
    {
        _containers = new ChassisTestContainers();
        await _containers.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _containers.PostgresConnectionString,
                ["RabbitMq:Host"] = _containers.RabbitMqHost,
                ["RabbitMq:User"] = _containers.RabbitMqUser,
                ["RabbitMq:Password"] = _containers.RabbitMqPassword
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddEventixChassis<TContext>(configuration, "test-service");

        _serviceProvider = services.BuildServiceProvider();

        // Create the schema directly from the model (no migrations needed for tests)
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        if (_containers is not null)
        {
            await _containers.DisposeAsync();
        }
    }
}