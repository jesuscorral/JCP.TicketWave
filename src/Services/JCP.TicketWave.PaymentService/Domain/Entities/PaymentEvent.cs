namespace JCP.TicketWave.PaymentService.Domain.Entities;

public class PaymentEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PaymentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public PaymentEventType EventType { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional data

    // Navigation property
    public Payment Payment { get; private set; } = null!;

    // Private constructor for EF Core
    private PaymentEvent() { }

    // Factory method
    public static PaymentEvent Create(
        Guid paymentId,
        string description,
        PaymentEventType eventType,
        string? metadata = null)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Payment ID is required", nameof(paymentId));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new PaymentEvent
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Description = description,
            EventType = eventType,
            OccurredAt = DateTime.UtcNow,
            Metadata = metadata
        };
    }
}