using Eventix.Booking.Domain.Availability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.Booking.Infrastructure.Persistence.Configurations;

public class EventAvailabilityConfiguration : IEntityTypeConfiguration<EventAvailability>
{
    public void Configure(EntityTypeBuilder<EventAvailability> builder)
    {
        builder.ToTable("EventAvailabilities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever(); // Id == EventId de EventCatalog

        builder.Property(a => a.TotalTickets).IsRequired();

        // Optimistic concurrency on stock: prevents oversell under concurrent reservations
        builder.Property(a => a.AvailableTickets)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Ignore(a => a.DomainEvents);
    }
}