using JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;
using JCP.TicketWave.PaymentService.Features.Payments.GetPaymentStatus;
using JCP.TicketWave.PaymentService.Features.Refunds.ProcessRefund;
using JCP.TicketWave.PaymentService.Controllers;
using JCP.TicketWave.PaymentService.Infrastructure.Data;
using JCP.TicketWave.PaymentService.Infrastructure.Data.Repositories;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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
