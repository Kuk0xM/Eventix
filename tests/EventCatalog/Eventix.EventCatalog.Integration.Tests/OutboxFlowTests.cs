using Eventix.Chassis.Infrastructure;
using Eventix.Chassis.Testing.Containers;
using Eventix.EventCatalog.Domain.Events;
using Eventix.EventCatalog.Infrastructure.Persistence;
using Eventix.Chassis.Application;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.EventCatalog.Integration.Tests;

public class OutboxFlowTests : IAsyncLifetime
{
    private ChassisTestContainers _containers = null!;
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        _containers = new ChassisTestContainers();
        await _containers.StartAsync();

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _containers.PostgresConnectionString,
            ["RabbitMq:Host"] = _containers.RabbitMqHost,
            ["RabbitMq:Port"] = _containers.RabbitMqPort.ToString(),
            ["RabbitMq:User"] = _containers.RabbitMqUser,
            ["RabbitMq:Password"] = _containers.RabbitMqPassword
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventixChassis<EventCatalogDbContext>(config, "eventcatalog-test");
        services.AddEventixApplication(typeof(Eventix.EventCatalog.Application.DependencyInjection).Assembly);
        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _containers.DisposeAsync();
    }

    [Fact]
    public async Task SaveChanges_WithNewEvent_WritesOutboxRowInSameTransaction()
    {
        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();

        var @event = Event.Create("Integration Test", EventDate.Create(DateTime.UtcNow.AddDays(30)), EventDate.Create(DateTime.UtcNow.AddDays(31)), Guid.NewGuid());
        ctx.Events.Add(@event);
        await ctx.SaveChangesAsync();

        var rows = await ctx.OutboxMessages.ToListAsync();
        rows.Should().ContainSingle();
        rows[0].ProcessedOnUtc.Should().BeNull(); // not published yet (worker not running in test)
    }

    [Fact]
    public async Task SaveChanges_WithoutDomainEvents_WritesNoOutboxRow()
    {
        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();

        ctx.Venues.Add(new Eventix.EventCatalog.Domain.Venues.Venue(Guid.NewGuid(), "Hall", 50, null!));
        await ctx.SaveChangesAsync();

        var rows = await ctx.OutboxMessages.ToListAsync();
        rows.Should().BeEmpty();
    }
}