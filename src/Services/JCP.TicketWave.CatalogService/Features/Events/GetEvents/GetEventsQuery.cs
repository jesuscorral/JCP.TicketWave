namespace JCP.TicketWave.CatalogService.Features.Events.GetEvents;

public record GetEventsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    string? Search = null);