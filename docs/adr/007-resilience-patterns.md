# ADR-007: Resilience Patterns and Fault Tolerance

## Status
Accepted

## Date
2025-11-26

## Context
A high-demand ticket sales system must be resilient to:

- **Service Failures**: Individual services may fail temporarily
- **Network Issues**: Latency, timeouts, intermittent connections
- **External Dependencies**: Payment APIs, email services may fail
- **Resource Exhaustion**: CPU, memory, DB connections under load
- **Cascading Failures**: A slow service can affect the entire system
- **Data Corruption**: Partial writes, corrupted messages

The system must maintain partial operability during failures and recover automatically.

## Decision
We implement a **multi-layer resilience strategy** with specific patterns per failure type:

### 1. Circuit Breaker Pattern
**For**: Prevent cascading failures in external service calls

### 2. Retry Policies with Exponential Backoff
**For**: Handle transient network and service failures

### 3. Bulkhead Pattern
**For**: Isolate critical resources from less important resources

### 4. Timeout Policies
**For**: Prevent slow requests from blocking the system

### 5. Fallback Mechanisms
**For**: Provide degraded functionality when services fail

### 6. Health Checks and Auto-Recovery
**For**: Early detection and automatic recovery

## Implementation by Pattern

### 1. Circuit Breaker Implementation

```csharp
// Polly Circuit Breaker configuration
public static class ResiliencePolicies
{
    public static AsyncCircuitBreakerPolicy CreateCircuitBreaker(string serviceName)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5, // Abre después de 5 fallos consecutivos
                durationOfBreak: TimeSpan.FromSeconds(30), // Mantiene abierto 30 segundos
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {ServiceName} for {Duration}s due to {Exception}",
                        serviceName, duration.TotalSeconds, exception.GetType().Name);
                    
                    // Metrics
                    _circuitBreakerOpenedCounter.WithTags(serviceName).Inc();
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker closed for {ServiceName}", serviceName);
                    _circuitBreakerResetCounter.WithTags(serviceName).Inc();
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker half-open for {ServiceName}", serviceName);
                });
    }
    
    public static AsyncRetryPolicy CreateRetryPolicy(string serviceName, int maxRetries = 3)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, delay, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for {ServiceName} after {Delay}ms delay. Exception: {Exception}",
                        retryCount, serviceName, delay.TotalMilliseconds, outcome.Exception?.Message);
                    
                    _retryAttemptCounter.WithTags(serviceName, retryCount.ToString()).Inc();
                });
    }
}

// HTTP Client configuration with resilience
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResilientHttpClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress,
        int timeoutSeconds = 10)
    {
        var circuitBreakerPolicy = ResiliencePolicies.CreateCircuitBreaker(clientName);
        var retryPolicy = ResiliencePolicies.CreateRetryPolicy(clientName);
        var timeoutPolicy = Policy.TimeoutAsync(timeoutSeconds);
        
        // Combine policies: Timeout → Retry → Circuit Breaker
        var combinedPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
        
        services.AddHttpClient(clientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds + 5); // Slightly higher than policy timeout
        })
        .AddPolicyHandler(combinedPolicy);
        
        return services;
    }
}
```

### 2. Service-Specific Resilience Strategies

#### API Gateway Resilience
```csharp
// Gateway con fallback para servicios críticos
public class ResilientGatewayService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResilientGatewayService> _logger;
    
    public async Task<IResult> GetEventsWithFallback(GetEventsQuery query)
    {
        var cacheKey = $"events_{query.Category}_{query.Page}_{query.PageSize}";
        
        try
        {
            // Primary: Try catalog service
            var client = _clientFactory.CreateClient("CatalogService");
            var response = await client.GetAsync($"/api/events?{query.ToQueryString()}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Cache successful response
                _cache.Set(cacheKey, content, TimeSpan.FromMinutes(5));
                
                return Results.Content(content, "application/json");
            }
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Catalog service circuit breaker is open, falling back to cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling catalog service, falling back to cache");
        }
        
        // Fallback: Return cached data
        if (_cache.TryGetValue(cacheKey, out string cachedContent))
        {
            _logger.LogInformation("Serving events from cache due to service unavailability");
            return Results.Content(cachedContent, "application/json", statusCode: 200);
        }
        
        // Last resort: Return minimal response
        var fallbackResponse = new
        {
            Events = Array.Empty<object>(),
            Message = "Event catalog is temporarily unavailable. Please try again later.",
            IsFromCache = false,
            IsFallback = true
        };
        
        return Results.Json(fallbackResponse, statusCode: 503);
    }
}
```

