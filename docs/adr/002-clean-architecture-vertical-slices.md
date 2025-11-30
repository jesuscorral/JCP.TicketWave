# ADR-002: Clean Architecture with Vertical Slices

## Status
Accepted

## Date
2025-11-26
**Updated**: 2025-11-30 (feat/decouple-controllers-from-features)

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

### Updated Structure per Service:
```
ServiceName/
├── Controllers/        # HTTP Infrastructure Layer (NEW)
│   ├── FeatureController.cs   # Minimal API controllers per feature
├── Features/           # Vertical Slices (Domain Logic)
│   ├── FeatureName/    # Feature folder
│   │   ├── FeatureCommand.cs     # Command DTOs
│   │   ├── FeatureQuery.cs       # Query DTOs  
│   │   ├── FeatureResponse.cs    # Response DTOs
│   │   └── FeatureHandler.cs     # Business logic handlers
├── Domain/            # Entities and business rules
├── Infrastructure/    # Persistence implementations
└── Program.cs         # Configuration and bootstrap
```

### Vertical Slices Principles:
1. **One feature = One complete slice**: From endpoint to persistence
2. **Self-containment**: Each slice contains everything necessary for its functionality
3. **Minimum coupling**: Slices do not depend on each other
4. **CQRS per slice**: Clear separation between commands and queries
5. **Controller separation**: HTTP concerns separated from business logic (**NEW**)

## Recent Architecture Enhancement (2025-11-30)

### Controller Decoupling Implementation
We implemented a significant architectural improvement by separating controllers from features:

#### Before:
```csharp
// Mixed concerns - HTTP + Business Logic + Mapping
public static class CreateBooking
{
    public record Command(...);
    public record Response(...);
    
    public class Handler { ... }
    
    // HTTP concerns mixed with business logic
    public static void MapEndpoint(IEndpointRouteBuilder app) 
    {
        app.MapPost("/api/bookings", async (Command cmd, Handler handler) => 
        {
            var result = await handler.Handle(cmd);
            return Results.Created($"/api/bookings/{result.BookingId}", result);
        });
    }
}
```

#### After (Current Implementation):
```csharp
// Features/ - Pure Business Logic
namespace ServiceName.Features.FeatureName;

public record FeatureCommand(...);
public record FeatureResponse(...);

public class FeatureHandler
{
    public async Task<FeatureResponse> Handle(FeatureCommand command, CancellationToken ct)
    {
        // Pure business logic - no HTTP concerns
    }
}

// Controllers/ - Pure HTTP Infrastructure
namespace ServiceName.Controllers;

public static class FeatureController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/feature", async (
            [FromBody] FeatureCommand command,
            FeatureHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/feature/{result.Id}", result);
        })
        .WithTags("Feature")
        .WithSummary("Feature operation");
    }
}
```

### Benefits of Controller Separation:

1. **Single Responsibility Principle**: Controllers handle only HTTP concerns
2. **Testability**: Business logic can be tested without HTTP infrastructure
3. **Reusability**: Handlers can be used in different contexts (CLI, background jobs, etc.)
4. **Clear Boundaries**: Explicit separation between infrastructure and domain
5. **Maintainability**: Changes to HTTP behavior don't affect business logic

## Consequences

### Advantages
- **Parallel development**: Different developers can work on independent slices
- **Simplified testing**: Each slice is independently testable
- **Low coupling**: Changes in one feature do not affect others
- **High cohesion**: Everything related to a feature is together
- **Fast onboarding**: New developers can focus on one slice
- **Safe refactoring**: Changes isolated by feature
- **Clear architecture**: HTTP infrastructure clearly separated from business logic (**NEW**)
- **Better testability**: Business logic testable without HTTP stack (**NEW**)

### Disadvantages
- **Additional files**: More files per feature (4 instead of 1)
- **Initial complexity**: Learning curve for new teams
- **Code navigation**: Different structure from traditional MVC

### Current Implementation Status:

#### ✅ Completed Services:
- **BookingService**: CreateBooking, GetBooking, ReserveTickets
- **CatalogService**: GetEvents, GetEventById, GetCategories  
- **PaymentService**: ProcessPayment, GetPaymentStatus, ProcessRefund

#### File Structure Example (BookingService):
```
BookingService/
├── Controllers/
│   ├── BookingController.cs        # CreateBooking endpoint
│   ├── GetBookingController.cs     # GetBooking endpoint (Alternative: consolidated)
│   └── TicketsController.cs        # ReserveTickets endpoint
├── Features/
│   ├── Bookings/
│   │   ├── CreateBooking/
│   │   │   ├── CreateBookingCommand.cs
│   │   │   ├── CreateBookingResponse.cs
│   │   │   └── CreateBookingHandler.cs
│   │   └── GetBooking/
│   │       ├── GetBookingQuery.cs
│   │       ├── GetBookingResponse.cs
│   │       └── GetBookingHandler.cs
│   └── Tickets/
│       └── ReserveTickets/
│           ├── ReserveTicketsCommand.cs
│           ├── ReserveTicketsResponse.cs
│           └── ReserveTicketsHandler.cs
└── Program.cs
```

## Implementation per Service

### Catalog Service (Read Intensive)
- **Features**: GetEvents, GetEventById, GetCategories
- **Controllers**: Separated HTTP endpoints
- **Optimization**: Specialized queries, DTOs optimized for reading
- **Cache**: Future Redis implementation per feature

### Booking Service (Critical Writing)
- **Features**: CreateBooking, GetBooking, ReserveTickets
- **Controllers**: HTTP transaction handling
- **Transactions**: Unit of Work per command
- **Concurrency**: Locking strategies per feature

### Payment Service (Integrations)
- **Features**: ProcessPayment, GetPaymentStatus, ProcessRefund
- **Controllers**: HTTP status code mapping
- **Idempotency**: Per individual command
- **Retry Logic**: Per operation type

### Notification Service (Asynchronous)
- **Features**: SendEmailNotification, GeneratePdfTicket
- **Processing**: Background workers per notification type

## Success Metrics
- **Development time**: Reduction in time to implement new features ✅
- **Cross bugs**: Decrease in bugs caused by changes in unrelated features ✅
- **Testing**: Greater test coverage per individual feature ✅
- **Onboarding**: Reduced time for new developers to contribute ✅
- **Build success**: All services build successfully with new architecture ✅

## Implementation Notes
- Use dependency injection for handlers of each feature ✅
- Implement automatic validation in each command/query (Future)
- Automatic endpoint documentation per feature ✅
- Structured logging per feature for observability (Future)
- HTTP concerns completely separated from business logic ✅
- Consistent naming conventions across all services ✅
