using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.PaymentService.Domain.Validators;
using JCP.TicketWave.PaymentService.Domain.Events;

namespace JCP.TicketWave.PaymentService.Domain.Models;

public class Payment : AggregateRoot
{
    public string TenantId { get; private set; } = "default";
    public Guid BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public string? ExternalPaymentId { get; private set; } // Stripe/PayPal ID
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public List<PaymentEvent> Events { get; private set; } = new();

    // Private constructor for EF Core
    private Payment() : base() { }

    // Private constructor for factory method
    private Payment(
        Guid bookingId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod,
        string tenantId) : base()
    {
        TenantId = tenantId;
        BookingId = bookingId;
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        Status = PaymentStatus.Pending;
        PaymentMethod = paymentMethod;
    }

    // Factory method
    public static Payment Create(
        Guid bookingId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod,
        string? tenantId = null)
    {
        // Create validation request
        var validationRequest = new CreatePaymentRequest(
            bookingId, amount, currency, paymentMethod.Id);

        // Validate using FluentValidation
        var validator = new PaymentValidator();
        var validationResult = validator.Validate(validationRequest);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new DomainException($"Payment validation failed: {errors}");
        }

        var payment = new Payment(
            bookingId,
            amount,
            currency,
            paymentMethod,
            tenantId ?? "default");

        payment.AddEvent("Payment created", PaymentEventType.Created);
        
        // Add domain event
        payment.AddDomainEvent(new PaymentInitiatedDomainEvent(payment.Id, bookingId, amount, currency, paymentMethod.Id, paymentMethod.Provider ?? string.Empty, DateTime.UtcNow));
        
        return payment;
    }

    public void MarkAsProcessing(string externalPaymentId)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot mark payment as processing. Current status: {Status}");

        Status = PaymentStatus.Processing;
        ExternalPaymentId = externalPaymentId;
        MarkAsModified();
        
        AddEvent($"Payment processing started with external ID: {externalPaymentId}", PaymentEventType.Processing);
        
        // Add domain event
        AddDomainEvent(new PaymentProcessedDomainEvent(Id, BookingId, Amount, Currency, externalPaymentId, PaymentMethod.Provider ?? string.Empty, DateTime.UtcNow));
    }

    public void MarkAsSucceeded()
    {
        if (Status != PaymentStatus.Processing)
            throw new DomainException($"Cannot mark payment as succeeded. Current status: {Status}");

        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        FailureReason = null;
        MarkAsModified();
        
        AddEvent("Payment succeeded", PaymentEventType.Succeeded);
        
        // Add domain events
        AddDomainEvent(new PaymentCompletedDomainEvent(Id, BookingId, Amount, Currency, ExternalPaymentId ?? string.Empty, DateTime.UtcNow));
        AddDomainEvent(new PaymentCompletedIntegrationEvent(Id, BookingId, Amount, Currency, ExternalPaymentId ?? string.Empty, DateTime.UtcNow));
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new DomainException("Failure reason is required");

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
        MarkAsModified();
        
        AddEvent($"Payment failed: {failureReason}", PaymentEventType.Failed);
        
        // Add domain events
        AddDomainEvent(new PaymentFailedDomainEvent(Id, BookingId, Amount, Currency, failureReason, "PAYMENT_FAILED", DateTime.UtcNow));
        AddDomainEvent(new PaymentFailedIntegrationEvent(Id, BookingId, Amount, Currency, failureReason, DateTime.UtcNow));
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Succeeded)
            throw new DomainException($"Cannot refund payment. Current status: {Status}");

        Status = PaymentStatus.Refunded;
        MarkAsModified();
        
        AddEvent("Payment refunded", PaymentEventType.Refunded);
    }

    private void AddEvent(string description, PaymentEventType eventType)
    {
        var paymentEvent = PaymentEvent.Create(Id, description, eventType);
        Events.Add(paymentEvent);
    }

    public bool CanBeRefunded => Status == PaymentStatus.Succeeded;
    public bool IsFinalized => Status is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Refunded;
}