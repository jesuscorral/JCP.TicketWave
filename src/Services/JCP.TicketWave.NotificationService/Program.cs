using JCP.TicketWave.NotificationService;
using JCP.TicketWave.NotificationService.Features.Email;
using JCP.TicketWave.NotificationService.Features.Pdf;
using JCP.TicketWave.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Register the background service
builder.Services.AddHostedService<Worker>();

// Register feature handlers
builder.Services.AddScoped<SendBookingConfirmation.Handler>();
builder.Services.AddScoped<GenerateTicketPdf.Handler>();

// Domain Events
builder.Services.AddDomainEvents();
builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);

// RabbitMQ Integration Events
builder.Services.AddRabbitMQ(builder.Configuration);

// Register integration event handlers
builder.Services.AddScoped<JCP.TicketWave.NotificationService.Application.EventHandlers.SendBookingNotificationIntegrationEventHandler>();

// TODO: Add email service configuration (SendGrid, SMTP, etc.)
// TODO: Add PDF generation services (iTextSharp, PdfSharp, etc.)

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Notification Service...");

host.Run();
