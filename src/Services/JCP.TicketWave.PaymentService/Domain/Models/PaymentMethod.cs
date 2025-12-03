using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.PaymentService.Domain.Models;

public class PaymentMethod : BaseEntity
{
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

    // Private constructor for EF Core
    private PaymentMethod() : base() { }

    // Private constructor for card
    private PaymentMethod(
        string tenantId,
        Guid? userId,
        string displayName,
        string last4Digits,
        string expiryMonth,
        string expiryYear,
        string cardBrand,
        string? externalMethodId) : base()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required");
        
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
            throw new DomainException("Last 4 digits must be exactly 4 characters");
        
        if (string.IsNullOrWhiteSpace(expiryMonth))
            throw new DomainException("Expiry month is required");
        
        if (string.IsNullOrWhiteSpace(expiryYear))
            throw new DomainException("Expiry year is required");

        TenantId = tenantId;
        UserId = userId;
        Type = PaymentMethodType.Card;
        DisplayName = displayName;
        Last4Digits = last4Digits;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CardBrand = cardBrand;
        ExternalMethodId = externalMethodId;
        IsActive = true;
    }

    // Private constructor for digital wallet
    private PaymentMethod(
        string tenantId,
        Guid? userId,
        PaymentMethodType type,
        string displayName,
        string? externalMethodId) : base()
    {
        if (type == PaymentMethodType.Card)
            throw new DomainException("Use CreateCard method for card payment methods");
        
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required");

        TenantId = tenantId;
        UserId = userId;
        Type = type;
        DisplayName = displayName;
        ExternalMethodId = externalMethodId;
        IsActive = true;
    }

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
        return new PaymentMethod(
            tenantId ?? "default",
            userId,
            displayName,
            last4Digits,
            expiryMonth,
            expiryYear,
            cardBrand,
            externalMethodId);
    }

    // Factory method for digital wallet (PayPal, Apple Pay, etc.)
    public static PaymentMethod CreateDigitalWallet(
        PaymentMethodType type,
        string displayName,
        string? externalMethodId = null,
        Guid? userId = null,
        string? tenantId = null)
    {
        return new PaymentMethod(
            tenantId ?? "default",
            userId,
            type,
            displayName,
            externalMethodId);
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdateTimestamp();
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
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