#### Booking Service Resilience
```csharp
// Booking service con database resilience
public class ResilientBookingService
{
    public async Task<BookingResult> CreateBookingWithResilience(CreateBookingCommand command)
    {
        var retryPolicy = Policy
            .Handle<SqlException>(ex => ex.Number == 1205) // Deadlock
            .Or<SqlException>(ex => ex.Number == -2) // Timeout
            .Or<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)),
                onRetry: (outcome, delay, retryCount, context) =>
                {
                    _logger.LogWarning("Database retry {RetryCount} for booking creation after {Delay}ms. Exception: {Exception}",
                        retryCount, delay.TotalMilliseconds, outcome.Exception?.Message);
                });
        
        try
        {
            return await retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                
                // Create booking with optimistic locking
                var booking = new Booking(command);
                await repository.AddAsync(booking);
                
                return new BookingResult { Success = true, BookingId = booking.Id };
            });
        }
        catch (InsufficientTicketsException)
        {
            // Business exception - don't retry
            return new BookingResult 
            { 
                Success = false, 
                ErrorCode = "INSUFFICIENT_TICKETS",
                Message = "Not enough tickets available for this event" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create booking after all retries");
            return new BookingResult 
            { 
                Success = false, 
                ErrorCode = "BOOKING_FAILED",
                Message = "Unable to process booking at this time" 
            };
        }
    }
}
```

#### Payment Service Resilience
```csharp
// Payment service con external API resilience
public class ResilientPaymentService
{
    private readonly Dictionary<string, IAsyncPolicy> _providerPolicies;
    
    public ResilientPaymentService()
    {
        _providerPolicies = new Dictionary<string, IAsyncPolicy>
        {
            ["stripe"] = CreateStripePolicy(),
            ["paypal"] = CreatePayPalPolicy()
        };
    }
    
    public async Task<PaymentResult> ProcessPaymentWithFallback(ProcessPaymentCommand command)
    {
        var primaryProvider = "stripe";
        var fallbackProvider = "paypal";
        
        // Try primary provider
        var result = await TryProcessPayment(command, primaryProvider);
        if (result.Success)
        {
            return result;
        }
        
        // Try fallback provider
        _logger.LogWarning("Primary payment provider {Provider} failed, trying fallback {Fallback}",
            primaryProvider, fallbackProvider);
            
        command = command with { PaymentProvider = fallbackProvider }; // Assuming record type
        result = await TryProcessPayment(command, fallbackProvider);
        
        if (result.Success)
        {
            result.UsedFallback = true;
            return result;
        }
        
        // Both providers failed
        return new PaymentResult
        {
            Success = false,
            ErrorCode = "ALL_PROVIDERS_FAILED",
            Message = "Payment processing is temporarily unavailable"
        };
    }
    
    private async Task<PaymentResult> TryProcessPayment(ProcessPaymentCommand command, string provider)
    {
        var policy = _providerPolicies[provider];
        
        try
        {
            return await policy.ExecuteAsync(async () =>
            {
                var paymentProvider = _serviceProvider.GetRequiredKeyedService<IPaymentProvider>(provider);
                return await paymentProvider.ProcessAsync(command);
            });
        }
        catch (BrokenCircuitException)
        {
            return new PaymentResult 
            { 
                Success = false, 
                ErrorCode = "PROVIDER_UNAVAILABLE",
                Message = $"Payment provider {provider} is currently unavailable" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed with provider {Provider}", provider);
            return new PaymentResult 
            { 
                Success = false, 
                ErrorCode = "PROCESSING_ERROR",
                Message = "Payment processing encountered an error" 
            };
        }
    }
    
    private IAsyncPolicy CreateStripePolicy()
    {
        var circuitBreaker = Policy
            .Handle<StripeException>()
            .CircuitBreakerAsync(3, TimeSpan.FromMinutes(2));
            
        var retry = Policy
            .Handle<StripeException>(ex => ex.HttpStatusCode >= 500)
            .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));
            
        return Policy.WrapAsync(circuitBreaker, retry);
    }
}
```

