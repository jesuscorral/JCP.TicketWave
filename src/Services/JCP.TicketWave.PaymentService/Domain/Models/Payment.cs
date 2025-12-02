using System.ComponentModel.DataAnnotations;

namespace JCP.TicketWave.PaymentService.Domain.Models;

public class Payment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = "default";
    public Guid BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public string? ExternalPaymentId { get; private set; } // Stripe/PayPal ID
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public List<PaymentEvent> Events { get; private set; } = new();

    // Private constructor for EF Core
    private Payment() { }

    // Factory method
    public static Payment Create(
        Guid bookingId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod,
        string? tenantId = null)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID is required", nameof(bookingId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            BookingId = bookingId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            PaymentMethod = paymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        payment.AddEvent("Payment created", PaymentEventType.Created);
        return payment;
    }

    public void MarkAsProcessing(string externalPaymentId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as processing. Current status: {Status}");

        Status = PaymentStatus.Processing;
        ExternalPaymentId = externalPaymentId;
        UpdatedAt = DateTime.UtcNow;
        
        AddEvent($"Payment processing started with external ID: {externalPaymentId}", PaymentEventType.Processing);
    }

    public void MarkAsSucceeded()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot mark payment as succeeded. Current status: {Status}");

        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        FailureReason = null;
        
        AddEvent("Payment succeeded", PaymentEventType.Succeeded);
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required", nameof(failureReason));

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddEvent($"Payment failed: {failureReason}", PaymentEventType.Failed);
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Succeeded)
            throw new InvalidOperationException($"Cannot refund payment. Current status: {Status}");

        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
        
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