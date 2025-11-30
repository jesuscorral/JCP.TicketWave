namespace JCP.TicketWave.BookingService.Features.Bookings.CreateBooking;

public record CreateBookingResponse(
    Guid BookingId,
    string BookingReference,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
    
public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Expired
}
