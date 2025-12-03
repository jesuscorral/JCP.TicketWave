# ADR-009: Repository Pattern Implementation

## Status
Accepted

## Date
2025-12-02

## Context
To complete the architecture defined in previous ADRs, we need to implement a consistent Repository Pattern across all microservices. This will provide:

- **Data Access Abstraction**: Clean separation between domain logic and persistence concerns
- **Testability**: Easy mocking of data access for unit tests
- **Technology Independence**: Ability to change persistence technologies without affecting business logic
- **Consistency**: Uniform data access patterns across all services

Each service has different persistence needs as defined in ADR-003:
- **Catalog Service**: SQL Server for read-heavy operations
- **Booking Service**: SQL Server for ACID transactions
- **Payment Service**: SQL Server for financial transactions with audit trail

## Decision

We implement a **Domain-Driven Repository Pattern** with the following characteristics:

### 1. Repository Interfaces in Domain Layer
`csharp
// Domain layer defines contracts
public interface IEventRepository
{
    Task<Event> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event> CreateAsync(Event entity);
    // ... other operations
}
`

### 2. Infrastructure Layer Implementation
`csharp
// Infrastructure implements domain contracts
public class EventRepository : IEventRepository
{
    private readonly CatalogDbContext _catalogDbContext;
    
    public async Task<Event> GetByIdAsync(Guid id)
    {
        // Technology-specific implementation
    }
}
`

## Implementation Details

### Domain Layer Structure
`
Domain/
├── Entities/           # Domain entities
├── Enums/             # Domain enumerations  
└── Interfaces/        # Repository contracts
`

### Infrastructure Layer Structure
`
Infrastructure/
├── Data/
│   ├── Configurations/    # EF Core configurations
│   ├── Models/           # Legacy document models (deprecated)
│   ├── Repositories/     # Repository implementations
│   └── DbContext.cs      # Database context
`

## Technology Mappings

### Catalog Service (SQL Server)
- **Entity Models**: Category, Event, Venue entities with EF Core
- **Schema Strategy**: catalog schema for logical separation
- **Mapping**: EF Core conventions and configurations

### Booking Service (SQL Server)
- **Entities**: Booking, Ticket with EF Core configurations
- **Migrations**: Database schema versioning
- **Concurrency**: Optimistic concurrency with row versioning

### Payment Service (SQL Server)
- **Entities**: Payment, Refund, PaymentMethod, PaymentEvent
- **Audit**: Complete event store for financial compliance
- **Constraints**: Business rules enforced at database level

## Benefits

### Architecture Benefits
- **Clean Architecture Compliance**: Repository interfaces in domain, implementations in infrastructure
- **Testability**: Easy to mock repositories for unit testing
- **Technology Independence**: Can switch databases without changing business logic
- **Consistency**: Uniform data access patterns across services

## Consequences

### Positive
- **Clean Architecture**: Proper separation of concerns achieved
- **Testability**: Business logic can be tested independently
- **Maintainability**: Consistent patterns across all services
- **Technology Flexibility**: Easy to change persistence technologies

### Challenges
- **Initial Complexity**: More layers and abstractions
- **Learning Curve**: Developers need to understand the pattern
- **Performance**: Additional abstraction layer (mitigated by proper implementation)

## Validation

### ADR Compliance
- ✅ **ADR-001 (Microservices)**: Each service has independent data access
- ✅ **ADR-002 (Clean Architecture)**: Domain/Infrastructure separation maintained  
- ✅ **ADR-003 (Persistence)**: Technology choices per service respected
- ✅ **ADR-006 (State Management)**: Transaction boundaries properly handled

### Implementation Status
- ✅ **Catalog Service**: SQL Server repository with EF Core mapping (catalog schema)
- ✅ **Booking Service**: SQL Server repository with EF Core (booking schema)  
- ✅ **Payment Service**: SQL Server repository with audit capabilities (payment schema)
- ✅ **Central Package Management**: Unified EF Core versions across all repositories
- ✅ **Database Consolidation**: Single SQL Server instance with schema separation
- ✅ **All Services**: Domain interfaces defined, infrastructure implemented
- ✅ **Dependency Injection**: Proper service registration configured
- ✅ **Database Migrations**: Schema creation and versioning implemented

## Related ADRs
- ADR-002: Clean Architecture with Vertical Slices (architectural foundation)
- ADR-003: Persistence Technologies per Service (technology choices)
- ADR-006: State Management and Transactions Strategy (transaction handling)
- ADR-010: Central Package Management Implementation (unified dependency management)
