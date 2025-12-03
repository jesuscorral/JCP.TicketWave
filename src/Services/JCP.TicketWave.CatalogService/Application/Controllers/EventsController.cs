using JCP.TicketWave.CatalogService.Application.Features.Events.GetEventById;
using JCP.TicketWave.CatalogService.Application.Features.Events.GetEvents;
using Microsoft.AspNetCore.Mvc;

namespace JCP.TicketWave.CatalogService.Application.Controllers;

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