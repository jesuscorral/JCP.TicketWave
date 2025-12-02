namespace JCP.TicketWave.PaymentService.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = "default";
    public Guid? UserId { get; private set; } // Optional: for registered users
    public PaymentMethodType Type { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Last4Digits { get; private set; } // For cards
    public string? ExpiryMonth { get; private set; } // For cards
    public string? ExpiryYear { get; private set; } // For cards
    public string? CardBrand { get; private set; } // Visa, MasterCard, etc.
    public string? ExternalMethodId { get; private set; } // Stripe payment method ID
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private PaymentMethod() { }

    // Factory method for card payment method
    public static PaymentMethod CreateCard(
        string displayName,
        string last4Digits,
        string expiryMonth,
        string expiryYear,
        string cardBrand,
        string? externalMethodId = null,
        Guid? userId = null,
        string? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
        
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
            throw new ArgumentException("Last 4 digits must be exactly 4 characters", nameof(last4Digits));
        
        if (string.IsNullOrWhiteSpace(expiryMonth))
            throw new ArgumentException("Expiry month is required", nameof(expiryMonth));
        
        if (string.IsNullOrWhiteSpace(expiryYear))
            throw new ArgumentException("Expiry year is required", nameof(expiryYear));

        return new PaymentMethod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            UserId = userId,
            Type = PaymentMethodType.Card,
            DisplayName = displayName,
            Last4Digits = last4Digits,
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            CardBrand = cardBrand,
            ExternalMethodId = externalMethodId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Factory method for digital wallet (PayPal, Apple Pay, etc.)
    public static PaymentMethod CreateDigitalWallet(
        PaymentMethodType type,
        string displayName,
        string? externalMethodId = null,
        Guid? userId = null,
        string? tenantId = null)
    {
        if (type == PaymentMethodType.Card)
            throw new ArgumentException("Use CreateCard method for card payment methods", nameof(type));
        
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        return new PaymentMethod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            UserId = userId,
            Type = type,
            DisplayName = displayName,
            ExternalMethodId = externalMethodId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetMaskedDisplayName()
    {
        return Type switch
        {
            PaymentMethodType.Card when !string.IsNullOrEmpty(Last4Digits) => $"{CardBrand} ending in {Last4Digits}",
            _ => DisplayName
        };
    }
}