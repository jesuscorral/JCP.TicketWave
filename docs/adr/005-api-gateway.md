# ADR-005: API Gateway as Entry Point

## Status
Accepted

## Date
2025-11-26

## Context
With multiple microservices, clients need:

- **Unified entry point**: Avoid clients knowing addresses of multiple services
- **Cross-cutting concerns**: Authentication, authorization, rate limiting, centralized logging
- **Data aggregation**: Combine responses from multiple services in a single request
- **Protocol translation**: Different internal protocols (HTTP, gRPC) exposed as HTTP
- **Circuit breaking**: Protection against cascading failures

Alternatives considered:
- Backend for Frontend (BFF) pattern
- Service mesh (Istio, Linkerd)
- Load balancer with reverse proxy
- Managed API Gateway (Azure API Management, AWS API Gateway)

## Decision
We implement a **custom API Gateway with ASP.NET Core** that acts as:

1. **Reverse Proxy**: Routes requests to appropriate microservices
2. **Cross-cutting Handler**: Handles shared concerns
3. **Health Check Aggregator**: Consolidates health checks from all services
4. **Response Composer**: Combines responses when necessary

### Gateway Architecture

```
Client → [API Gateway] → [Specific Microservice]
            ↓
        [Auth, Logging, Rate Limiting, Circuit Breaking]
```

## Implementation

### 1. Routing Configuration

```csharp
// Program.cs - API Gateway
var builder = WebApplication.CreateBuilder(args);

// HTTP Clients for each service
builder.Services.AddHttpClient("CatalogService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CatalogService:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient("BookingService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BookingService:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Route mapping
app.MapCatalogRoutes();
app.MapBookingRoutes();
app.MapPaymentRoutes();
```

### 2. Service-Specific Route Mapping

```csharp
// Catalog Service Routes
public static class CatalogRouteExtensions
{
    public static void MapCatalogRoutes(this IEndpointRouteBuilder app)
    {
        // Events
        app.MapGet("/api/events", ProxyToService("CatalogService", "/api/events"))
           .WithTags("Events")
           .WithSummary("Get events with pagination and filtering");
           
        app.MapGet("/api/events/{id:guid}", ProxyToService("CatalogService", "/api/events/{id}"))
           .WithTags("Events");
           
        // Categories
        app.MapGet("/api/categories", ProxyToService("CatalogService", "/api/categories"))
           .WithTags("Categories");
    }
    
    private static RequestDelegate ProxyToService(string serviceName, string path)
    {
        return async (HttpContext context) =>
        {
            var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            
            try
            {
                var client = httpClientFactory.CreateClient(serviceName);
                var targetUri = BuildTargetUri(path, context);
                
                logger.LogInformation("Proxying request to {Service}: {Method} {Uri}", 
                    serviceName, context.Request.Method, targetUri);
                
                var response = await client.GetAsync(targetUri);
                var content = await response.Content.ReadAsStringAsync();
                
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsync(content);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Service {Service} is unavailable", serviceName);
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("{\"error\":\"Service unavailable\"}");
            }
        };
    }
}
```

### 3. Cross-Cutting Concerns

#### Authentication & Authorization Middleware
```csharp
// Implementación futura
public class AuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip auth for health checks and public endpoints
        if (IsPublicEndpoint(context.Request.Path))
        {
            await next(context);
            return;
        }
        
        var token = ExtractToken(context.Request);
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"error\":\"Authentication required\"}");
            return;
        }
        
        var principal = await ValidateTokenAsync(token);
        if (principal == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"error\":\"Invalid token\"}");
            return;
        }
        
        context.User = principal;
        await next(context);
    }
}
```

#### Rate Limiting
```csharp
// Implementación futura con ASP.NET Core Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

#### Request Correlation
```csharp
public class CorrelationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        // Forward correlation ID to downstream services
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path
        });
        
        await next(context);
    }
}
```

### 4. Health Check Aggregation

```csharp
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Gateway" }));

