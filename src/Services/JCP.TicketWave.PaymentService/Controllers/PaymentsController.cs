using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.PaymentService.Features.Payments.GetPaymentStatus;
using JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;
using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Controllers;

public static class PaymentsController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/{paymentId:guid}", async (
            Guid paymentId,
            GetPaymentStatusHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPaymentStatusQuery(paymentId);
            var result = await handler.Handle(query, cancellationToken);

            return result is not null
                ? Results.Ok(result)
                : Results.NotFound();
        })
        .WithTags("Payments")
        .WithSummary("Get payment status by ID");
        
        app.MapPost("/api/payments", async (
            [FromBody] ProcessPaymentCommand command,
            ProcessPaymentHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            
            return result.Status == PaymentStatus.Succeeded 
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithTags("Payments")
        .WithSummary("Process a payment");
    }
}