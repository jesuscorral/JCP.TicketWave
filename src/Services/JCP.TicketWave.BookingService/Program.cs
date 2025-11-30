using JCP.TicketWave.BookingService.Features.Bookings.CreateBooking;
using JCP.TicketWave.BookingService.Features.Bookings.GetBooking;
using JCP.TicketWave.BookingService.Features.Tickets.ReserveTickets;
using JCP.TicketWave.BookingService.Controllers;

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
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<GetBookingHandler>();
builder.Services.AddScoped<ReserveTicketsHandler>();

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
BookingController.MapEndpoint(app);
TicketsController.MapEndpoint(app);

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "BookingService" }))
   .WithTags("Health");

app.Run();