app.MapGet("/health/services", async (IHttpClientFactory clientFactory, ILogger<Program> logger) =>
{
    var services = new Dictionary<string, object>();
    var serviceClients = new[]
    {
        ("CatalogService", "CatalogService"),
        ("BookingService", "BookingService"), 
        ("PaymentService", "PaymentService")
    };
    
    var tasks = serviceClients.Select(async service =>
    {
        try
        {
            var client = clientFactory.CreateClient(service.Item2);
            var response = await client.GetAsync("/health");
            return (service.Item1, response.IsSuccessStatusCode ? "Healthy" : "Unhealthy");
        }
        catch (Exception ex)
        {
            logger.LogWarning("Health check failed for {Service}: {Error}", service.Item1, ex.Message);
            return (service.Item1, "Unreachable");
        }
    });
    
    var results = await Task.WhenAll(tasks);
    foreach (var (serviceName, status) in results)
    {
        services[serviceName] = status;
    }
    
    var overallStatus = services.Values.All(s => s.ToString() == "Healthy") ? "Healthy" : "Degraded";
    
    return Results.Ok(new 
    { 
        Gateway = "Healthy", 
        OverallStatus = overallStatus,
        Services = services 
    });
});
```

### 5. Error Handling y Circuit Breaking

```csharp
// Extensión para manejo de errores consistente
public static class ErrorHandlingExtensions
{
    public static async Task<IResult> HandleServiceCall(
        Func<Task<HttpResponseMessage>> serviceCall,
        ILogger logger,
        string serviceName)
    {
        try
        {
            var response = await serviceCall();
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
            }
            
            logger.LogWarning("Service {Service} returned error: {StatusCode} {Content}", 
                serviceName, response.StatusCode, content);
                
            return Results.Problem(
                detail: content,
                statusCode: (int)response.StatusCode,
                title: $"{serviceName} Error");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Service {Service} is unreachable", serviceName);
            return Results.Problem(
                detail: "Service temporarily unavailable",
                statusCode: 503,
                title: "Service Unavailable");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Service {Service} request timeout", serviceName);
            return Results.Problem(
                detail: "Service request timeout",
                statusCode: 408,
                title: "Request Timeout");
        }
    }
}

// Uso en endpoints
app.MapGet("/api/events", async (HttpContext context, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    return await ErrorHandlingExtensions.HandleServiceCall(
        async () =>
        {
            var client = factory.CreateClient("CatalogService");
            return await client.GetAsync($"/api/events{context.Request.QueryString}");
        },
        logger,
        "CatalogService");
});
```

## Configuration Management

### appsettings.json
```json
{
  "Services": {
    "CatalogService": {
      "BaseUrl": "https://localhost:7001",
      "Timeout": "00:00:05",
      "RetryAttempts": 3
    },
    "BookingService": {
      "BaseUrl": "https://localhost:7002", 
      "Timeout": "00:00:10",
      "RetryAttempts": 2
    },
    "PaymentService": {
      "BaseUrl": "https://localhost:7003",
      "Timeout": "00:00:15",
      "RetryAttempts": 3
    }
  },
  "RateLimit": {
    "PermitLimit": 100,
    "WindowMinutes": 1
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://ticketwave.app"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization", "X-Correlation-ID"]
  }
}
```

### Environment-specific Configuration
```csharp
// Development
"Services:CatalogService:BaseUrl": "https://localhost:7001"

// Production
"Services:CatalogService:BaseUrl": "https://catalog.ticketwave.internal"

// Docker Compose
"Services:CatalogService:BaseUrl": "http://catalog-service:80"
```

## Response Composition (Implementación Futura)

```csharp
// Para casos que requieren agregación de múltiples servicios
app.MapGet("/api/events/{id}/booking-info", async (Guid id, IHttpClientFactory factory) =>
{
    var catalogClient = factory.CreateClient("CatalogService");
    var bookingClient = factory.CreateClient("BookingService");
    
    // Parallel calls
    var eventTask = catalogClient.GetFromJsonAsync<EventDto>($"/api/events/{id}");
    var availabilityTask = bookingClient.GetFromJsonAsync<AvailabilityDto>($"/api/events/{id}/availability");
    
    await Task.WhenAll(eventTask, availabilityTask);
    
    return Results.Ok(new
    {
        Event = eventTask.Result,
        Availability = availabilityTask.Result
    });
});
```

## Observabilidad

### Logging Structure
```csharp
_logger.LogInformation("Gateway request {Method} {Path} forwarded to {Service} [CorrelationId: {CorrelationId}] [Duration: {Duration}ms]",
    context.Request.Method,
    context.Request.Path,
    serviceName,
    correlationId,
    stopwatch.ElapsedMilliseconds);
