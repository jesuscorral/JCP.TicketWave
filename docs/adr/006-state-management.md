# ADR-006: State Management and Transactions Strategy

## Status
Accepted

## Date
2025-11-26

## Context
A ticket sales system handles multiple critical states that require consistency:

- **Inventory Management**: Real-time ticket availability
- **Booking Lifecycle**: Reservation states (Pending → Confirmed → Expired)
- **Payment Processing**: Payment states (Initiated → Processing → Completed/Failed)
- **Concurrent Access**: Multiple users buying tickets simultaneously
- **Data Consistency**: Between distributed services without global transactions

Specific challenges:
- **Overbooking Prevention**: Don't sell more tickets than available
- **Race Conditions**: Multiple users reserving the last tickets
- **Partial Failures**: What to do if payment fails after reserving
- **Timeouts**: Automatic release of unpaid reservations

## Decision
We adopt a **hybrid state management strategy** adapted per service:

### 1. Booking Service: Pessimistic Locking + State Machine
**For**: Prevent overbooking in high concurrency scenarios

### 2. Payment Service: Optimistic Locking + Idempotency
**For**: Safe payment handling with automatic retry

### 3. Catalog Service: Eventually Consistent + Cache Invalidation
**For**: Performance in reads with asynchronous updates

### 4. Cross-Service: Saga Pattern + Compensating Actions
**For**: Eventual consistency between services

## Implementation per Service

### Booking Service - Pessimistic Locking Strategy

```csharp
// Entity with row versioning
[Table("Bookings")]
public class Booking : BaseEntity
{
    public string BookingReference { get; set; }
    public Guid EventId { get; set; }
    public string UserId { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    [Timestamp]
    public byte[] Version { get; set; } // For optimistic locking
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3
}

// Repository con locking
public class BookingRepository : IBookingRepository
{
    public async Task<TicketReservation> ReserveTicketsWithLock(
        Guid eventId, 
        int ticketCount,
        TimeSpan lockTimeout,
        CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, // Highest isolation level
            cancellationToken);
        
        try
        {
            // Lock tickets for update
            var availableTickets = await _dbContext.TicketInventory
                .FromSqlRaw(@"
                    SELECT * FROM TicketInventory 
                    WHERE EventId = {0} AND Status = {1}
                    FOR UPDATE NOWAIT", // PostgreSQL syntax
                    eventId, TicketStatus.Available)
                .Take(ticketCount)
                .ToListAsync(cancellationToken);
            
            if (availableTickets.Count < ticketCount)
            {
                throw new InsufficientTicketsException(
                    $"Only {availableTickets.Count} tickets available, requested {ticketCount}");
            }
            
            // Update ticket status atomically
            foreach (var ticket in availableTickets)
            {
                ticket.Status = TicketStatus.Reserved;
                ticket.ReservedAt = DateTime.UtcNow;
                ticket.ReservationExpires = DateTime.UtcNow.Add(lockTimeout);
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return new TicketReservation
            {
                ReservationId = Guid.NewGuid(),
                TicketIds = availableTickets.Select(t => t.Id).ToList(),
                ExpiresAt = DateTime.UtcNow.Add(lockTimeout)
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

### State Machine Implementation
```csharp
public class BookingStateMachine
{
    private static readonly Dictionary<(BookingStatus From, BookingEvent Event), BookingStatus> _transitions = new()
    {
        // Valid transitions
        { (BookingStatus.Pending, BookingEvent.PaymentCompleted), BookingStatus.Confirmed },
        { (BookingStatus.Pending, BookingEvent.PaymentFailed), BookingStatus.Cancelled },
        { (BookingStatus.Pending, BookingEvent.ReservationExpired), BookingStatus.Expired },
        { (BookingStatus.Confirmed, BookingEvent.RefundProcessed), BookingStatus.Cancelled },
        
        // Invalid transitions will throw exception
    };
    
    public static BookingStatus Transition(BookingStatus currentState, BookingEvent bookingEvent)
    {
        if (_transitions.TryGetValue((currentState, bookingEvent), out var newState))
        {
            return newState;
        }
        
        throw new InvalidStateTransitionException(
            $"Cannot transition from {currentState} with event {bookingEvent}");
    }
    
    public static bool CanTransition(BookingStatus currentState, BookingEvent bookingEvent)
    {
        return _transitions.ContainsKey((currentState, bookingEvent));
    }
}

