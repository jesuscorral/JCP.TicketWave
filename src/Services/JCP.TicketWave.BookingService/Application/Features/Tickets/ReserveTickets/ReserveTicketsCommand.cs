namespace JCP.TicketWave.BookingService.Application.Features.Tickets.ReserveTickets;

public record ReserveTicketsCommand(
    Guid EventId,
    int TicketCount,
    string UserId);