using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.BookingService.Domain.Events;

namespace JCP.TicketWave.BookingService.Domain.Models;

public class Booking : AggregateRoot
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal TotalAmount { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    
    // Navigation property
    public List<Ticket> Tickets { get; private set; } = new();

    // Private constructor for EF Core
    private Booking() : base() { }

    // Private constructor for factory method
    private Booking(
        Guid eventId,
        Guid userId,
        string customerEmail,
        int quantity,
        decimal totalAmount,
        DateTime? expiresAt) : base()
    {
        EventId = eventId;
        UserId = userId;
        CustomerEmail = customerEmail;
        Quantity = quantity;
        TotalAmount = totalAmount;
        Status = BookingStatus.Pending;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(15);
    }

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
            throw new DomainException("Customer email is required");
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
        
        if (totalAmount < 0)
            throw new DomainException("Total amount cannot be negative");

        var booking = new Booking(eventId, userId, customerEmail, quantity, totalAmount, expiresAt);
        
        // Add domain event
        booking.AddDomainEvent(new BookingCreatedDomainEvent(booking.Id, eventId, userId, quantity, totalAmount));
        
        return booking;
    }

    // Business methods
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException($"Cannot confirm booking in {Status} status");
        
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            throw new DomainException("Cannot confirm expired booking");

        Status = BookingStatus.Confirmed;
        MarkAsModified();
        
        AddDomainEvent(new BookingConfirmedDomainEvent(Id, EventId, UserId));
    }

    public void Cancel(string reason = "")
    {
        if (Status == BookingStatus.Cancelled)
            return; // Already cancelled

        if (Status == BookingStatus.Completed)
            throw new DomainException("Cannot cancel completed booking");

        Status = BookingStatus.Cancelled;
        MarkAsModified();
        
        AddDomainEvent(new BookingCancelledDomainEvent(Id, EventId, reason));
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new DomainException($"Cannot complete booking in {Status} status");

        Status = BookingStatus.Completed;
        MarkAsModified();
        
        AddDomainEvent(new BookingCompletedDomainEvent(Id, EventId, TotalAmount));
    }

    public void AddTicket(Ticket ticket)
    {
        if (ticket.BookingId != Id)
            throw new DomainException("Ticket booking ID does not match");

        Tickets.Add(ticket);
        MarkAsModified();
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}