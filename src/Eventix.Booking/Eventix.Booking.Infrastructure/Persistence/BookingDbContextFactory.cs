using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Eventix.Booking.Infrastructure.Persistence;

public class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=eventix_booking;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new BookingDbContext(options);
    }
}