```

### Metrics (Implementación Futura)
```csharp
// Prometheus metrics
private readonly Counter _requestsTotal = Metrics.CreateCounter("gateway_requests_total", "Total requests", "method", "service", "status");
private readonly Histogram _requestDuration = Metrics.CreateHistogram("gateway_request_duration_seconds", "Request duration", "service");

// Usage
_requestsTotal.WithTags(context.Request.Method, serviceName, response.StatusCode.ToString()).Inc();
_requestDuration.WithTags(serviceName).Observe(stopwatch.Elapsed.TotalSeconds);
```

## Consequences

### Advantages
- **Single Point of Entry**: Clients only need to know one endpoint
- **Cross-cutting Concerns**: Authentication, logging, rate limiting centralized
- **Service Discovery**: Clients don't need to know service locations
- **Flexibility**: Can change routing without affecting clients
- **Monitoring**: Centralized observability of all requests

### Disadvantages
- **Single Point of Failure**: Gateway down = entire system down
- **Performance Bottleneck**: All requests go through the gateway
- **Complexity**: Additional routing and error handling logic
- **Latency**: Additional hop on each request

### Mitigated Risks
- **High Availability**: Deploy multiple instances behind load balancer
- **Performance**: Keep gateway light, minimal processing
- **Circuit Breaking**: Fail fast when services are down
- **Caching**: Cache responses from slow services

## Alternatives Considered

### 1. Backend for Frontend (BFF)
**Advantages**: Optimized for each client type
**Disadvantages**: Multiple gateways to maintain

### 2. Service Mesh (Istio)
**Advantages**: Advanced features, sidecar pattern
**Disadvantages**: Operational complexity, learning curve

### 3. Managed API Gateway (Azure APIM)
**Advantages**: Enterprise ready features, less code
**Disadvantages**: Vendor lock-in, additional cost

### 4. No Gateway (Direct Service Access)
**Advantages**: Simplicity, no single point of failure
**Disadvantages**: Clients coupled to services, no cross-cutting concerns

## Roadmap de Features

### Fase 1 (MVP) - Implementado
- ✅ Reverse proxy básico
- ✅ Health check aggregation
- ✅ Error handling básico
- ✅ CORS configuration

### Fase 2 - Security & Observability
- JWT authentication
- Request correlation tracking
- Structured logging
- Basic metrics

### Fase 3 - Resilience
- Rate limiting
- Circuit breakers
- Retry policies con Polly
- Response caching

### Fase 4 - Advanced Features
- Request/Response transformation
- API versioning support
- WebSocket proxying
- Load balancing strategies

## Deployment Considerations

### Container Deployment
```dockerfile
# Dockerfile para Gateway
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "JCP.TicketWave.Gateway.dll"]
```

### Kubernetes Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  replicas: 3  # High availability
  selector:
    matchLabels:
      app: gateway
  template:
    spec:
      containers:
      - name: gateway
        image: ticketwave/gateway:latest
        ports:
        - containerPort: 80
        env:
        - name: Services__CatalogService__BaseUrl
          value: "http://catalog-service:80"
```

## Testing Strategy

### Integration Tests
```csharp
[Test]
public async Task Gateway_Should_Route_To_CatalogService()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/events");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    // Verify content structure
}
```

### Load Testing
```csharp
// NBomber load test
var scenario = Scenario.Create("gateway_load_test", async context =>
{
    var response = await httpClient.GetAsync("/api/events");
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(5))
);
```