using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.BookingService.Domain.Events;
using JCP.TicketWave.BookingService.Domain.Validators;

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
        // Create validation request
        var validationRequest = new CreateBookingRequest(
            eventId, userId, customerEmail, quantity, totalAmount, expiresAt);

        // Validate using FluentValidation
        var validator = new BookingValidator();
        var validationResult = validator.Validate(validationRequest);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new DomainException($"Booking validation failed: {errors}");
        }

        var booking = new Booking(eventId, userId, customerEmail, quantity, totalAmount, expiresAt);
        
        // Add domain events
        booking.AddDomainEvent(new BookingCreatedDomainEvent(booking.Id, eventId, userId, quantity, totalAmount));
        booking.AddDomainEvent(new BookingCreatedIntegrationEvent(booking.Id, eventId, userId, customerEmail, quantity, totalAmount, booking.ExpiresAt ?? DateTime.UtcNow.AddMinutes(15)));
        
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
        
        // Add domain events
        AddDomainEvent(new BookingConfirmedDomainEvent(Id, EventId, UserId));
        AddDomainEvent(new BookingConfirmedIntegrationEvent(Id, EventId, UserId, CustomerEmail, Quantity, TotalAmount, DateTime.UtcNow));
    }

    public void Cancel(string reason = "")
    {
        if (Status == BookingStatus.Cancelled)
            return; // Already cancelled

        if (Status == BookingStatus.Completed)
            throw new DomainException("Cannot cancel completed booking");

        Status = BookingStatus.Cancelled;
        MarkAsModified();
        
        // Add domain events
        AddDomainEvent(new BookingCancelledDomainEvent(Id, EventId, reason));
        AddDomainEvent(new BookingCancelledIntegrationEvent(Id, EventId, UserId, reason, DateTime.UtcNow));
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new DomainException($"Cannot complete booking in {Status} status");

        Status = BookingStatus.Completed;
        MarkAsModified();
        
        // Add domain events
        AddDomainEvent(new BookingCompletedDomainEvent(Id, EventId, TotalAmount));
        AddDomainEvent(new BookingCompletedIntegrationEvent(Id, EventId, UserId, TotalAmount, DateTime.UtcNow));
    }

    public void Expire()
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException($"Cannot expire booking in {Status} status");

        Status = BookingStatus.Cancelled;
        MarkAsModified();

        // Add domain event
        AddDomainEvent(new BookingExpiredDomainEvent(Id, EventId, UserId, CustomerEmail, Quantity, TotalAmount, DateTime.UtcNow));
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