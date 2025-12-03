using JCP.TicketWave.PaymentService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JCP.TicketWave.PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");
        
        builder.HasKey(pm => pm.Id);
        
        builder.Property(pm => pm.Id)
            .ValueGeneratedNever();
            
        builder.Property(pm => pm.TenantId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(pm => pm.UserId);
            
        builder.Property(pm => pm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(pm => pm.DisplayName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(pm => pm.Last4Digits)
            .HasMaxLength(4);
            
        builder.Property(pm => pm.ExpiryMonth)
            .HasMaxLength(2);
            
        builder.Property(pm => pm.ExpiryYear)
            .HasMaxLength(4);
            
        builder.Property(pm => pm.CardBrand)
            .HasMaxLength(50);
            
        builder.Property(pm => pm.ExternalMethodId)
            .HasMaxLength(200);
            
        builder.Property(pm => pm.IsDefault)
            .IsRequired();
            
        builder.Property(pm => pm.IsActive)
            .IsRequired();
            
        builder.Property(pm => pm.CreatedAt)
            .IsRequired();
            
        builder.Property(pm => pm.UpdatedAt);

        // Indexes
        builder.HasIndex(pm => pm.TenantId)
            .HasDatabaseName("IX_PaymentMethods_TenantId");
            
        builder.HasIndex(pm => pm.UserId)
            .HasDatabaseName("IX_PaymentMethods_UserId");
            
        builder.HasIndex(pm => pm.ExternalMethodId)
            .HasDatabaseName("IX_PaymentMethods_ExternalMethodId");
            
        builder.HasIndex(pm => new { pm.UserId, pm.IsDefault })
            .HasDatabaseName("IX_PaymentMethods_UserId_IsDefault");
    }
}