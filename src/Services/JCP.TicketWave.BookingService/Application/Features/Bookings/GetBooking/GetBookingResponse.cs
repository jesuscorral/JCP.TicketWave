namespace JCP.TicketWave.BookingService.Application.Features.Bookings.GetBooking;

public record GetBookingResponse(
    Guid BookingId,
    string BookingReference,
    Guid EventId,
    string UserId,
    int TicketCount,
    decimal TotalAmount,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? ConfirmedAt);

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Expired
}