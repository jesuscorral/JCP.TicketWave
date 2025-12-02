using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Infrastructure.Data.Configurations;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedNever();
            
        builder.Property(r => r.PaymentId)
            .IsRequired();
            
        builder.Property(r => r.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,4)");
            
        builder.Property(r => r.Currency)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(r => r.ExternalRefundId)
            .HasMaxLength(200);
            
        builder.Property(r => r.FailureReason)
            .HasMaxLength(1000);
            
        builder.Property(r => r.CreatedAt)
            .IsRequired();
            
        builder.Property(r => r.ProcessedAt);
        builder.Property(r => r.UpdatedAt);

        // Navigation properties
        builder.HasOne(r => r.Payment)
            .WithMany()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.PaymentId)
            .HasDatabaseName("IX_Refunds_PaymentId");
            
        builder.HasIndex(r => r.ExternalRefundId)
            .HasDatabaseName("IX_Refunds_ExternalRefundId");
            
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Refunds_Status");
            
        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_Refunds_CreatedAt");
    }
}