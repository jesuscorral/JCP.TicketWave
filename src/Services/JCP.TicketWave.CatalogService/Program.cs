using Microsoft.EntityFrameworkCore;
using FluentValidation;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence.Repositories;
using JCP.TicketWave.CatalogService.Application.Controllers;
using JCP.TicketWave.CatalogService.Application.Features.Events.GetEventById;
using JCP.TicketWave.CatalogService.Application.Features.Events.GetEvents;
using JCP.TicketWave.CatalogService.Application.Features.Categories.GetCategories;
using JCP.TicketWave.CatalogService.Domain.Validators;
using JCP.TicketWave.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddValidatorsFromAssemblyContaining<EventValidator>();

// Domain Events
builder.Services.AddDomainEvents();
builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);

// Database configuration - SQL Server with EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<CatalogDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
            sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
        }));
}
else
{
    throw new InvalidOperationException("DefaultConnection string is required");
}

// Register repository implementations
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();

// Register handlers for dependency injection
builder.Services.AddScoped<GetEventsHandler>();
builder.Services.AddScoped<GetEventByIdHandler>();
builder.Services.AddScoped<GetCategoriesHandler>();

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
EventsController.MapEndpoint(app);
CategoriesController.MapEndpoint(app);

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "CatalogService" }))
   .WithTags("Health");

app.Run();
