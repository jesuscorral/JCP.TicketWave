using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.CatalogService.Features.Events.GetEvents;

namespace JCP.TicketWave.CatalogService.Controllers;

public static class GetEventsController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? category,
            [FromQuery] string? search,
            GetEventsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetEventsQuery(page, pageSize, category, search);
            var result = await handler.Handle(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Events")
        .WithSummary("Get events with pagination and filters");
    }
}