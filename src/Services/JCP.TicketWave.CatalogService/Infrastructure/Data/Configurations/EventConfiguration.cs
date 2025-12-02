using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // Table configuration
        builder.ToTable("Events", "catalog");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Properties
        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.StartDateTime)
            .IsRequired();

        builder.Property(e => e.EndDateTime)
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .IsRequired();

        builder.Property(e => e.VenueId)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        // Complex type mapping for collections
        builder.Property(e => e.TicketTypes)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<TicketTypeInfo>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<TicketTypeInfo>())
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Images)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Tags)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("IX_Events_CategoryId");

        builder.HasIndex(e => e.VenueId)
            .HasDatabaseName("IX_Events_VenueId");

        builder.HasIndex(e => e.StartDateTime)
            .HasDatabaseName("IX_Events_StartDateTime");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Events_Status");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_Events_CreatedAt");

        builder.HasIndex(e => new { e.TenantId, e.CategoryId })
            .HasDatabaseName("IX_Events_TenantCategory");
    }
}