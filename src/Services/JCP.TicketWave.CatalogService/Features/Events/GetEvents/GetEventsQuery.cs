namespace JCP.TicketWave.CatalogService.Features.Events.GetEvents;

using System.ComponentModel.DataAnnotations;

public record GetEventsQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 10;

    public string? Category { get; init; }
    public string? Search { get; init; }
}