using Eventix.Booking.Infrastructure.Persistence;
using Eventix.Chassis.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ONE LINE: EF Core + PostgreSQL + Outbox + RabbitMQ + Polly
        services.AddEventixChassis<BookingDbContext>(configuration, "booking");

        return services;
    }
}