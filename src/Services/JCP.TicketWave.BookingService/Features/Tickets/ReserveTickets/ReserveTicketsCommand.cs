namespace JCP.TicketWave.BookingService.Features.Tickets.ReserveTickets;

public record ReserveTicketsCommand(
    Guid EventId,
    int TicketCount,
    string UserId);