using JCP.TicketWave.NotificationService;
using JCP.TicketWave.NotificationService.Features.Email;
using JCP.TicketWave.NotificationService.Features.Pdf;

var builder = Host.CreateApplicationBuilder(args);

// Register the background service
builder.Services.AddHostedService<Worker>();

// Register feature handlers
builder.Services.AddScoped<SendBookingConfirmation.Handler>();
builder.Services.AddScoped<GenerateTicketPdf.Handler>();

// TODO: Add message queue services (Azure Service Bus, RabbitMQ, etc.)
// TODO: Add email service configuration (SendGrid, SMTP, etc.)
// TODO: Add PDF generation services (iTextSharp, PdfSharp, etc.)

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Notification Service...");

host.Run();
