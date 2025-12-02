using JCP.TicketWave.CatalogService.Features.Events.GetEvents;
using JCP.TicketWave.CatalogService.Features.Events.GetEventById;
using JCP.TicketWave.CatalogService.Features.Categories.GetCategories;
using JCP.TicketWave.CatalogService.Controllers;
using JCP.TicketWave.CatalogService.Infrastructure.Data;
using JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using Microsoft.Azure.Cosmos;

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

// Configure Azure Cosmos DB
var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddSingleton(serviceProvider =>
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
            ConnectionMode = ConnectionMode.Gateway,
            MaxRequestsPerTcpConnection = 10,
            MaxTcpConnectionsPerEndpoint = 16,
            RequestTimeout = TimeSpan.FromSeconds(30)
        };

        return new CosmosClient(cosmosConnectionString, cosmosClientOptions);
    });

    builder.Services.AddSingleton<CosmosDbService>();
    
    // Register repository implementations
    builder.Services.AddScoped<IEventRepository, EventRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IVenueRepository, VenueRepository>();
}
else
{
    throw new InvalidOperationException("CosmosDb connection string is required");
}

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
