namespace JCP.TicketWave.BookingService.Domain.Models;

public class Booking
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal TotalAmount { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation property
    public List<Ticket> Tickets { get; private set; } = new();

    // Private constructor for EF Core
    private Booking() { }

    // Factory method for creating new bookings
    public static Booking Create(
        Guid eventId,
        Guid userId,
        string customerEmail,
        int quantity,
        decimal totalAmount,
        DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email is required", nameof(customerEmail));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(totalAmount));

        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            CustomerEmail = customerEmail,
            Quantity = quantity,
            TotalAmount = totalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(15) // 15 minutes default expiration
        };
    }

    // Business methods
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm booking in {Status} status");
        
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            throw new InvalidOperationException("Cannot confirm expired booking");

        Status = BookingStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason = "")
    {
        if (Status == BookingStatus.Cancelled)
            return; // Already cancelled

        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed booking");

        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Cannot complete booking in {Status} status");

        Status = BookingStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTicket(Ticket ticket)
    {
        if (ticket.BookingId != Id)
            throw new ArgumentException("Ticket booking ID does not match", nameof(ticket));

        Tickets.Add(ticket);
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}