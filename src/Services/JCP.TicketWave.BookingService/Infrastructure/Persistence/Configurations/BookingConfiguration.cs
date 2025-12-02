using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.BookingService.Domain.Models;

namespace JCP.TicketWave.BookingService.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Id)
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(b => b.EventId)
            .HasColumnType("uniqueidentifier")
            .IsRequired();
            
        builder.Property(b => b.UserId)
            .HasColumnType("uniqueidentifier")
            .IsRequired();
            
        builder.Property(b => b.CustomerEmail)
            .HasMaxLength(320)
            .IsRequired();
            
        builder.Property(b => b.Quantity)
            .IsRequired();
            
        builder.Property(b => b.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(b => b.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(b => b.ExpiresAt)
            .HasColumnType("datetime2");
            
        builder.Property(b => b.UpdatedAt)
            .HasColumnType("datetime2");
            
        // Configure navigation property
        builder.HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes for high-performance queries
        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_bookings_user_id");
            
        builder.HasIndex(b => b.EventId)
            .HasDatabaseName("IX_bookings_event_id");
            
        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_bookings_status");
            
        builder.HasIndex(b => b.CreatedAt)
            .HasDatabaseName("IX_bookings_created_at");
            
        builder.HasIndex(b => b.ExpiresAt)
            .HasDatabaseName("IX_bookings_expires_at")
            .HasFilter("expires_at IS NOT NULL");
            
        // Composite indexes for common query patterns
        builder.HasIndex(b => new { b.UserId, b.Status })
            .HasDatabaseName("IX_bookings_user_status");
            
        builder.HasIndex(b => new { b.EventId, b.Status })
            .HasDatabaseName("IX_bookings_event_status");
    }
}