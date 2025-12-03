# ADR-012: Message Broker and Event Streaming

## Status
Accepted

## Date
2025-12-03

## Context
The implementation of integration events requires a reliable message broker for asynchronous communication between services. Following ADR-004 (Service Communication Strategy), we need:

- **Event-driven architecture**: Support for domain events and integration events
- **Decoupling**: Services communicate without direct dependencies
- **Reliability**: Message durability and delivery guarantees
- **Scalability**: Handle high event volumes during demand spikes
- **Development efficiency**: Easy setup for local development
- **Production readiness**: Monitoring, clustering, and high availability

Technology options considered:
- **RabbitMQ**: Full-featured, reliable, good .NET support
- **Azure Service Bus**: Cloud-native, managed service
- **Apache Kafka**: High-throughput, event streaming
- **Redis Pub/Sub**: Simple, fast, but less durable

## Decision
We adopt **RabbitMQ** as our primary message broker with the following architecture:

### 1. Local Development Setup
`yaml
# docker-compose.yml
services:
  rabbitmq:
    image: rabbitmq:3.13-management
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
    ports:
      - "5672:5672"     # AMQP
      - "15672:15672"   # Management UI
`

### 2. Event Categories
- **Domain Events**: Local to each service (in-process)
- **Integration Events**: Cross-service communication (RabbitMQ)

### 3. Event Flow Pattern
`
Domain Event → Domain Event Handler → Integration Event → Message Broker → Integration Event Handler
`

### 4. Message Patterns
- **Publish/Subscribe**: One event, multiple handlers
- **Topic Routing**: Route events by type and service
- **Dead Letter Queue**: Handle failed message processing
- **Durability**: Persistent messages for critical events

## Implementation Details

### Event Types Defined
`csharp
// Integration events for cross-service communication
public abstract record IntegrationEvent : DomainEvent, IIntegrationEvent
{
    public abstract string EventType { get; }
    public abstract string Source { get; }
}
`

### Service Integration
- **BookingService**: Publishes inventory, notification, and payment events
- **CatalogService**: Handles inventory updates
- **PaymentService**: Handles payment preparation
- **NotificationService**: Handles notification sending

### Configuration Pattern
`csharp
// Shared configuration in each service
builder.Services.AddRabbitMQ(builder.Configuration);
`

## Consequences

### Positive
- **Loose Coupling**: Services communicate via events, not direct API calls
- **Scalability**: Each service can scale independently
- **Resilience**: Failed events can be retried automatically
- **Auditability**: Event log provides business operation history
- **Development Speed**: Easy local setup with Docker

### Negative
- **Complexity**: Additional infrastructure component to manage
- **Eventual Consistency**: Async operations may have delays
- **Debugging**: Distributed event flows can be harder to trace
- **Operational Overhead**: Monitoring and maintenance of message broker

### Neutral
- **Learning Curve**: Team needs to understand event-driven patterns
- **Testing**: Requires integration testing with message broker

## Compliance with Other ADRs

### ADR-001 (Microservices Architecture)
✅ **Compliant**: Enables independent service scaling and deployment

### ADR-004 (Service Communication)
✅ **Compliant**: Implements the asynchronous communication strategy defined
- Domain events trigger integration events
- Event-driven choreography for complex flows
- Foundation for Saga pattern implementation

### ADR-007 (Resilience Patterns)
✅ **Compliant**: Adds resilience through:
- Message durability and persistence
- Dead letter queues for failed processing
- Retry mechanisms for transient failures
- Circuit breaker pattern can be applied to event publishing

### ADR-002 (Clean Architecture)
✅ **Compliant**: 
- Domain events remain in domain layer
- Integration events in infrastructure layer
- Handlers follow dependency injection patterns

## Migration Strategy

### Phase 1: Foundation (Current)
- ✅ RabbitMQ infrastructure setup
- ✅ Basic integration events implementation
- ✅ Cross-service event handlers

### Phase 2: Production Readiness
- [ ] Add event versioning and backward compatibility
- [ ] Implement dead letter queue handling
- [ ] Add comprehensive logging and monitoring
- [ ] Configure clustering for high availability

### Phase 3: Advanced Patterns
- [ ] Implement Saga pattern for complex transactions
- [ ] Add event sourcing for critical aggregates
- [ ] Implement CQRS with event replay capabilities

## Monitoring and Operations

### Metrics to Track
- Message throughput and latency
- Queue depths and processing rates
- Failed message counts
- Service response times to events

### Alerts
- Queue depth exceeding thresholds
- High error rates in event processing
- RabbitMQ service availability

### Development Tools
- RabbitMQ Management UI (http://localhost:15672)
- Application logs with correlation IDs
- Integration tests with event verification

## Example Usage

### Publishing Integration Events
`csharp
// In domain event handler
await _integrationEventBus.PublishAsync(new UpdateEventInventoryIntegrationEvent(
    EventId: domainEvent.EventId,
    BookingId: domainEvent.BookingId,
    QuantityReduced: domainEvent.Quantity,
    RequestedAt: DateTime.UtcNow
));
`

### Handling Integration Events
`csharp
// In service event handler
public class UpdateEventInventoryIntegrationEventHandler : IMessageHandler<UpdateEventInventoryIntegrationEvent>
{
    public async Task HandleAsync(UpdateEventInventoryIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Process inventory update
        // Publish result event
    }
}
`

This decision establishes a robust foundation for event-driven architecture while maintaining alignment with existing architectural decisions and providing a clear path for future enhancements.
