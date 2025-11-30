# Inter-Service Communication Patterns

## Communication Overview

```mermaid
graph TB
    %% Services
    AG[🚪 API Gateway]
    CS[📋 Catalog Service]
    BS[🎫 Booking Service]
    PS[💰 Payment Service]
    NS[📨 Notification Service]
    
    %% Message Broker
    MB[📡 Azure Service Bus]
    
    %% Synchronous Communication (HTTP/REST)
    AG -.->|HTTP/REST<br/>Synchronous| CS
    AG -.->|HTTP/REST<br/>Synchronous| BS
    AG -.->|HTTP/REST<br/>Synchronous| PS
    PS -.->|HTTP/REST<br/>Validation| BS
    
    %% Asynchronous Communication (Events)
    BS -->|Events<br/>Asynchronous| MB
    PS -->|Events<br/>Asynchronous| MB
    CS -->|Events<br/>Asynchronous| MB
    
    MB -->|Events<br/>Consumption| NS
    MB -->|Events<br/>Consumption| BS
    MB -->|Events<br/>Consumption| PS
    
    %% External Services
    PS -.->|HTTP/REST<br/>External API| PaymentGW[💳 Payment Gateway]
    NS -.->|HTTP/REST<br/>External API| EmailSvc[📧 Email Service]
    
    %% Legend
    subgraph "Legend"
        SyncLegend[Synchronous HTTP/REST]
        AsyncLegend[Asynchronous Events]
        ExtLegend[External API calls]
    end
    
    style AG fill:#fff9c4
    style CS fill:#e3f2fd
    style BS fill:#e8f5e8
    style PS fill:#fff3e0
    style NS fill:#f3e5f5
    style MB fill:#fce4ec
    style PaymentGW fill:#ffebee
    style EmailSvc fill:#e8eaf6
```

## Synchronous Communication Patterns

### 1. API Gateway → Microservices (Request/Response)

```mermaid
sequenceDiagram
    participant Client as Cliente
    participant AG as API Gateway
    participant Auth as Auth Service
    participant CS as Catalog Service
    
    Note over Client, CS: Request/Response Pattern with Authentication
    
    Client->>AG: GET /api/events
    
    rect rgb(255, 248, 220)
        Note over AG, Auth: Authentication Validation
        AG->>Auth: Validate JWT token
        Auth->>AG: Token valid + user claims
    end
    
    rect rgb(245, 255, 245)
        Note over AG, CS: Proxy Request
        AG->>CS: GET /events (with user context)
        CS->>CS: Apply user-specific filters
        CS->>AG: Events list + metadata
    end
    
    rect rgb(240, 248, 255)
        Note over AG, Client: Response Aggregation
        AG->>AG: Add CORS headers
        AG->>AG: Apply rate limiting headers
        AG->>AG: Add request tracking ID
        AG->>Client: Enhanced response
    end
```

### 2. Service-to-Service Validation

```mermaid
sequenceDiagram
    participant AG as API Gateway
    participant PS as Payment Service
    participant BS as Booking Service
    
    Note over AG, BS: Cross-Service Validation
    
    AG->>PS: POST /payments {bookingId, amount}
    
    rect rgb(255, 245, 238)
        Note over PS, BS: Business Validation
        PS->>BS: GET /bookings/{bookingId}/validate
        BS->>BS: Check booking status
        BS->>BS: Verify amount matches
        BS->>BS: Ensure booking not expired
        BS->>PS: Validation result + booking details
    end
    
    alt Validation Success
        PS->>PS: Process payment
        PS->>AG: Payment success
    else Validation Failed
        PS->>AG: Error: Invalid booking
    end
```

### 3. Circuit Breaker Pattern in Synchronous Communication

```mermaid
stateDiagram-v2
    [*] --> Closed: Circuit Closed
    
    Closed --> Open: Failure threshold exceeded<br/>(5 consecutive failures)
    Closed --> Closed: Successful requests
    
    Open --> HalfOpen: Timeout period<br/>(30 seconds)
    Open --> Open: All requests rejected
    
    HalfOpen --> Closed: Test request successful
    HalfOpen --> Open: Test request failed
    
    note right of Closed
        Normal operation
        All requests pass through
    end note
    
    note right of Open
        Fast-fail mode
        Return cached response
        or degraded service
    end note
    
    note right of HalfOpen
        Limited test requests
        Single request to test
        service health
    end note
```

## Asynchronous Communication Patterns

### 1. Event-Driven Communication

