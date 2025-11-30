namespace JCP.TicketWave.CatalogService.Features.Events.GetEvents;

public record GetEventsResponse(
    IEnumerable<EventDto> Events,
    int TotalCount,
    int Page,
    int PageSize);

public record EventDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Venue,
    string Category,
    decimal Price,
    int AvailableTickets);