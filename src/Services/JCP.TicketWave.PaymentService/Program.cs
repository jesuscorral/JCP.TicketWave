using Microsoft.EntityFrameworkCore;
using FluentValidation;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Persistence;
using JCP.TicketWave.PaymentService.Infrastructure.Persistence.Repositories;
using JCP.TicketWave.PaymentService.Application.Controllers;
using JCP.TicketWave.PaymentService.Application.Features.Payments.ProcessPayment;
using JCP.TicketWave.PaymentService.Application.Features.Payments.GetPaymentStatus;
using JCP.TicketWave.PaymentService.Application.Features.Refunds.ProcessRefund;
using JCP.TicketWave.PaymentService.Domain.Validators;
using JCP.TicketWave.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type =>
    {
        // Handle nested classes to avoid schema ID conflicts
        if (type.IsNested)
        {
            var declaringType = type.DeclaringType?.Name;
            return $"{declaringType}{type.Name}";
        }
        return type.Name;
    });
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PaymentValidator>();

// Domain Events
builder.Services.AddDomainEvents();
builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);

// RabbitMQ Integration Events
builder.Services.AddRabbitMQ(builder.Configuration);

// Register integration event handlers
builder.Services.AddScoped<JCP.TicketWave.PaymentService.Application.EventHandlers.PreparePaymentDataIntegrationEventHandler>();

// Configure SQL Server Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<PaymentDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
}
else
{
    throw new InvalidOperationException("DefaultConnection string is required");
}

// Register repository implementations
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();

// Register handlers for dependency injection
builder.Services.AddScoped<ProcessPaymentHandler>();
builder.Services.AddScoped<GetPaymentStatusHandler>();
builder.Services.AddScoped<ProcessRefundHandler>();

// Add CORS for microservices communication
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Map feature endpoints
PaymentsController.MapEndpoint(app);
RefundsController.MapEndpoint(app);

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "PaymentService" }))
   .WithTags("Health");

app.Run();