```mermaid
graph TD
    subgraph "Event Producers"
        BS[🎫 Booking Service]
        PS[💰 Payment Service]
        CS[📋 Catalog Service]
    end
    
    subgraph "Event Infrastructure"
        MB[📡 Azure Service Bus]
        
        subgraph "Topics"
            BookingTopic[booking-events]
            PaymentTopic[payment-events]
            CatalogTopic[catalog-events]
        end
        
        subgraph "Subscriptions"
            BookingSub[booking-subscription]
            PaymentSub[payment-subscription]
            NotificationSub[notification-subscription]
        end
    end
    
    subgraph "Event Consumers"
        NS[📨 Notification Service]
        ReportingService[📊 Reporting Service]
        AuditService[📋 Audit Service]
    end
    
    %% Event Publishing
    BS -->|BookingCreated<br/>BookingConfirmed<br/>BookingCancelled| BookingTopic
    PS -->|PaymentCompleted<br/>PaymentFailed<br/>RefundProcessed| PaymentTopic
    CS -->|InventoryUpdated<br/>EventCreated<br/>PriceChanged| CatalogTopic
    
    %% Event Consumption
    BookingTopic --> BookingSub
    PaymentTopic --> PaymentSub
    BookingTopic --> NotificationSub
    PaymentTopic --> NotificationSub
    CatalogTopic --> NotificationSub
    
    BookingSub --> NS
    PaymentSub --> NS
    NotificationSub --> NS
    BookingSub --> ReportingService
    PaymentSub --> AuditService
    
    style BS fill:#e8f5e8
    style PS fill:#fff3e0
    style CS fill:#e3f2fd
    style NS fill:#f3e5f5
    style MB fill:#fce4ec
```

### 2. Event Schema y Versionado

```mermaid
classDiagram
    class BaseEvent {
        +string EventId
        +string EventType
        +DateTime Timestamp
        +string CorrelationId
        +string UserId
        +int Version
        +Dictionary~string,object~ Metadata
    }
    
    class BookingCreatedEvent {
        +string BookingId
        +string EventId
        +int TicketCount
        +decimal TotalAmount
        +DateTime ExpirationTime
        +string CustomerEmail
    }
    
    class PaymentCompletedEvent {
        +string PaymentId
        +string BookingId
        +decimal Amount
        +string Currency
        +string PaymentProvider
        +string TransactionId
        +DateTime ProcessedAt
    }
    
    class InventoryUpdatedEvent {
        +string EventId
        +int AvailableTickets
        +int ReservedTickets
        +int SoldTickets
        +string UpdateReason
    }
    
    BaseEvent <|-- BookingCreatedEvent
    BaseEvent <|-- PaymentCompletedEvent
    BaseEvent <|-- InventoryUpdatedEvent
    
    note for BaseEvent "Esquema base para todos los eventos\nGarantiza consistencia y trazabilidad"
    note for BookingCreatedEvent "v2.0 - Added CustomerEmail\nv1.0 - Initial version"
```

### 3. Saga Pattern para Transacciones Distribuidas

```mermaid
sequenceDiagram
    participant U as Usuario
    participant BS as Booking Service
    participant PS as Payment Service
    participant CS as Catalog Service
    participant NS as Notification Service
    participant MB as Message Broker
    
    Note over U, MB: Saga: Process Booking Transaction
    
    U->>BS: Create booking request
    
    rect rgb(245, 255, 245)
        Note over BS, MB: Step 1: Reserve Inventory
        BS->>CS: Reserve tickets
        CS->>MB: InventoryReserved event
        BS->>MB: BookingCreated event
    end
    
    rect rgb(255, 248, 220)
        Note over PS, MB: Step 2: Process Payment
        MB->>PS: Consume BookingCreated
        PS->>PS: Process payment
        
        alt Payment Success
            PS->>MB: PaymentCompleted event
        else Payment Failed
            PS->>MB: PaymentFailed event
        end
    end
    
    rect rgb(240, 248, 255)
        Note over BS, NS: Step 3: Finalize or Compensate
        alt Payment Success Path
            MB->>BS: Consume PaymentCompleted
            BS->>BS: Confirm booking
            BS->>MB: BookingConfirmed event
            MB->>NS: Send confirmation
            
        else Payment Failed Path
            MB->>BS: Consume PaymentFailed
            BS->>CS: Release reserved tickets
            CS->>MB: InventoryReleased event
            BS->>MB: BookingCancelled event
            MB->>NS: Send cancellation notice
        end
    end
```

## Manejo de Errores y Reintentos

### 1. Dead Letter Queue Pattern

```mermaid
flowchart TD
    Producer[Event Producer] --> Topic[Service Bus Topic]
    Topic --> Subscription[Main Subscription]
    
    Subscription --> Consumer[Event Consumer]
    
    Consumer --> ProcessSuccess{Processing<br/>Successful?}
    ProcessSuccess -->|✅ Yes| Complete[Complete Message]
    ProcessSuccess -->|❌ No| RetryLogic{Retry Logic}
    
    RetryLogic --> MaxRetries{Max Retries<br/>Exceeded?}
    MaxRetries -->|❌ No| DelayRetry[Exponential Backoff]
    DelayRetry --> Consumer
    
    MaxRetries -->|✅ Yes| DLQ[Dead Letter Queue]
    DLQ --> DLQMonitor[DLQ Monitor]
    DLQMonitor --> ManualReview[Manual Review]
    
    ManualReview --> FixIssue[Fix Issue]
    FixIssue --> Reprocess[Reprocess Message]
    Reprocess --> Topic
    
    ManualReview --> DiscardMessage[Discard Message]
    
    style Producer fill:#e8f5e8
    style Consumer fill:#e3f2fd
    style DLQ fill:#ffebee
    style Complete fill:#c8e6c9
```

