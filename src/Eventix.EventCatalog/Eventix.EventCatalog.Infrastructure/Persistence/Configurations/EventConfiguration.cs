using Eventix.EventCatalog.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Event aggregate root.
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .HasConversion(
                eventDate => eventDate.Value,
                dateTime => EventDate.Create(dateTime))
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasConversion(
                eventDate => eventDate.Value,
                dateTime => EventDate.Create(dateTime))
            .IsRequired();

        builder.Property(e => e.VenueId)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Ignore domain events - they are not persisted
        builder.Ignore(e => e.DomainEvents);
    }
}