### 3. Bulkhead Pattern Implementation

```csharp
// Separate thread pools para different workloads
public class BulkheadConfiguration
{
    public static void ConfigureThreadPools(IServiceCollection services)
    {
        // Critical operations (bookings) - dedicated thread pool
        services.AddSingleton<ICriticalOperationExecutor>(provider =>
            new DedicatedThreadPoolExecutor("Critical", maxThreads: 10));
        
        // Background operations (notifications) - separate thread pool  
        services.AddSingleton<IBackgroundOperationExecutor>(provider =>
            new DedicatedThreadPoolExecutor("Background", maxThreads: 5));
        
        // External API calls - limited concurrency
        services.AddSingleton<IExternalApiExecutor>(provider =>
            new SemaphoreBasedExecutor("ExternalAPI", maxConcurrency: 20));
    }
}

public class SemaphoreBasedExecutor : IExternalApiExecutor
{
    private readonly SemaphoreSlim _semaphore;
    private readonly string _name;
    private readonly ILogger<SemaphoreBasedExecutor> _logger;
    
    public SemaphoreBasedExecutor(string name, int maxConcurrency)
    {
        _name = name;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SemaphoreBasedExecutor>();
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
        if (!acquired)
        {
            throw new ResourceExhaustionException($"Could not acquire semaphore for {_name} executor within timeout");
        }
        
        try
        {
            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

// Usage in services
public class BookingService
{
    private readonly ICriticalOperationExecutor _criticalExecutor;
    
    public async Task<BookingResult> CreateBooking(CreateBookingCommand command)
    {
        // Use critical thread pool for booking operations
        return await _criticalExecutor.ExecuteAsync(async () =>
        {
            // Actual booking logic here
            return await ProcessBookingInternal(command);
        });
    }
}
```

### 4. Health Checks y Auto-Recovery

```csharp
// Comprehensive health checks
public class ServiceHealthChecks
{
    public static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            // Database health
            .AddSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                name: "database",
                tags: new[] { "critical", "database" })
            
            // External services health
            .AddUrlGroup(
                new Uri($"{configuration["Services:CatalogService:BaseUrl"]}/health"),
                name: "catalog-service",
                tags: new[] { "service", "catalog" })
            
            // Memory usage check
            .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "resource" })
            
            // Custom business logic health
            .AddCheck<BookingConsistencyHealthCheck>("booking-consistency", tags: new[] { "business" });
    }
}

public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var memoryUsed = GC.GetTotalMemory(false);
        var maxMemory = 1024L * 1024L * 1024L; // 1GB threshold
        
        var status = memoryUsed < maxMemory ? HealthStatus.Healthy : HealthStatus.Degraded;
        
        var data = new Dictionary<string, object>
        {
            ["MemoryUsed"] = memoryUsed,
            ["MaxMemory"] = maxMemory,
            ["UsagePercentage"] = (double)memoryUsed / maxMemory * 100
        };
        
        return Task.FromResult(new HealthCheckResult(status, data: data));
    }
}

// Auto-recovery mechanism
public class HealthMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthMonitoringService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRecoverServices();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in health monitoring");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
    
    private async Task CheckAndRecoverServices()
    {
        using var scope = _serviceProvider.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        
        var healthReport = await healthCheckService.CheckHealthAsync();
        
        foreach (var entry in healthReport.Entries)
        {
            if (entry.Value.Status == HealthStatus.Unhealthy)
            {
                _logger.LogWarning("Health check {HealthCheck} is unhealthy: {Description}",
                    entry.Key, entry.Value.Description);
                
                // Attempt recovery based on health check type
                await AttemptRecovery(entry.Key, entry.Value);
            }
        }
    }
    
    private async Task AttemptRecovery(string healthCheckName, HealthCheckResult result)
    {
        switch (healthCheckName)
        {
            case "memory":
                // Trigger garbage collection
                _logger.LogInformation("Triggering garbage collection due to high memory usage");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                break;
                
            case "booking-consistency":
                // Trigger cleanup job
                var cleanupService = _serviceProvider.GetRequiredService<ReservationCleanupService>();
                await cleanupService.CleanupExpiredReservations();
                break;
                
            case "database":
                // Reset connection pool
                await ResetDatabaseConnections();
                break;
        }
    }
}
```

