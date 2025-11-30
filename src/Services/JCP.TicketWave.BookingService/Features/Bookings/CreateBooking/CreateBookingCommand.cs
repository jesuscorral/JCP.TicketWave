namespace JCP.TicketWave.BookingService.Features.Bookings.CreateBooking;

public record CreateBookingCommand(
    Guid EventId,
    string UserId,
    int TicketCount);
