using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.CatalogService.Domain.Models;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        // Table configuration
        builder.ToTable("Venues", "catalog");

        // Primary key
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        // Properties
        builder.Property(v => v.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.CityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Description)
            .HasMaxLength(2000);

        builder.Property(v => v.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.PostalCode)
            .HasMaxLength(20);

        builder.Property(v => v.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(v => v.Email)
            .HasMaxLength(200);

        builder.Property(v => v.Website)
            .HasMaxLength(500);

        builder.Property(v => v.Capacity)
            .IsRequired();

        builder.Property(v => v.Latitude)
            .HasColumnType("decimal(10,8)");

        builder.Property(v => v.Longitude)
            .HasColumnType("decimal(11,8)");

        builder.Property(v => v.ImageUrl)
            .HasMaxLength(500);

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(v => v.UpdatedAt);

        // Relationships
        builder.HasMany(v => v.Events)
            .WithOne(e => e.Venue)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(v => v.TenantId)
            .HasDatabaseName("IX_Venues_TenantId");

        builder.HasIndex(v => v.CityId)
            .HasDatabaseName("IX_Venues_CityId");

        builder.HasIndex(v => v.Name)
            .HasDatabaseName("IX_Venues_Name");

        builder.HasIndex(v => v.IsActive)
            .HasDatabaseName("IX_Venues_IsActive");

        builder.HasIndex(v => new { v.TenantId, v.CityId })
            .HasDatabaseName("IX_Venues_TenantCity");
    }
}