using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.BookingService.Features.Bookings.CreateBooking;

namespace JCP.TicketWave.BookingService.Controllers;

public static class BookingController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
         app.MapPost("/api/bookings", async (
            [FromBody] CreateBookingCommand command,
            CreateBookingHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/bookings/{result.BookingId}", result);
        })
        .WithTags("Bookings")
        .WithSummary("Create a new booking");
    }
}