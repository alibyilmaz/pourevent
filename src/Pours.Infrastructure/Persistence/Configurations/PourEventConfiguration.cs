using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pours.Domain.Entities;

namespace Pours.Infrastructure.Persistence.Configurations;

public sealed class PourEventConfiguration : IEntityTypeConfiguration<PourEvent>
{
    public void Configure(EntityTypeBuilder<PourEvent> builder)
    {
        builder.ToTable("Pours");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(p => p.EventId)
            .IsRequired();

        builder.Property(p => p.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LocationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ProductId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.StartedAt)
            .IsRequired();

        builder.Property(p => p.EndedAt)
            .IsRequired();

        builder.Property(p => p.VolumeMl)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");

        // Unique index on EventId for idempotency
        builder.HasIndex(p => p.EventId)
            .IsUnique()
            .HasDatabaseName("IX_Pours_EventId");

        // Index on DeviceId
        builder.HasIndex(p => p.DeviceId)
            .HasDatabaseName("IX_Pours_DeviceId");

        // Composite index on DeviceId + StartedAt
        builder.HasIndex(p => new { p.DeviceId, p.StartedAt })
            .HasDatabaseName("IX_Pours_DeviceId_StartedAt");

        // Covering index for summary queries (includes all columns needed for aggregation)
        builder.HasIndex(p => new { p.DeviceId, p.StartedAt, p.ProductId, p.LocationId, p.VolumeMl })
            .HasDatabaseName("IX_Pours_Summary_Covering");

        // Index on ProductId
        builder.HasIndex(p => p.ProductId)
            .HasDatabaseName("IX_Pours_ProductId");

        // Index on LocationId
        builder.HasIndex(p => p.LocationId)
            .HasDatabaseName("IX_Pours_LocationId");
    }
}
