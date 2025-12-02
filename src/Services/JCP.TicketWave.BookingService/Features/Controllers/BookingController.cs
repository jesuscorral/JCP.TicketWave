using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.BookingService.Features.Features.Bookings.CreateBooking;
using JCP.TicketWave.BookingService.Features.Features.Bookings.GetBooking;

namespace JCP.TicketWave.BookingService.Features.Controllers;

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
        
        app.MapGet("/api/bookings/{bookingId:guid}", async (
            Guid bookingId,
            [FromQuery] string? userId,
            GetBookingHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBookingQuery(bookingId, userId);
            var result = await handler.Handle(query, cancellationToken);
            
            return result is not null 
                ? Results.Ok(result) 
                : Results.NotFound();
        })
        .WithTags("Bookings")
        .WithSummary("Get booking by ID");
    }
}