namespace JCP.TicketWave.BookingService.Features.Bookings.GetBooking;

public record GetBookingQuery(Guid BookingId, string? UserId = null);