namespace JCP.TicketWave.BookingService.Application.Features.Bookings.GetBooking;

public record GetBookingQuery(Guid BookingId, string? UserId = null);