# Integration Events Flow Documentation

## Overview
This document describes the complete integration events flow implemented in JCP.TicketWave, demonstrating how domain events trigger cross-service communication via RabbitMQ.

## Architecture Pattern

### Domain Events → Integration Events Pattern
`
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   BookingService│    │    RabbitMQ      │    │  CatalogService │
│                 │    │                  │    │                 │
│  Domain Event   │───▶│ Integration Event│───▶│  Event Handler  │
│     Handler     │    │     Queue        │    │                 │
│                 │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
`

## Event Flow Sequence

### 1. Booking Creation Flow
`mermaid
sequenceDiagram
    participant Client
    participant BookingService
    participant RabbitMQ
    participant CatalogService
    participant PaymentService
    participant NotificationService

    Client->>BookingService: POST /api/bookings
    BookingService->>BookingService: Create Booking (Domain)
    BookingService->>BookingService: Raise BookingCreatedDomainEvent
    BookingService->>RabbitMQ: Publish UpdateEventInventoryIntegrationEvent
    BookingService->>RabbitMQ: Publish PreparePaymentDataIntegrationEvent
    BookingService->>RabbitMQ: Publish SendBookingNotificationIntegrationEvent
    BookingService->>Client: Booking Created Response

    RabbitMQ->>CatalogService: UpdateEventInventoryIntegrationEvent
    CatalogService->>CatalogService: Update Inventory
    CatalogService->>RabbitMQ: Publish InventoryUpdatedIntegrationEvent

    RabbitMQ->>PaymentService: PreparePaymentDataIntegrationEvent
    PaymentService->>PaymentService: Prepare Payment
    PaymentService->>RabbitMQ: Publish PaymentDataPreparedIntegrationEvent

    RabbitMQ->>NotificationService: SendBookingNotificationIntegrationEvent
    NotificationService->>NotificationService: Send Email
    NotificationService->>RabbitMQ: Publish NotificationSentIntegrationEvent
`

## Integration Events Catalog

### 1. Booking Service Events (Published)
`csharp
// Requests inventory update
UpdateEventInventoryIntegrationEvent
{
    EventId: Guid,
    BookingId: Guid,
    QuantityReduced: int,
    RequestedAt: DateTime
}

// Requests notification sending
SendBookingNotificationIntegrationEvent
{
    BookingId: Guid,
    UserId: Guid,
    EventId: Guid,
    Quantity: int,
    TotalAmount: decimal,
    NotificationType: string,
    RequestedAt: DateTime
}

// Requests payment preparation
PreparePaymentDataIntegrationEvent
{
    BookingId: Guid,
    Amount: decimal,
    Currency: string,
    UserId: Guid,
    ExpiresAt: DateTime,
    RequestedAt: DateTime
}
`

### 2. Catalog Service Events (Published)
`csharp
// Confirms inventory was updated
InventoryUpdatedIntegrationEvent
{
    EventId: Guid,
    BookingId: Guid,
    PreviousAvailableTickets: int,
    CurrentAvailableTickets: int,
    UpdateType: string,
    UpdatedAt: DateTime
}
`

### 3. Payment Service Events (Published)
`csharp
// Confirms payment data was prepared
PaymentDataPreparedIntegrationEvent
{
    BookingId: Guid,
    PaymentId: Guid,
    Amount: decimal,
    Currency: string,
    ExpiresAt: DateTime,
    PaymentUrl: string,
    PreparedAt: DateTime
}
`

### 4. Notification Service Events (Published)
`csharp
// Confirms notification was sent
NotificationSentIntegrationEvent
{
    BookingId: Guid,
    UserId: Guid,
    NotificationType: string,
    Channel: string,
    Success: bool,
    ErrorMessage: string?,
    SentAt: DateTime
}
`

## Implementation Details

### Event Handlers Registration
`csharp
// In each service Program.cs
builder.Services.AddRabbitMQ(builder.Configuration);
builder.Services.AddScoped<UpdateEventInventoryIntegrationEventHandler>();
`

