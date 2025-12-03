namespace JCP.TicketWave.BookingService.Domain.Models;

public enum TicketStatus
{
    Available = 0,
    Reserved = 1,
    Confirmed = 2,
    Sold = 3,
    Cancelled = 4
}