### 5. Graceful Degradation Strategies

```csharp
// Feature flags para graceful degradation
public class FeatureFlags
{
    public bool EnableRealTimeInventory { get; set; } = true;
    public bool EnableExternalPaymentProviders { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnablePdfGeneration { get; set; } = true;
    public int MaxConcurrentBookings { get; set; } = 100;
}

public class GracefulDegradationService
{
    private readonly FeatureFlags _featureFlags;
    private readonly ILogger<GracefulDegradationService> _logger;
    
    public async Task<BookingResult> CreateBookingWithDegradation(CreateBookingCommand command)
    {
        // Check system load
        var systemLoad = await GetCurrentSystemLoad();
        
        if (systemLoad > 0.8) // 80% load threshold
        {
            _logger.LogWarning("System under high load ({Load:P}), enabling degraded mode", systemLoad);
            return await CreateBookingDegraded(command);
        }
        
        return await CreateBookingNormal(command);
    }
    
    private async Task<BookingResult> CreateBookingDegraded(CreateBookingCommand command)
    {
        // Simplified booking flow under high load
        var result = await _bookingService.CreateSimplifiedBooking(command);
        
        if (result.Success)
        {
            // Queue non-critical operations for later
            await _backgroundQueue.QueueAsync(new SendConfirmationEmailTask(result.BookingId));
            
            if (_featureFlags.EnablePdfGeneration)
            {
                await _backgroundQueue.QueueAsync(new GeneratePdfTicketTask(result.BookingId));
            }
        }
        
        return result;
    }
    
    public async Task<EventsResult> GetEventsWithDegradation(GetEventsQuery query)
    {
        if (!_featureFlags.EnableRealTimeInventory)
        {
            _logger.LogInformation("Real-time inventory disabled, using cached data");
            return await GetEventsCached(query);
        }
        
        try
        {
            return await GetEventsRealTime(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Real-time events failed, falling back to cache");
            return await GetEventsCached(query);
        }
    }
}
```

## Monitoring y Metrics

### Resilience Metrics
```csharp
// Prometheus metrics para resilience patterns
public static class ResilienceMetrics
{
    public static readonly Counter CircuitBreakerOpened = Metrics
        .CreateCounter("circuit_breaker_opened_total", "Circuit breakers opened", "service");
    
    public static readonly Counter RetryAttempts = Metrics
        .CreateCounter("retry_attempts_total", "Retry attempts", "service", "attempt");
    
    public static readonly Histogram FallbackDuration = Metrics
        .CreateHistogram("fallback_duration_seconds", "Time spent in fallback mode", "service");
    
    public static readonly Gauge DegradedModeActive = Metrics
        .CreateGauge("degraded_mode_active", "Services in degraded mode", "service");
    
    public static readonly Counter HealthCheckFailures = Metrics
        .CreateCounter("health_check_failures_total", "Health check failures", "check_name");
}

// Alerting rules (Prometheus AlertManager)
/*
groups:
- name: ticketwave.resilience
  rules:
  - alert: CircuitBreakerOpenTooLong
    expr: circuit_breaker_opened_total > 0 and time() - circuit_breaker_opened_total > 300
    for: 5m
    labels:
      severity: critical
    annotations:
      summary: "Circuit breaker for {{ $labels.service }} has been open for more than 5 minutes"
      
  - alert: HighRetryRate
    expr: rate(retry_attempts_total[5m]) > 10
    for: 2m
    labels:
      severity: warning
    annotations:
      summary: "High retry rate detected for {{ $labels.service }}"
      
  - alert: DegradedModeActive
    expr: degraded_mode_active > 0
    for: 1m
    labels:
      severity: warning
    annotations:
      summary: "Service {{ $labels.service }} is running in degraded mode"
*/
```

