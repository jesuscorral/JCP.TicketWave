using JCP.TicketWave.Shared.Infrastructure.Domain;

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
        if (bookingId == Guid.Empty)
            throw new DomainException("Booking ID is required");
        
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero");
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");
        
        if (paymentMethod == null)
            throw new DomainException("Payment method is required");

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
        var payment = new Payment(
            bookingId,
            amount,
            currency,
            paymentMethod,
            tenantId ?? "default");

        payment.AddEvent("Payment created", PaymentEventType.Created);
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