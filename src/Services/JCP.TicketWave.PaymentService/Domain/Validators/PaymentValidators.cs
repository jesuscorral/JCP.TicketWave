using FluentValidation;

namespace JCP.TicketWave.PaymentService.Domain.Validators;

public class PaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public PaymentValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEqual(Guid.Empty)
            .WithMessage("Booking ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(100000)
            .WithMessage("Amount cannot exceed $100,000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR)")
            .Must(BeASupportedCurrency)
            .WithMessage("Currency must be one of: USD, EUR, GBP, CAD, AUD");

        RuleFor(x => x.PaymentMethodId)
            .NotEqual(Guid.Empty)
            .WithMessage("Payment method is required");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private static bool BeASupportedCurrency(string currency)
    {
        var supportedCurrencies = new[] { "USD", "EUR", "GBP", "CAD", "AUD" };
        return supportedCurrencies.Contains(currency.ToUpper());
    }
}

public class PaymentMethodValidator : AbstractValidator<CreatePaymentMethodRequest>
{
    public PaymentMethodValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required")
            .MaximumLength(100)
            .WithMessage("Display name cannot exceed 100 characters");

        When(x => x.Type == "CARD", () =>
        {
            RuleFor(x => x.Last4Digits)
                .NotEmpty()
                .WithMessage("Last 4 digits are required for card payments")
                .Length(4)
                .WithMessage("Last 4 digits must be exactly 4 characters")
                .Matches(@"^\d{4}$")
                .WithMessage("Last 4 digits must be numeric");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty()
                .WithMessage("Expiry month is required for card payments")
                .InclusiveBetween(1, 12)
                .WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty()
                .WithMessage("Expiry year is required for card payments")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("Card cannot be expired")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 20)
                .WithMessage("Expiry year seems invalid");

            RuleFor(x => x.Brand)
                .NotEmpty()
                .WithMessage("Card brand is required")
                .Must(BeASupportedCardBrand)
                .WithMessage("Card brand must be one of: Visa, Mastercard, Amex, Discover");
        });

        RuleFor(x => x.ExternalMethodId)
            .NotEmpty()
            .WithMessage("External method ID is required")
            .MaximumLength(100)
            .WithMessage("External method ID cannot exceed 100 characters");
    }

    private static bool BeASupportedCardBrand(string? brand)
    {
        if (string.IsNullOrEmpty(brand))
            return false;
        
        var supportedBrands = new[] { "Visa", "Mastercard", "Amex", "Discover" };
        return supportedBrands.Contains(brand, StringComparer.OrdinalIgnoreCase);
    }
}

public class RefundValidator : AbstractValidator<CreateRefundRequest>
{
    public RefundValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEqual(Guid.Empty)
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Refund amount must be greater than zero");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}

// Request models for validation
public record CreatePaymentRequest(
    Guid BookingId,
    decimal Amount,
    string Currency,
    Guid PaymentMethodId,
    string? Description = null);

public record CreatePaymentMethodRequest(
    Guid UserId,
    string Type,
    string DisplayName,
    string? Last4Digits,
    int? ExpiryMonth,
    int? ExpiryYear,
    string? Brand,
    string ExternalMethodId);

public record CreateRefundRequest(
    Guid PaymentId,
    decimal Amount,
    string Reason);