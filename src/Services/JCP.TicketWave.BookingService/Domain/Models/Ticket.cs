using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.BookingService.Domain.Events;

namespace JCP.TicketWave.BookingService.Domain.Models;

public class Ticket : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid? BookingId { get; private set; }
    public string TicketType { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? SeatNumber { get; private set; }
    public DateTime? ReservedUntil { get; private set; }

    // Navigation property
    public Booking? Booking { get; private set; }

    // Private constructor for EF Core
    private Ticket() : base() { }

    // Private constructor for factory method
    private Ticket(
        Guid eventId,
        string ticketType,
        decimal price,
        string? seatNumber) : base()
    {
        EventId = eventId;
        TicketType = ticketType;
        Price = price;
        SeatNumber = seatNumber;
        Status = TicketStatus.Available;
    }

    // Factory method for creating new tickets
    public static Ticket Create(
        Guid eventId,
        string ticketType,
        decimal price,
        string? seatNumber = null)
    {
        if (string.IsNullOrWhiteSpace(ticketType))
            throw new DomainException("Ticket type is required");
        
        if (price < 0)
            throw new DomainException("Price cannot be negative");

        return new Ticket(eventId, ticketType, price, seatNumber);
    }

    // Business methods
    public void Reserve(Guid bookingId, TimeSpan reservationDuration = default)
    {
        if (Status != TicketStatus.Available)
            throw new DomainException($"Cannot reserve ticket in {Status} status");

        Status = TicketStatus.Reserved;
        BookingId = bookingId;
        ReservedUntil = DateTime.UtcNow.Add(reservationDuration == default ? TimeSpan.FromMinutes(15) : reservationDuration);
        UpdateTimestamp();

        // Add domain event
        AddDomainEvent(new TicketReservedDomainEvent(Id, bookingId, EventId, SeatNumber ?? string.Empty, Price, DateTime.UtcNow));
    }

    public void Purchase(Guid bookingId)
    {
        if (Status != TicketStatus.Reserved)
            throw new DomainException($"Cannot purchase ticket in {Status} status");
        
        if (BookingId != bookingId)
            throw new DomainException("Ticket is reserved for a different booking");

        Status = TicketStatus.Confirmed;
        ReservedUntil = null;
        UpdateTimestamp();

        // Add domain event
        AddDomainEvent(new TicketConfirmedDomainEvent(Id, bookingId, EventId, SeatNumber ?? string.Empty, Price, DateTime.UtcNow));
    }

    public void Release()
    {
        if (Status == TicketStatus.Available)
            return; // Already available

        if (Status == TicketStatus.Sold)
            throw new DomainException("Cannot release sold ticket");

        var originalBookingId = BookingId ?? Guid.Empty;
        Status = TicketStatus.Available;
        BookingId = null;
        ReservedUntil = null;
        UpdateTimestamp();

        // Add domain event
        AddDomainEvent(new TicketReservationReleasedDomainEvent(Id, originalBookingId, EventId, SeatNumber ?? string.Empty, DateTime.UtcNow, "Manual release"));
    }

    public void Cancel()
    {
        if (Status == TicketStatus.Sold)
        {
            Status = TicketStatus.Cancelled;
            UpdateTimestamp();
        }
        else
        {
            Release();
        }
    }

    public bool IsReservationExpired => 
        Status == TicketStatus.Reserved && 
        ReservedUntil.HasValue && 
        DateTime.UtcNow > ReservedUntil.Value;
}