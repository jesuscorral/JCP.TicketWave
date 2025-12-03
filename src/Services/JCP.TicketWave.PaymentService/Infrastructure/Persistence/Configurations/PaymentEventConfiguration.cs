using JCP.TicketWave.PaymentService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JCP.TicketWave.PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("PaymentEvents");
        
        builder.HasKey(pe => pe.Id);
        
        builder.Property(pe => pe.Id)
            .ValueGeneratedNever();
            
        builder.Property(pe => pe.PaymentId)
            .IsRequired();
            
        builder.Property(pe => pe.Description)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(pe => pe.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(pe => pe.OccurredAt)
            .IsRequired();
            
        builder.Property(pe => pe.Metadata)
            .HasMaxLength(2000); // JSON data

        // Navigation properties configured in PaymentConfiguration
        
        // Indexes
        builder.HasIndex(pe => pe.PaymentId)
            .HasDatabaseName("IX_PaymentEvents_PaymentId");
            
        builder.HasIndex(pe => pe.EventType)
            .HasDatabaseName("IX_PaymentEvents_EventType");
            
        builder.HasIndex(pe => pe.OccurredAt)
            .HasDatabaseName("IX_PaymentEvents_OccurredAt");
    }
}