public enum BookingEvent
{
    PaymentCompleted,
    PaymentFailed,
    ReservationExpired,
    RefundProcessed,
    ManualCancellation
}

// Usage in service
public async Task ConfirmBooking(Guid bookingId, Guid paymentId)
{
    var booking = await _repository.GetByIdAsync(bookingId);
    
    if (!BookingStateMachine.CanTransition(booking.Status, BookingEvent.PaymentCompleted))
    {
        throw new InvalidOperationException($"Cannot confirm booking in status {booking.Status}");
    }
    
    booking.Status = BookingStateMachine.Transition(booking.Status, BookingEvent.PaymentCompleted);
    booking.PaymentId = paymentId;
    booking.ConfirmedAt = DateTime.UtcNow;
    
    await _repository.UpdateAsync(booking);
}
```

### Payment Service - Idempotency + Optimistic Locking

```csharp
[Table("Payments")]
public class Payment : BaseEntity
{
    public string IdempotencyKey { get; set; } // Unique per request
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public PaymentStatus Status { get; set; }
    public string ExternalTransactionId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    [Timestamp]
    public byte[] Version { get; set; }
}

// Idempotency implementation
public class PaymentService : IPaymentService
{
    public async Task<PaymentResult> ProcessPayment(ProcessPaymentCommand command)
    {
        // Check for existing payment with same idempotency key
        var existingPayment = await _repository.GetByIdempotencyKeyAsync(command.IdempotencyKey);
        if (existingPayment != null)
        {
            _logger.LogInformation("Returning cached result for idempotency key {IdempotencyKey}", 
                command.IdempotencyKey);
            return MapToResult(existingPayment);
        }
        
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = command.IdempotencyKey,
            BookingId = command.BookingId,
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.Pending
        };
        
        try
        {
            // Persist payment record first
            await _repository.AddAsync(payment);
            
            // Process with external service
            var externalResult = await _externalPaymentService.ProcessAsync(
                payment.Id.ToString(),
                command.Amount,
                command.PaymentMethodId);
            
            // Update payment with result
            payment.Status = externalResult.IsSuccess ? PaymentStatus.Succeeded : PaymentStatus.Failed;
            payment.ExternalTransactionId = externalResult.TransactionId;
            payment.ProcessedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(payment);
            
            return MapToResult(payment);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle optimistic locking conflict
            _logger.LogWarning("Concurrency conflict updating payment {PaymentId}", payment.Id);
            throw new PaymentConcurrencyException("Payment was modified by another process");
        }
        catch (ExternalServiceException ex)
        {
            // Handle external service failures
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = ex.Message;
            await _repository.UpdateAsync(payment);
            
            throw new PaymentProcessingException("Payment processing failed", ex);
        }
    }
}
```

### Timeout and Cleanup Background Service

```csharp
// Background service para cleanup automático
public class ReservationCleanupService : BackgroundService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<ReservationCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredReservations();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reservation cleanup");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retry
            }
        }
    }
    
    private async Task CleanupExpiredReservations()
    {
        var expiredBookings = await _bookingRepository.GetExpiredPendingBookings();
        
        foreach (var booking in expiredBookings)
        {
            try
            {
                // Transition to expired state
                booking.Status = BookingStateMachine.Transition(
                    booking.Status, 
                    BookingEvent.ReservationExpired);
                
                // Release reserved tickets
                await _ticketService.ReleaseReservedTickets(booking.Id);
                
                await _bookingRepository.UpdateAsync(booking);
                
                _logger.LogInformation("Expired booking {BookingId} cleaned up", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup booking {BookingId}", booking.Id);
            }
        }
    }
}
```

## Distributed Transaction Patterns

### Saga Pattern Implementation

```csharp
// Saga Orchestrator para booking completo
public class BookingProcessSaga : ISaga
{
    public Guid SagaId { get; set; }
    public SagaState State { get; set; }
    public BookingProcessData Data { get; set; }
    
    public async Task<SagaResult> ProcessStep(SagaStepContext context)
    {
        return State switch
        {
            SagaState.Started => await ReserveTickets(context),
            SagaState.TicketsReserved => await InitiatePayment(context),
            SagaState.PaymentInitiated => await WaitForPaymentResult(context),
            SagaState.PaymentCompleted => await ConfirmBooking(context),
            SagaState.PaymentFailed => await CompensateTicketReservation(context),
            SagaState.Completed => SagaResult.Completed,
            SagaState.Compensated => SagaResult.Compensated,
            _ => throw new InvalidOperationException($"Unknown saga state: {State}")
        };
    }
    
