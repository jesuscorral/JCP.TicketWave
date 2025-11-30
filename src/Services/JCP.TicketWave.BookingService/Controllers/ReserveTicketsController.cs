using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.BookingService.Features.Tickets.ReserveTickets;

namespace JCP.TicketWave.BookingService.Controllers;

public static class ReserveTicketsController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tickets/reserve", async (
            [FromBody] ReserveTicketsCommand command,
            ReserveTicketsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            
            return result.Success 
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithTags("Tickets")
        .WithSummary("Reserve tickets for an event");
    }
}