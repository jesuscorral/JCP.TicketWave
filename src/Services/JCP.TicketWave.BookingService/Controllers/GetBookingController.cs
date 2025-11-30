using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.BookingService.Features.Bookings.GetBooking;

namespace JCP.TicketWave.BookingService.Controllers;

public static class GetBookingController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
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