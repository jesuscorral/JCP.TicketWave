using JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;
using JCP.TicketWave.PaymentService.Features.Payments.GetPaymentStatus;
using JCP.TicketWave.PaymentService.Features.Refunds.ProcessRefund;
using JCP.TicketWave.PaymentService.Controllers;

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
