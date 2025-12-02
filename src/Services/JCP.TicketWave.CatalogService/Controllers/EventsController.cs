using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.CatalogService.Features.Events.GetEvents;
using JCP.TicketWave.CatalogService.Features.Events.GetEventById;

namespace JCP.TicketWave.CatalogService.Controllers;

public static class EventsController
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
            var query = new GetEventsQuery 
            { 
                Page = page, 
                PageSize = pageSize, 
                Category = category, 
                Search = search 
            };
            var result = await handler.Handle(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Events")
        .WithSummary("Get events with pagination and filters");

        app.MapGet("/api/events/{id:guid}", async (
            Guid id,
            GetEventByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetEventByIdQuery(id);
            var result = await handler.Handle(query, cancellationToken);
            
            return result is not null
                ? Results.Ok(result)
                : Results.NotFound();
        })
        .WithTags("Events")
        .WithSummary("Get event by ID");
    }
}