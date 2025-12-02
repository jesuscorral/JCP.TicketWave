using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Configurations;

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

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(v => v.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Complex type mapping for Address
        builder.OwnsOne(v => v.Address, address =>
        {
            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("AddressStreet");

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("AddressCity");

            address.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("AddressState");

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("AddressPostalCode");

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("AddressCountry");

            address.Property(a => a.Latitude)
                .HasColumnType("decimal(10,8)")
                .HasColumnName("AddressLatitude");

            address.Property(a => a.Longitude)
                .HasColumnType("decimal(11,8)")
                .HasColumnName("AddressLongitude");
        });

        // Complex type mapping for Capacity
        builder.OwnsOne(v => v.Capacity, capacity =>
        {
            capacity.Property(c => c.TotalCapacity)
                .IsRequired()
                .HasColumnName("CapacityTotal");

            capacity.Property(c => c.HasSeatedSections)
                .HasColumnName("CapacityHasSeatedSections");

            capacity.Property(c => c.HasStandingAreas)
                .HasColumnName("CapacityHasStandingAreas");

            capacity.Property(c => c.SectionCapacities)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, int>())
                .HasColumnType("nvarchar(max)")
                .HasColumnName("CapacitySections");
        });

        // Complex type mapping for collections
        builder.Property(v => v.Amenities)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.Images)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(v => v.Name)
            .HasDatabaseName("IX_Venues_Name");

        builder.HasIndex(v => new { v.TenantId, v.CityId })
            .HasDatabaseName("IX_Venues_TenantCity");

        builder.HasIndex(v => v.IsActive)
            .HasDatabaseName("IX_Venues_IsActive");
    }
}