using Eventix.Booking.Domain.Availability;
using Eventix.Booking.Domain.Reservations;
using Eventix.Booking.Infrastructure.Persistence;
using Eventix.Chassis.Application;
using Eventix.Chassis.Infrastructure;
using Eventix.Chassis.Testing.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.Booking.Integration.Tests;

public class ReservationFlowTests : IAsyncLifetime
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
        services.AddEventixChassis<BookingDbContext>(config, "booking-test");
        services.AddEventixApplication(typeof(Eventix.Booking.Application.DependencyInjection).Assembly);
        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _containers.DisposeAsync();
    }

    [Fact]
    public async Task Reserve_PersistsStockDecrement()
    {
        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var eventId = Guid.NewGuid();
        ctx.EventAvailabilities.Add(EventAvailability.Create(eventId, 100));
        await ctx.SaveChangesAsync();

        var availability = await ctx.EventAvailabilities.FirstAsync(a => a.Id == eventId);
        availability.Reserve(10);
        await ctx.SaveChangesAsync();

        var reloaded = await ctx.EventAvailabilities.AsNoTracking().FirstAsync(a => a.Id == eventId);
        reloaded.AvailableTickets.Should().Be(90);
    }

    [Fact]
    public async Task SaveChanges_WithReservation_WritesOutboxRow()
    {
        using var scope = _provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        ctx.Reservations.Add(Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2));
        await ctx.SaveChangesAsync();

        var rows = await ctx.OutboxMessages.ToListAsync();
        rows.Should().ContainSingle();
    }
}