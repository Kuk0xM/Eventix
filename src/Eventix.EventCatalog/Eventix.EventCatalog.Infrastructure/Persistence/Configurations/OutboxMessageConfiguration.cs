using Eventix.EventCatalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.EventCatalog.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .HasColumnType("jsonb") // PostgreSQL native JSON type
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired();
    }
}