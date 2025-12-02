# ADR-003: Persistence Technologies per Service

## Status
Accepted

## Date
2025-11-26

## Context
Each microservice has different data access patterns and unique consistency requirements:

- **Catalog Service**: Read intensive, complex queries, latency tolerant
- **Booking Service**: Critical writing, ACID transactions, strong consistency
- **Payment Service**: Complete audit, idempotency, external integrations
- **Notification Service**: Asynchronous processing, temporal events

The "database per service" strategy requires selecting the most appropriate technology for each usage pattern.

## Decision
**Updated Decision (December 2024)**: We have simplified to a unified SQL Server database with schema separation for operational efficiency.

### Unified SQL Server Database
- **Database**: SQL Server with schema-based separation
- **ORM**: Entity Framework Core across all services
- **Schemas**:
  - `catalog` - Event catalog management (CatalogService)
  - `booking` - Booking and ticket transactions (BookingService) 
  - `payment` - Payment processing and audit (PaymentService)

**Justification for Unified Approach**:
- **Simplified Operations**: Single database instance to manage
- **Consistent Technology**: EF Core across all services
- **Logical Separation**: Schema boundaries maintain service isolation
- **ACID Transactions**: Strong consistency across all domains
- **Operational Efficiency**: Reduced infrastructure complexity
- **Development Velocity**: Simplified local development setup

### Notification Service: Message Queue + Light SQL
- **Message Queue**: Azure Service Bus / RabbitMQ
- **Database**: SQLite / SQL Server Express
- **Justification**:
  - Asynchronous processing
  - Temporal notification status
  - Retry and dead letter queues

## Consequences

### Advantages
- **Optimization per use case**: Each service uses the most efficient technology
- **Independent scalability**: Each DB can scale according to its patterns
- **Failure isolation**: DB problems do not affect other services
- **Specialized expertise**: Teams can specialize in specific technologies
- **Optimized performance**: Queries and operations optimized by workload type

### Disadvantages
- **Operational complexity**: Multiple DBs to maintain and monitor
- **Required expertise**: Knowledge of multiple technologies
- **Backup and recovery**: Different strategies per technology
- **Cross-service joins**: Not possible, requires application aggregation
- **Eventual consistency**: Between services, not global ACID

## Strategies per Service

### Catalog Service - MongoDB Strategy
```csharp
// Ejemplo de esquema flexible
{
  "_id": "ObjectId",
  "title": "Concierto Rock Nacional",
  "category": "music",
  "venue": {
    "name": "Estadio Nacional",
    "capacity": 50000,
    "location": { "lat": -33.4569, "lng": -70.6483 }
  },
  "pricing": {
    "general": { "price": 25000, "available": 30000 },
    "vip": { "price": 85000, "available": 1000 }
  },
  "metadata": {
    "tags": ["rock", "nacional", "weekend"],
    "popularity_score": 8.5
  }
}
```

**Índices optimizados**:
- Compound index: (category, startDate, location)
- Text index: (title, description, tags)
- Geospatial index: venue.location

**Cache Strategy**:
- Popular events: TTL 1 hour
- Event details: TTL 30 minutes
- Categories: TTL 24 hours

### Booking Service - SQL Strategy
```sql
-- Structure optimized for transactions
CREATE TABLE Bookings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Reference NVARCHAR(20) NOT NULL UNIQUE,
    EventId UNIQUEIDENTIFIER NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Status INT NOT NULL, -- Pending, Confirmed, Cancelled, Expired
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL,
    ConfirmedAt DATETIME2 NULL,
    Version ROWVERSION -- For optimistic locking
);

CREATE TABLE TicketReservations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    SeatNumber NVARCHAR(50) NULL,
    Category NVARCHAR(100) NOT NULL,
    Status INT NOT NULL, -- Reserved, Confirmed, Released
    ReservedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    BookingId UNIQUEIDENTIFIER NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
);
```

**Concurrency Strategy**:
- Pessimistic locking for ticket reservations
- Optimistic locking for booking updates
- Automatic deadlock retry policy

### Payment Service - Event Sourcing Strategy
```csharp
// Event Store for complete audit
public abstract class PaymentEvent
{
    public Guid PaymentId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
}

public class PaymentInitiated : PaymentEvent
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PaymentMethodId { get; set; }
    public string IdempotencyKey { get; set; }
}

public class PaymentProcessed : PaymentEvent
{
    public string ExternalTransactionId { get; set; }
    public string ProcessorResponse { get; set; }
}
```

**Idempotency Strategy**:
- Command hash as unique key
- TTL of 24 hours for idempotency keys
- Response caching for duplicate requests

## Migration and Evolution

### Phase 1: Initial Implementation (MVP)
- Catalog Service: MongoDB with basic queries
- Booking Service: SQL Server with Entity Framework
- Payment Service: SQL Server with audit tables
- Notification Service: In memory + SQL Server

### Phase 2: Optimization (Post-MVP)
- Catalog Service: + Redis cache, optimized indexes
- Booking Service: + Optimistic locking, connection pooling
- Payment Service: + Complete Event Store
- Notification Service: + Real message queue

### Phase 3: Scalability (Production)
- Catalog Service: + Read replicas, sharding by region
- Booking Service: + Read replicas for reporting
- Payment Service: + Complete CQRS with read models
- Notification Service: + Event streaming

## Monitoring and Observability

### Metrics per Service:
- **Catalog Service**: Query performance, cache hit rate, index usage
- **Booking Service**: Transaction duration, deadlock frequency, connection pool
- **Payment Service**: Event processing latency, idempotency hit rate
- **Notification Service**: Message processing rate, dead letter count

### Critical Alerts:
- Booking Service: High frequency of deadlocks
- Payment Service: Unprocessed payment events > 5 min
- Catalog Service: Cache miss rate > 30%
- Notification Service: Dead letter queue > 100 messages

## Backup and Recovery

### Strategy per Service:
- **Catalog Service**: Daily snapshot + incremental every 6h
- **Booking Service**: Transaction log backup every 15min
- **Payment Service**: PITR (Point in Time Recovery) enabled
- **Notification Service**: Simple daily backup (temporal data)

## Alternatives Considered

### 1. Single Database per Service Type
**Advantages**: Operational simplicity, one technology to learn
**Disadvantages**: Sub-optimal for specific patterns

### 2. Shared Database
**Advantages**: Direct joins, global transactions
**Disadvantages**: Data coupling, limited scalability

### 3. Single Event Store
**Advantages**: Complete audit, event replay
**Disadvantages**: Unnecessary complexity for simple reads

## Implementation Notes
- Implement specific health checks per DB technology
- Centralized but secure connection string management
- Versioned migration scripts per service
- Periodic disaster recovery testing per service