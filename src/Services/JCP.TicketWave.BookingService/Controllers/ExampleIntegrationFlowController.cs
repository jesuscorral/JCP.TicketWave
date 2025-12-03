using Microsoft.AspNetCore.Mvc;
using JCP.TicketWave.BookingService.Application.Features.Bookings.CreateBooking;

namespace JCP.TicketWave.BookingService.Controllers;

/// <summary>
/// Controlador para demostrar el flujo de integration events
/// </summary>
[ApiController]
[Route("api/example")]
public class ExampleIntegrationFlowController : ControllerBase
{
    private readonly CreateBookingHandler _createBookingHandler;
    private readonly ILogger<ExampleIntegrationFlowController> _logger;

    public ExampleIntegrationFlowController(
        CreateBookingHandler createBookingHandler,
        ILogger<ExampleIntegrationFlowController> logger)
    {
        _createBookingHandler = createBookingHandler;
        _logger = logger;
    }

    /// <summary>
    /// Crea un booking de ejemplo que desencadena el flujo completo de integration events
    /// </summary>
    [HttpPost("create-booking-with-flow")]
    public async Task<IActionResult> CreateBookingWithIntegrationFlow(
        [FromBody] CreateBookingExampleRequest request)
    {
        try
        {
            _logger.LogInformation(
                "🚀 Starting integration events example flow - EventId: {EventId}, UserId: {UserId}",
                request.EventId, request.UserId);

            // Crear el booking (esto desencadena el domain event)
            var command = new CreateBookingCommand(
                request.EventId,
                request.UserId,
                request.TicketCount
            );

            var result = await _createBookingHandler.Handle(command, CancellationToken.None);

            _logger.LogInformation(
                "✅ Booking created successfully - BookingId: {BookingId}. Integration events should be processing...",
                result.BookingId);

            return Ok(new
            {
                BookingId = result.BookingId,
                EventId = request.EventId,
                UserId = request.UserId,
                TicketCount = request.TicketCount,
                Status = "Created",
                Message = "Booking created successfully. Check logs for integration events flow.",
                ExpectedFlow = new[]
                {
                    "1. UpdateEventInventoryIntegrationEvent → CatalogService",
                    "2. SendBookingNotificationIntegrationEvent → NotificationService",
                    "3. PreparePaymentDataIntegrationEvent → PaymentService",
                    "4. InventoryUpdatedIntegrationEvent ← CatalogService",
                    "5. NotificationSentIntegrationEvent ← NotificationService",
                    "6. PaymentDataPreparedIntegrationEvent ← PaymentService"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error creating booking for integration events example");
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene información sobre el flujo de integration events
    /// </summary>
    [HttpGet("integration-flow-info")]
    public IActionResult GetIntegrationFlowInfo()
    {
        return Ok(new
        {
            Title = "Integration Events Example Flow",
            Description = "Este ejemplo demuestra cómo los domain events se convierten en integration events y fluyen entre servicios",
            Flow = new
            {
                Step1 = new
                {
                    Service = "BookingService",
                    Action = "Domain Event: BookingCreatedDomainEvent",
                    Handler = "BookingCreatedDomainEventHandler",
                    Publishes = new[]
                    {
                        "UpdateEventInventoryIntegrationEvent",
                        "SendBookingNotificationIntegrationEvent", 
                        "PreparePaymentDataIntegrationEvent"
                    }
                },
                Step2 = new
                {
                    Service = "CatalogService",
                    Action = "Handles UpdateEventInventoryIntegrationEvent",
                    Handler = "UpdateEventInventoryIntegrationEventHandler",
                    Publishes = new[] { "InventoryUpdatedIntegrationEvent" }
                },
                Step3 = new
                {
                    Service = "NotificationService", 
                    Action = "Handles SendBookingNotificationIntegrationEvent",
                    Handler = "SendBookingNotificationIntegrationEventHandler",
                    Publishes = new[] { "NotificationSentIntegrationEvent" }
                },
                Step4 = new
                {
                    Service = "PaymentService",
                    Action = "Handles PreparePaymentDataIntegrationEvent", 
                    Handler = "PreparePaymentDataIntegrationEventHandler",
                    Publishes = new[] { "PaymentDataPreparedIntegrationEvent" }
                }
            },
            SagaPreparation = "Este flujo prepara la base para implementar un Saga pattern que pueda manejar transacciones distribuidas y compensaciones",
            Usage = "POST /api/example/create-booking-with-flow con { \"eventId\": \"guid\", \"userId\": \"string\", \"ticketCount\": number }"
        });
    }
}

/// <summary>
/// Request para el ejemplo de integration events
/// </summary>
public record CreateBookingExampleRequest(
    Guid EventId,
    string UserId,
    int TicketCount
);