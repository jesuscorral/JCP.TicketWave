using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.CatalogService.Domain.Models;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Configurations;

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

        builder.Property(e => e.AvailableTickets)
            .IsRequired();

        builder.Property(e => e.TicketPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.MaxPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.ExternalUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt);

        // Relationships
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

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