using Eventix.Booking.Domain.Availability;
using Eventix.Booking.Domain.Reservations;
using Eventix.Chassis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Booking.Infrastructure.Persistence;

public class BookingDbContext : ChassisDbContext<BookingDbContext>
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<EventAvailability> EventAvailabilities => Set<EventAvailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}