    private async Task<SagaResult> ReserveTickets(SagaStepContext context)
    {
        try
        {
            var reservation = await _bookingService.ReserveTickets(
                Data.EventId, 
                Data.TicketCount,
                TimeSpan.FromMinutes(15));
            
            Data.ReservationId = reservation.ReservationId;
            State = SagaState.TicketsReserved;
            
            return SagaResult.Continue;
        }
        catch (InsufficientTicketsException)
        {
            State = SagaState.Failed;
            return SagaResult.Failed("Insufficient tickets available");
        }
    }
    
    private async Task<SagaResult> CompensateTicketReservation(SagaStepContext context)
    {
        try
        {
            await _bookingService.ReleaseReservation(Data.ReservationId);
            State = SagaState.Compensated;
            return SagaResult.Compensated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compensate ticket reservation {ReservationId}", 
                Data.ReservationId);
            return SagaResult.CompensationFailed;
        }
    }
}

public class BookingProcessData
{
    public Guid EventId { get; set; }
    public string UserId { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? ReservationId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? BookingId { get; set; }
}
```

### Event Sourcing for Audit Trail

```csharp
// Para Payment Service - complete audit trail
public abstract class PaymentEvent
{
    public Guid PaymentId { get; set; }
    public Guid StreamId => PaymentId; // Event stream per payment
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
    public long Version { get; set; }
}

public class PaymentInitiated : PaymentEvent
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string IdempotencyKey { get; set; }
}

public class PaymentProcessing : PaymentEvent
{
    public string ExternalTransactionId { get; set; }
    public string PaymentProvider { get; set; }
}

public class PaymentCompleted : PaymentEvent
{
    public string ExternalTransactionId { get; set; }
    public decimal ProcessedAmount { get; set; }
    public Dictionary<string, string> ProviderMetadata { get; set; }
}

// Event Store Repository
public class PaymentEventStore : IPaymentEventStore
{
    public async Task<IEnumerable<PaymentEvent>> GetEventsAsync(Guid paymentId, long fromVersion = 0)
    {
        return await _dbContext.PaymentEvents
            .Where(e => e.PaymentId == paymentId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync();
    }
    
    public async Task AppendEventAsync(PaymentEvent paymentEvent)
    {
        // Optimistic concurrency control
        var currentVersion = await GetCurrentVersionAsync(paymentEvent.PaymentId);
        paymentEvent.Version = currentVersion + 1;
        
        try
        {
            _dbContext.PaymentEvents.Add(paymentEvent);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            throw new ConcurrencyException("Event stream was modified by another process");
        }
    }
}

// Payment Aggregate reconstruction
public class Payment
{
    public Guid Id { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public List<PaymentEvent> UncommittedEvents { get; private set; } = new();
    
    public static async Task<Payment> LoadFromEvents(Guid paymentId, IPaymentEventStore eventStore)
    {
        var events = await eventStore.GetEventsAsync(paymentId);
        var payment = new Payment();
        
        foreach (var evt in events)
        {
            payment.Apply(evt);
        }
        
        return payment;
    }
    
    private void Apply(PaymentEvent evt)
    {
        switch (evt)
        {
            case PaymentInitiated initiated:
                Id = initiated.PaymentId;
                Amount = initiated.Amount;
                Status = PaymentStatus.Initiated;
                break;
                
            case PaymentCompleted completed:
                Status = PaymentStatus.Completed;
                break;
                
            // ... other events
        }
    }
}
```

## Monitoring and Alerting

### State Transition Metrics
```csharp
// Prometheus metrics para monitoreo
private readonly Counter _stateTransitionsTotal = Metrics
    .CreateCounter("booking_state_transitions_total", "Total state transitions", "from_state", "to_state", "event");

private readonly Histogram _reservationDuration = Metrics
    .CreateHistogram("ticket_reservation_duration_seconds", "Time tickets are reserved");

private readonly Gauge _pendingReservations = Metrics
    .CreateGauge("pending_reservations_total", "Number of pending reservations");

// Usage
public async Task TransitionBookingState(Booking booking, BookingEvent evt)
{
    var oldState = booking.Status;
    var newState = BookingStateMachine.Transition(oldState, evt);
    
    booking.Status = newState;
    await _repository.UpdateAsync(booking);
    
    // Record metrics
    _stateTransitionsTotal
        .WithTags(oldState.ToString(), newState.ToString(), evt.ToString())
        .Inc();
        
    if (newState == BookingStatus.Confirmed || newState == BookingStatus.Cancelled)
    {
        var duration = (DateTime.UtcNow - booking.CreatedAt).TotalSeconds;
        _reservationDuration.Observe(duration);
        _pendingReservations.Dec();
    }
}
```

### Health Checks for State Consistency
```csharp
public class BookingConsistencyHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        var issues = new List<string>();
        
