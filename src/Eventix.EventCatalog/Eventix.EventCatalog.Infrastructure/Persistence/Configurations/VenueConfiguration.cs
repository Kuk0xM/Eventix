using Eventix.EventCatalog.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Venue entity.
/// </summary>
public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Capacity)
            .IsRequired();

        // OwnsOne maps the VenueLocation Value Object as columns in the same table.
        // This creates Venue_Location_City and Venue_Location_Country columns.
        builder.OwnsOne(v => v.Location, locationBuilder =>
        {
            locationBuilder.Property(l => l.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            locationBuilder.Property(l => l.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}