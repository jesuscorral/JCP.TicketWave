using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.PaymentService.Domain.Models;
using JCP.TicketWave.PaymentService.Application.Features.Refunds.ProcessRefund;

namespace JCP.TicketWave.PaymentService.Application.Controllers;

public static class RefundsController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/refunds", async (
            [FromBody] ProcessRefundCommand command,
            ProcessRefundHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            
            return result.Status == RefundStatus.Succeeded 
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithTags("Refunds")
        .WithSummary("Process a refund");
    }
}
