using JCP.TicketWave.BookingService.Domain.Enums;

namespace JCP.TicketWave.BookingService.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EventId { get; private set; }
    public Guid? BookingId { get; private set; }
    public string TicketType { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? SeatNumber { get; private set; }
    public DateTime? ReservedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    public Booking? Booking { get; private set; }

    // Private constructor for EF Core
    private Ticket() { }

    // Factory method for creating new tickets
    public static Ticket Create(
        Guid eventId,
        string ticketType,
        decimal price,
        string? seatNumber = null)
    {
        if (string.IsNullOrWhiteSpace(ticketType))
            throw new ArgumentException("Ticket type is required", nameof(ticketType));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        return new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            TicketType = ticketType,
            Price = price,
            SeatNumber = seatNumber,
            Status = TicketStatus.Available,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void Reserve(Guid bookingId, TimeSpan reservationDuration = default)
    {
        if (Status != TicketStatus.Available)
            throw new InvalidOperationException($"Cannot reserve ticket in {Status} status");

        Status = TicketStatus.Reserved;
        BookingId = bookingId;
        ReservedUntil = DateTime.UtcNow.Add(reservationDuration == default ? TimeSpan.FromMinutes(15) : reservationDuration);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Purchase(Guid bookingId)
    {
        if (Status != TicketStatus.Reserved)
            throw new InvalidOperationException($"Cannot purchase ticket in {Status} status");
        
        if (BookingId != bookingId)
            throw new InvalidOperationException("Ticket is reserved for a different booking");

        Status = TicketStatus.Sold;
        ReservedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        if (Status == TicketStatus.Available)
            return; // Already available

        if (Status == TicketStatus.Sold)
            throw new InvalidOperationException("Cannot release sold ticket");

        Status = TicketStatus.Available;
        BookingId = null;
        ReservedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == TicketStatus.Sold)
        {
            Status = TicketStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
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