### Event Publishing Pattern
`csharp
// In Domain Event Handler
public class BookingCreatedDomainEventHandler : IDomainEventHandler<BookingCreatedDomainEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;

    public async Task HandleAsync(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Create integration event
        var integrationEvent = new UpdateEventInventoryIntegrationEvent(
            EventId: domainEvent.EventId,
            BookingId: domainEvent.BookingId,
            QuantityReduced: domainEvent.Quantity,
            RequestedAt: DateTime.UtcNow
        );

        // Publish to RabbitMQ
        await _integrationEventBus.PublishAsync(integrationEvent);
    }
}
`

### Event Handling Pattern
`csharp
// In Integration Event Handler
public class UpdateEventInventoryIntegrationEventHandler : IMessageHandler<UpdateEventInventoryIntegrationEvent>
{
    public async Task HandleAsync(UpdateEventInventoryIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Process the event
        var eventEntity = await _eventRepository.GetByIdAsync(integrationEvent.EventId);
        eventEntity.UpdateAvailableTickets(newQuantity);
        await _eventRepository.UpdateAsync(eventEntity);

        // Publish response event
        var responseEvent = new InventoryUpdatedIntegrationEvent(...);
        await _integrationEventBus.PublishAsync(responseEvent);
    }
}
`

## Testing the Flow

### Test Endpoint
`http
POST http://localhost:5001/api/example/create-booking-with-flow
Content-Type: application/json

{
    "eventId": "123e4567-e89b-12d3-a456-426614174000",
    "userId": "test-user-123",
    "ticketCount": 2
}
`

### Expected Log Output
`
🚀 Starting integration events example flow - EventId: {EventId}, UserId: {UserId}
📅 Processing BookingCreatedDomainEvent for BookingId: {BookingId}
🎫 Publishing UpdateEventInventoryIntegrationEvent - EventId: {EventId}, Quantity: {Quantity}
📧 Publishing SendBookingNotificationIntegrationEvent - BookingId: {BookingId}, UserId: {UserId}
💰 Publishing PreparePaymentDataIntegrationEvent - BookingId: {BookingId}, Amount: {Amount}
✅ Successfully processed BookingCreatedDomainEvent for BookingId: {BookingId}
🎫 Processing UpdateEventInventoryIntegrationEvent - EventId: {EventId}, BookingId: {BookingId}
✅ Inventory updated successfully - EventId: {EventId}, Previous: {Previous}, New: {New}
📊 Published InventoryUpdatedIntegrationEvent - EventId: {EventId}
`

## Saga Pattern Foundation

This implementation provides the foundation for implementing Saga patterns:

### 1. Choreography-based Saga
- ✅ Event-driven communication established
- ✅ Events contain correlation information (BookingId)
- ✅ Success and failure events defined
- 🚧 Compensation events need to be added

### 2. Orchestration-based Saga
- ✅ Event infrastructure ready
- 🚧 Saga coordinator service needs to be implemented
- 🚧 State management for saga instances needed

## Monitoring and Observability

### RabbitMQ Management
- **URL**: http://localhost:15672
- **Credentials**: admin/admin123
- **Queues**: Monitor queue depths and message rates
- **Exchanges**: Verify routing and binding configurations

### Application Logs
- **Correlation IDs**: Track events across services
- **Event Types**: Filter by integration event types
- **Error Handling**: Monitor failed event processing

## Future Enhancements

### Phase 2: Production Readiness
- [ ] Dead letter queue handling
- [ ] Event versioning and backward compatibility
- [ ] Comprehensive monitoring and alerting
- [ ] Performance optimization

### Phase 3: Advanced Patterns
- [ ] Full Saga pattern implementation
- [ ] Event sourcing for critical aggregates
- [ ] CQRS with event replay
- [ ] Real-time event streaming dashboards

This implementation establishes a robust, scalable foundation for event-driven microservices architecture with clear patterns for future expansion.
