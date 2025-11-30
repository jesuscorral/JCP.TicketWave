using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.CatalogService.Features.Events.GetEventById;

namespace JCP.TicketWave.CatalogService.Controllers;

public static class GetEventByIdController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
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