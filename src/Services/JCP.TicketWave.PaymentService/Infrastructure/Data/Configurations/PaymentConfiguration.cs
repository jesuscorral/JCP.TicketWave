using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
            
        builder.Property(p => p.TenantId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.BookingId)
            .IsRequired();
            
        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,4)");
            
        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(p => p.ExternalPaymentId)
            .HasMaxLength(200);
            
        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);
            
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        builder.Property(p => p.ProcessedAt);
        builder.Property(p => p.UpdatedAt);

        // Navigation properties
        builder.HasOne(p => p.PaymentMethod)
            .WithMany()
            .HasForeignKey("PaymentMethodId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Events)
            .WithOne(e => e.Payment)
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("IX_Payments_TenantId");
            
        builder.HasIndex(p => p.BookingId)
            .IsUnique()
            .HasDatabaseName("IX_Payments_BookingId");
            
        builder.HasIndex(p => p.ExternalPaymentId)
            .HasDatabaseName("IX_Payments_ExternalPaymentId");
            
        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");
            
        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Payments_CreatedAt");

        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("IX_Payments_TenantId_Status");
    }
}