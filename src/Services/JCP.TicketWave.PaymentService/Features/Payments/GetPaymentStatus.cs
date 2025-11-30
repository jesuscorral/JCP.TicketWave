using Microsoft.AspNetCore.Mvc;

namespace JCP.TicketWave.PaymentService.Features.Payments;

public static class GetPaymentStatus
{
    public record Query(Guid PaymentId);

    public record Response(
        Guid PaymentId,
        string PaymentIntentId,
        Guid BookingId,
        decimal Amount,
        string Currency,
        PaymentStatus Status,
        DateTime ProcessedAt,
        string? FailureReason);

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Cancelled
    }

    public class Handler
    {
        // TODO: Implement repository pattern
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
        app.MapGet("/api/payments/{paymentId:guid}", async (
            Guid paymentId,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new Query(paymentId);
            var result = await handler.Handle(query, cancellationToken);
            
            return result is not null 
                ? Results.Ok(result) 
                : Results.NotFound();
        })
        .WithTags("Payments")
        .WithSummary("Get payment status by ID");
    }
}