        // Check for orphaned reservations
        var orphanedReservations = await _repository.GetOrphanedReservationsAsync();
        if (orphanedReservations.Any())
        {
            issues.Add($"{orphanedReservations.Count} orphaned reservations found");
        }
        
        // Check for expired but not cleaned up bookings
        var expiredPendingBookings = await _repository.GetExpiredPendingBookings();
        if (expiredPendingBookings.Count > 10) // Threshold
        {
            issues.Add($"{expiredPendingBookings.Count} expired bookings need cleanup");
        }
        
        // Check for stuck payments
        var stuckPayments = await _paymentRepository.GetStuckPaymentsAsync();
        if (stuckPayments.Any())
        {
            issues.Add($"{stuckPayments.Count} payments stuck in processing state");
        }
        
        return issues.Any() 
            ? HealthCheckResult.Degraded($"State consistency issues: {string.Join(", ", issues)}")
            : HealthCheckResult.Healthy("All state consistency checks passed");
    }
}
```

## Consequences

### Advantages
- **Data Integrity**: Pessimistic locking prevents overbooking
- **Auditability**: Event sourcing provides complete audit trail
- **Resilience**: Saga pattern handles distributed failures
- **Performance**: Optimistic locking for low contention scenarios
- **Consistency**: State machines prevent invalid transitions

### Disadvantages
- **Complexity**: Multiple patterns increase complexity
- **Performance Impact**: Locking can create bottlenecks
- **Storage Overhead**: Event sourcing requires more space
- **Debugging**: Distributed transactions harder to debug

### Mitigated Risks
- **Deadlocks**: Timeout policies and retry logic
- **Orphaned Resources**: Background cleanup services
- **Inconsistent State**: Health checks and monitoring
- **Performance Degradation**: Circuit breakers and fallback mechanisms

## Testing Strategies

### Concurrency Testing
```csharp
[Test]
public async Task ReserveTickets_ConcurrentRequests_ShouldNotOverbook()
{
    // Arrange
    const int availableTickets = 100;
    const int concurrentRequests = 50;
    const int ticketsPerRequest = 3;
    
    await SetupEventWithTickets(eventId, availableTickets);
    
    // Act - Fire concurrent requests
    var tasks = Enumerable.Range(0, concurrentRequests)
        .Select(i => _bookingService.ReserveTickets(eventId, ticketsPerRequest))
        .ToArray();
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    var successfulReservations = results.Count(r => r.Success);
    var reservedTickets = successfulReservations * ticketsPerRequest;
    
    reservedTickets.Should().BeLessOrEqualTo(availableTickets);
}

[Test]
public async Task Saga_PaymentFailure_ShouldCompensateReservation()
{
    // Arrange
    var sagaData = new BookingProcessData { /* ... */ };
    var saga = new BookingProcessSaga();
    
    // Simulate payment failure
    _paymentService.Setup(x => x.ProcessPayment(It.IsAny<ProcessPaymentCommand>()))
        .ThrowsAsync(new PaymentFailedException());
    
    // Act
    var result = await saga.ProcessStep(sagaData);
    
    // Assert
    result.Should().Be(SagaResult.Compensated);
    // Verify tickets were released
}
```

### Load Testing for State Management
```csharp
// NBomber scenario for booking under load
var bookingScenario = Scenario.Create("booking_load", async context =>
{
    var eventId = GetRandomPopularEvent();
    var request = new ReserveTicketsRequest
    {
        EventId = eventId,
        TicketCount = Random.Shared.Next(1, 5),
        UserId = $"user_{context.ScenarioInfo.ThreadId}_{context.InvocationNumber}"
    };
    
    var response = await httpClient.PostAsJsonAsync("/api/tickets/reserve", request);
    
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(10))
);
```