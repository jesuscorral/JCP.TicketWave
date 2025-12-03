using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.CatalogService.Domain.Models;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table configuration
        builder.ToTable("Categories", "catalog");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        // Properties
        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.Color)
            .HasMaxLength(7) // HEX color code
            .HasDefaultValue("#007ACC");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique()
            .HasDatabaseName("IX_Categories_TenantName");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");

        builder.HasIndex(c => c.DisplayOrder)
            .HasDatabaseName("IX_Categories_DisplayOrder");
    }
}