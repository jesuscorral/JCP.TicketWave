using Microsoft.EntityFrameworkCore;
using FluentValidation;
using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Infrastructure.Persistence;
using JCP.TicketWave.BookingService.Infrastructure.Persistence.Repositories;
using JCP.TicketWave.BookingService.Application.Controllers;
using JCP.TicketWave.BookingService.Application.Features.Bookings.GetBooking;
using JCP.TicketWave.BookingService.Application.Features.Bookings.CreateBooking;
using JCP.TicketWave.BookingService.Application.Features.Tickets.ReserveTickets;
using JCP.TicketWave.BookingService.Domain.Validators;
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
builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();

// Domain Events
builder.Services.AddDomainEvents();
builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);

// Register handlers for dependency injection
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<GetBookingHandler>();
builder.Services.AddScoped<ReserveTicketsHandler>();

// Database configuration
builder.Services.AddDbContext<BookingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
        sqlOptions.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName);
    });
});

// Repository registration
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

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
