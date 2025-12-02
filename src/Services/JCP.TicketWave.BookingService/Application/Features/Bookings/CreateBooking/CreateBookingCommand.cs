namespace JCP.TicketWave.BookingService.Application.Features.Bookings.CreateBooking;

public record CreateBookingCommand(
    Guid EventId,
    string UserId,
    int TicketCount);
