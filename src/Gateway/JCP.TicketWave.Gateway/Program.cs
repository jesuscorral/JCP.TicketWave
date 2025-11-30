var builder = WebApplication.CreateBuilder(args);

// Add services to the container
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

// Add HTTP clients for microservices
builder.Services.AddHttpClient("CatalogService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CatalogService:BaseUrl"] ?? "https://localhost:7001");
});

builder.Services.AddHttpClient("BookingService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BookingService:BaseUrl"] ?? "https://localhost:7002");
});

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService:BaseUrl"] ?? "https://localhost:7003");
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add rate limiting (future enhancement)
// builder.Services.AddRateLimiter(...);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Gateway endpoints - Route to microservices

// Catalog Service routes
app.MapGet("/api/events", async (IHttpClientFactory clientFactory, HttpContext context) =>
{
    var client = clientFactory.CreateClient("CatalogService");
    var response = await client.GetAsync($"/api/events{context.Request.QueryString}");
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Events")
.WithSummary("Get events from Catalog Service");

app.MapGet("/api/events/{id:guid}", async (Guid id, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("CatalogService");
    var response = await client.GetAsync($"/api/events/{id}");
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Events")
.WithSummary("Get event by ID from Catalog Service");

app.MapGet("/api/categories", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("CatalogService");
    var response = await client.GetAsync("/api/categories");
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Categories")
.WithSummary("Get categories from Catalog Service");

// Booking Service routes
app.MapPost("/api/bookings", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("BookingService");
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/api/bookings", content);
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Bookings")
.WithSummary("Create booking via Booking Service");

app.MapGet("/api/bookings/{id:guid}", async (Guid id, IHttpClientFactory clientFactory, HttpContext context) =>
{
    var client = clientFactory.CreateClient("BookingService");
    var response = await client.GetAsync($"/api/bookings/{id}{context.Request.QueryString}");
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Bookings")
.WithSummary("Get booking by ID from Booking Service");

app.MapPost("/api/tickets/reserve", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("BookingService");
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/api/tickets/reserve", content);
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Tickets")
.WithSummary("Reserve tickets via Booking Service");

// Payment Service routes
app.MapPost("/api/payments", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("PaymentService");
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/api/payments", content);
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Payments")
.WithSummary("Process payment via Payment Service");

app.MapGet("/api/payments/{id:guid}", async (Guid id, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("PaymentService");
    var response = await client.GetAsync($"/api/payments/{id}");
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Payments")
.WithSummary("Get payment status from Payment Service");

app.MapPost("/api/refunds", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("PaymentService");
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/api/refunds", content);
    return Results.Json(await response.Content.ReadAsStringAsync(), statusCode: (int)response.StatusCode);
})
.WithTags("Refunds")
.WithSummary("Process refund via Payment Service");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Gateway" }))
   .WithTags("Health");

// Combined health check for all services
app.MapGet("/health/services", async (IHttpClientFactory clientFactory) =>
{
    var services = new Dictionary<string, object>();
    
    try
    {
        var catalogClient = clientFactory.CreateClient("CatalogService");
        var catalogResponse = await catalogClient.GetAsync("/health");
        services["CatalogService"] = catalogResponse.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
    }
    catch
    {
        services["CatalogService"] = "Unreachable";
    }
    
    try
    {
        var bookingClient = clientFactory.CreateClient("BookingService");
        var bookingResponse = await bookingClient.GetAsync("/health");
        services["BookingService"] = bookingResponse.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
    }
    catch
    {
        services["BookingService"] = "Unreachable";
    }
    
    try
    {
        var paymentClient = clientFactory.CreateClient("PaymentService");
        var paymentResponse = await paymentClient.GetAsync("/health");
        services["PaymentService"] = paymentResponse.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
    }
    catch
    {
        services["PaymentService"] = "Unreachable";
    }
    
    return Results.Ok(new { Gateway = "Healthy", Services = services });
})
.WithTags("Health");

app.Run();
