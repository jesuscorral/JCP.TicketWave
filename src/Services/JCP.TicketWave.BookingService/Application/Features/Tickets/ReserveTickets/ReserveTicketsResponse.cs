namespace JCP.TicketWave.BookingService.Application.Features.Tickets.ReserveTickets;

public record ReserveTicketsResponse(
    Guid ReservationId,
    DateTime ExpiresAt,
    bool Success,
    string? ErrorMessage);