## Testing Resilience

### Chaos Engineering
```csharp
// Chaos monkey implementation
public class ChaosMonkeyService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue<bool>("ChaosMonkey:Enabled"))
            return;
            
        while (!stoppingToken.IsCancellationRequested)
        {
            var chaosAction = SelectRandomChaosAction();
            await ExecuteChaosAction(chaosAction);
            
            var delay = TimeSpan.FromMinutes(_random.Next(5, 30));
            await Task.Delay(delay, stoppingToken);
        }
    }
    
    private async Task ExecuteChaosAction(ChaosAction action)
    {
        switch (action)
        {
            case ChaosAction.KillRandomService:
                await KillRandomServiceInstance();
                break;
                
            case ChaosAction.InjectLatency:
                await InjectRandomLatency();
                break;
                
            case ChaosAction.FillMemory:
                await ConsumeMemory();
                break;
                
            case ChaosAction.DisruptDatabase:
                await DisruptDatabaseConnections();
                break;
        }
    }
}

// Resilience integration tests
[Test]
public async Task Booking_ShouldSucceed_WhenPaymentServiceIsTemporarilyUnavailable()
{
    // Arrange
    _paymentServiceMock.Setup(x => x.ProcessPayment(It.IsAny<ProcessPaymentCommand>()))
        .ThrowsAsync(new HttpRequestException())
        .Callback(() => Thread.Sleep(100)); // Simulate slow failure
    
    // Act
    var result = await _bookingService.CreateBooking(new CreateBookingCommand
    {
        EventId = Guid.NewGuid(),
        UserId = "test-user",
        TicketCount = 2
    });
    
    // Assert
    result.Success.Should().BeTrue();
    result.Status.Should().Be(BookingStatus.PaymentPending); // Graceful degradation
}

[Test]
public async Task CircuitBreaker_ShouldOpenAfter_ConsecutiveFailures()
{
    // Arrange - Configure service to fail
    for (int i = 0; i < 6; i++) // More than circuit breaker threshold
    {
        try
        {
            await _httpClient.GetAsync("/api/events");
        }
        catch { }
    }
    
    // Act - Next call should be circuit breaker rejection
    var exception = await Assert.ThrowsAsync<BrokenCircuitException>(
        () => _httpClient.GetAsync("/api/events"));
    
    // Assert
    exception.Should().NotBeNull();
}
```

## Consequences

### Advantages
- **Fault Isolation**: Failures in one component don't affect the entire system
- **Graceful Degradation**: System maintains basic functionality during failures
- **Auto-Recovery**: System automatically recovers from temporary failures
- **Observable**: Metrics and logging provide complete visibility
- **Testable**: Patterns can be validated with chaos engineering

### Disadvantages
- **Complexity**: Multiple patterns increase code complexity
- **Performance Overhead**: Retry policies and circuit breakers add latency
- **Configuration Management**: Multiple timeouts and thresholds to configure
- **Testing Complexity**: Resilience testing requires specialized infrastructure

### Mitigated Risks
- **Cascading Failures**: Circuit breakers prevent propagation
- **Resource Exhaustion**: Bulkheads limit impact
- **Data Corruption**: Transactional patterns with rollback
- **Extended Downtime**: Auto-recovery reduces time to resolution

## Roadmap de Implementación

### Fase 1: Basic Resilience
- ✅ HTTP client timeouts
- ✅ Basic retry policies
- ✅ Health checks
- ✅ Error handling

### Fase 2: Advanced Patterns
- Circuit breakers con Polly
- Bulkhead isolation
- Graceful degradation
- Background auto-recovery

### Fase 3: Observability
- Resilience metrics
- Distributed tracing
- Alerting rules
- Dashboards

### Fase 4: Chaos Engineering
- Chaos monkey implementation
- Resilience testing automation
- Game days y disaster recovery drills