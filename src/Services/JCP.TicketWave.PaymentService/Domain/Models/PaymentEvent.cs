using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.PaymentService.Domain.Models;

public class PaymentEvent : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public PaymentEventType EventType { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional data

    // Navigation property
    public Payment Payment { get; private set; } = null!;

    // Private constructor for EF Core
    private PaymentEvent() : base() { }

    // Private constructor for factory method
    private PaymentEvent(
        Guid paymentId,
        string description,
        PaymentEventType eventType,
        string? metadata) : base()
    {
        if (paymentId == Guid.Empty)
            throw new DomainException("Payment ID is required");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required");

        PaymentId = paymentId;
        Description = description;
        EventType = eventType;
        OccurredAt = DateTime.UtcNow;
        Metadata = metadata;
    }

    // Factory method
    public static PaymentEvent Create(
        Guid paymentId,
        string description,
        PaymentEventType eventType,
        string? metadata = null)
    {
        return new PaymentEvent(paymentId, description, eventType, metadata);
    }
}