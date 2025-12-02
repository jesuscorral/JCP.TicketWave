namespace JCP.TicketWave.PaymentService.Domain.Entities;

public class Refund
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public string? ExternalRefundId { get; private set; } // Stripe/PayPal refund ID
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    public Payment Payment { get; private set; } = null!;

    // Private constructor for EF Core
    private Refund() { }

    // Factory method
    public static Refund Create(
        Guid paymentId,
        decimal amount,
        string currency,
        string reason)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Payment ID is required", nameof(paymentId));
        
        if (amount <= 0)
            throw new ArgumentException("Refund amount must be greater than zero", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Refund reason is required", nameof(reason));

        return new Refund
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Reason = reason,
            Status = RefundStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing(string externalRefundId)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot mark refund as processing. Current status: {Status}");

        Status = RefundStatus.Processing;
        ExternalRefundId = externalRefundId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSucceeded()
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException($"Cannot mark refund as succeeded. Current status: {Status}");

        Status = RefundStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        FailureReason = null;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required", nameof(failureReason));

        Status = RefundStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsFinalized => Status is RefundStatus.Succeeded or RefundStatus.Failed;
}

public enum RefundStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3
}