### 2. Idempotency Pattern

```mermaid
sequenceDiagram
    participant P as Payment Service
    participant I as Idempotency Store
    participant E as External Payment API
    
    Note over P, E: Ensuring Idempotent Payment Processing
    
    P->>I: Check if request already processed
    I->>P: Return cached result or null
    
    alt Request Already Processed
        P->>P: Return cached result
        Note over P: Avoid duplicate processing
    else New Request
        P->>E: Process payment
        E->>P: Payment result
        P->>I: Store result with idempotency key
        P->>P: Return result
    end
    
    Note over P, E: Idempotency key = hash(bookingId + amount + timestamp)
```

## Service Bus Configuration

### Topics y Subscriptions

```yaml
# Azure Service Bus Configuration
servicebus:
  namespace: "ticketwave-messaging"
  topics:
    booking-events:
      subscriptions:
        - name: "payment-processing"
          filters:
            - eventType: "BookingCreated"
        - name: "notification-service"
          filters:
            - eventType: "BookingCreated,BookingConfirmed,BookingCancelled"
        - name: "audit-service"
          filters:
            - "*" # All booking events
    
    payment-events:
      subscriptions:
        - name: "booking-completion"
          filters:
            - eventType: "PaymentCompleted,PaymentFailed"
        - name: "notification-service"
          filters:
            - eventType: "PaymentCompleted"
        - name: "fraud-detection"
          filters:
            - eventType: "PaymentFailed"
    
    catalog-events:
      subscriptions:
        - name: "inventory-sync"
          filters:
            - eventType: "InventoryUpdated"
        - name: "pricing-updates"
          filters:
            - eventType: "PriceChanged"

# Retry Policy Configuration
retryPolicy:
  maxRetries: 3
  backoffType: "exponential"
  initialInterval: "00:00:01"
  maxInterval: "00:01:00"
  timeToLive: "00:10:00"
```

## Communication Monitoring

### Key Metrics

```mermaid
graph LR
    subgraph "Synchronous Metrics"
        HttpLatency[📊 HTTP Latency P95]
        HttpErrors[❌ HTTP Error Rate]
        CircuitState[🔌 Circuit Breaker State]
        Timeout[⏰ Timeout Rate]
    end
    
    subgraph "Asynchronous Metrics"
        EventLatency[📊 Event Processing Latency]
        DLQMessages[💀 Dead Letter Queue Count]
        EventThroughput[🚀 Events/Second]
        ConsumerLag[📈 Consumer Lag]
    end
    
    subgraph "Business Metrics"
        EventCorrelation[🔗 End-to-End Tracing]
        SagaSuccess[✅ Saga Success Rate]
        DataConsistency[⚖️ Data Consistency]
        UserExperience[👤 User Experience Score]
    end
```

### Distributed Tracing

```mermaid
sequenceDiagram
    participant Client
    participant AG as API Gateway
    participant BS as Booking Service
    participant MB as Message Broker
    participant PS as Payment Service
    
    Note over Client, PS: Correlation ID: abc-123-def
    
    Client->>AG: POST /bookings<br/>X-Correlation-ID: abc-123-def
    AG->>BS: Forward with same correlation ID
    
    BS->>BS: Log: Processing booking abc-123-def
    BS->>MB: Publish event with correlation ID
    
    MB->>PS: Consume event abc-123-def
    PS->>PS: Log: Processing payment abc-123-def
    
    Note over Client, PS: All logs tagged with abc-123-def for tracing
```

## Communication Testing

### Contract Testing

```csharp
[Test]
public async Task PaymentService_ShouldConsumeBookingCreatedEvent_WithExpectedSchema()
{
    // Arrange
    var bookingCreatedEvent = new BookingCreatedEvent
    {
        EventId = Guid.NewGuid().ToString(),
        EventType = "BookingCreated",
        BookingId = "booking-123",
        EventId = "event-456",
        TicketCount = 2,
        TotalAmount = 100.00m,
        CustomerEmail = "test@example.com"
    };
    
    // Act
    var result = await _paymentService.HandleBookingCreatedAsync(bookingCreatedEvent);
    
    // Assert
    result.Should().NotBeNull();
    result.PaymentRequired.Should().BeTrue();
    result.Amount.Should().Be(100.00m);
}
```

### Load Testing para Messaging

```javascript
// k6 load test for event processing
import { check } from 'k6';
import { ServiceBusClient } from 'k6/x/servicebus';

export let options = {
  stages: [
    { duration: '30s', target: 100 }, // Ramp up
    { duration: '60s', target: 100 }, // Sustained load
    { duration: '30s', target: 0 },   // Ramp down
  ],
};

export default function() {
  const event = {
    eventType: 'BookingCreated',
    bookingId: `booking-${__VU}-${__ITER}`,
    timestamp: new Date().toISOString()
  };
  
  const response = ServiceBusClient.sendMessage('booking-events', event);
  
  check(response, {
    'message sent successfully': (r) => r.status === 200,
    'delivery time < 100ms': (r) => r.timings.duration < 100,
  });
}
```

This communication architecture ensures both transactional consistency and system scalability, providing robust patterns for handling high concurrency and network failures.