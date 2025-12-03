namespace JCP.TicketWave.CatalogService.Application.Features.Events.GetEventById;

public record GetEventByIdResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Venue,
    string Category,
    decimal Price,
    int AvailableTickets,
    string? ImageUrl);