using Microsoft.AspNetCore.Mvc;

namespace JCP.TicketWave.BookingService.Features.Bookings;

public static class GetBooking
{
    public record Query(Guid BookingId, string? UserId = null);

    public record Response(
        Guid BookingId,
        string BookingReference,
        Guid EventId,
        string UserId,
        int TicketCount,
        decimal TotalAmount,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        DateTime? ConfirmedAt);

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Expired
    }

    public class Handler
    {
        // TODO: Implement repository pattern with SQL Server
        public async Task<Response?> Handle(Query query, CancellationToken cancellationToken)
        {
            // Placeholder implementation
            await Task.Delay(10, cancellationToken);
            
            // Return null if not found, actual implementation would query database
            return null;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/{bookingId:guid}", async (
            Guid bookingId,
            [FromQuery] string? userId,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new Query(bookingId, userId);
            var result = await handler.Handle(query, cancellationToken);
            
            return result is not null 
                ? Results.Ok(result) 
                : Results.NotFound();
        })
        .WithTags("Bookings")
        .WithSummary("Get booking by ID");
    }
}