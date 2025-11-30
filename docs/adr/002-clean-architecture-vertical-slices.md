# ADR-002: Clean Architecture with Vertical Slices

## Status
Accepted

## Date
2025-11-26

## Context
With multiple microservices, we need a code organization strategy that:

- **Reduces coupling** between different functionalities
- **Facilitates parallel development** of features
- **Maintains cohesion** within each functionality
- **Simplifies testing** of individual features
- **Improves maintainability** of code in the long term

The considered options were:
- Traditional Clean Architecture (horizontal layers)
- Vertical Slice Architecture
- Hexagonal architecture
- Organization by domain entities

## Decision
We adopt **Clean Architecture combined with Vertical Slices** to organize the code of each microservice:

### Structure per Service:
```
ServiceName/
├── Features/           # Vertical Slices
│   ├── FeatureName/
│   │   ├── GetQuery.cs     # Query with DTOs and Handler
│   │   ├── CreateCommand.cs # Command with DTOs and Handler
│   │   └── UpdateCommand.cs # Command with DTOs and Handler
├── Domain/            # Entities and business rules
├── Infrastructure/    # Persistence implementations
└── Program.cs         # Configuration and bootstrap
```

### Vertical Slices Principles:
1. **One feature = One complete slice**: From endpoint to persistence
2. **Self-containment**: Each slice contains everything necessary for its functionality
3. **Minimum coupling**: Slices do not depend on each other
4. **CQRS per slice**: Clear separation between commands and queries

## Consequences

### Advantages
- **Parallel development**: Different developers can work on independent slices
- **Simplified testing**: Each slice is independently testable
- **Low coupling**: Changes in one feature do not affect others
- **High cohesion**: Everything related to a feature is together
- **Fast onboarding**: New developers can focus on one slice
- **Safe refactoring**: Changes isolated by feature

### Disadvantages
- **Potential duplication**: Similar code between slices
- **Initial complexity**: Learning curve for new teams
- **Code navigation**: Different structure from traditional MVC

### Patterns Implemented per Slice:

#### 1. CQRS (Command Query Responsibility Segregation)
```csharp
// Ejemplo de Query
public static class GetEvents
{
    public record Query(int Page, int PageSize, string? Category);
    public record Response(IEnumerable<EventDto> Events, int Total);
    
    public class Handler
    {
        public async Task<Response> Handle(Query query, CancellationToken ct)
        {
            // Implementation optimized for reading
        }
    }
}

// Command example
public static class CreateBooking
{
    public record Command(Guid EventId, string UserId, int TicketCount);
    public record Response(Guid BookingId, string Reference);
    
    public class Handler
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            // Implementation optimized for writing
        }
    }
}
```

#### 2. Endpoint Mapping por Feature
```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapGet("/api/events", async (Query query, Handler handler) =>
    {
        var result = await handler.Handle(query);
        return Results.Ok(result);
    });
}
```

## Alternatives Considered

### 1. Traditional Clean Architecture (Horizontal Layers)
**Structure**: Controllers → Application → Domain → Infrastructure
**Advantages**: Familiar, clear separation of responsibilities
**Disadvantages**: Horizontal coupling, changes traverse multiple layers

### 2. Pure Hexagonal Architecture
**Structure**: Ports and adapters with central domain
**Advantages**: Excellent abstraction, testability
**Disadvantages**: Unnecessary complexity for our use case

### 3. Organization by Domain Entities
**Structure**: By aggregates (Event, Booking, Payment)
**Advantages**: Aligned with DDD
**Disadvantages**: Coupling by entity, not by functionality

## Implementation per Service

### Catalog Service (Read Intensive)
- **Features**: GetEvents, GetEventById, GetCategories
- **Optimization**: Specialized queries, DTOs optimized for reading
- **Cache**: Future Redis implementation per feature

### Booking Service (Critical Writing)
- **Features**: CreateBooking, GetBooking, ReserveTickets
- **Transactions**: Unit of Work per command
- **Concurrency**: Locking strategies per feature

### Payment Service (Integrations)
- **Features**: ProcessPayment, GetPaymentStatus, ProcessRefund
- **Idempotency**: Per individual command
- **Retry Logic**: Per operation type

### Notification Service (Asynchronous)
- **Features**: SendEmailNotification, GeneratePdfTicket
- **Processing**: Background workers per notification type

## Success Metrics
- **Development time**: Reduction in time to implement new features
- **Cross bugs**: Decrease in bugs caused by changes in unrelated features
- **Testing**: Greater test coverage per individual feature
- **Onboarding**: Reduced time for new developers to contribute

## Implementation Notes
- Use dependency injection for handlers of each feature
- Implement automatic validation in each command/query
- Automatic endpoint documentation per feature
- Structured logging per feature for observability