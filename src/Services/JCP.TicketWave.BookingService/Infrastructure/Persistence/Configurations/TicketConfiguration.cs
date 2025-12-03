using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.BookingService.Domain.Models;

namespace JCP.TicketWave.BookingService.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(t => t.EventId)
            .HasColumnType("uniqueidentifier")
            .IsRequired();
            
        builder.Property(t => t.BookingId)
            .HasColumnType("uniqueidentifier");
            
        builder.Property(t => t.TicketType)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(t => t.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(t => t.SeatNumber)
            .HasMaxLength(20);
            
        builder.Property(t => t.ReservedUntil)
            .HasColumnType("datetime2");
            
        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime2");
        
        // Foreign key relationship configured in BookingConfiguration
        
        // Indexes for high-performance queries
        builder.HasIndex(t => t.EventId)
            .HasDatabaseName("IX_tickets_event_id");
            
        builder.HasIndex(t => t.BookingId)
            .HasDatabaseName("IX_tickets_booking_id")
            .HasFilter("booking_id IS NOT NULL");
            
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_tickets_status");
            
        builder.HasIndex(t => t.ReservedUntil)
            .HasDatabaseName("IX_tickets_reserved_until")
            .HasFilter("reserved_until IS NOT NULL");
            
        // Composite indexes for common query patterns
        builder.HasIndex(t => new { t.EventId, t.Status })
            .HasDatabaseName("IX_tickets_event_status");
            
        builder.HasIndex(t => new { t.EventId, t.TicketType, t.Status })
            .HasDatabaseName("IX_tickets_event_type_status");
            
        // Unique constraint for seat numbers per event (if seat is assigned)
        builder.HasIndex(t => new { t.EventId, t.SeatNumber })
            .HasDatabaseName("IX_tickets_event_seat_unique")
            .IsUnique()
            .HasFilter("seat_number IS NOT NULL");
    }
}