namespace JCP.TicketWave.BookingService.Domain.Models;

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    Expired = 4
}