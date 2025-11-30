# Ticket Booking Process Flow

## Main Booking Flow

```mermaid
sequenceDiagram
    participant U as User
    participant AG as API Gateway
    participant CS as Catalog Service
    participant BS as Booking Service
    participant PS as Payment Service
    participant NS as Notification Service
    participant MB as Message Broker
    participant DB as Booking Database
    
    Note over U, DB: Complete Ticket Booking Process
    
    %% 1. Event Discovery
    rect rgb(240, 248, 255)
        Note over U, CS: Phase 1: Event Discovery
        U->>AG: GET /api/events?category=music
        AG->>CS: Forward request with auth
        CS->>CS: Query available events
        CS->>AG: Return events with real-time availability
        AG->>U: Event list + inventory status
    end
    
    %% 2. Event Details
    rect rgb(245, 255, 245)
        Note over U, CS: Phase 2: Event Details
        U->>AG: GET /api/events/{eventId}
        AG->>CS: Get event details
        CS->>CS: Get detailed info + pricing tiers
        CS->>AG: Event details + available seats
        AG->>U: Complete event information
    end
    
    %% 3. Booking Creation
    rect rgb(255, 248, 220)
        Note over U, MB: Phase 3: Booking Creation
        U->>AG: POST /api/bookings<br/>{eventId, ticketCount, seatPreferences}
        AG->>BS: Create booking request
        
        BS->>DB: BEGIN TRANSACTION
        BS->>DB: Check ticket availability (FOR UPDATE)
        
        alt Tickets Available
            BS->>DB: Reserve tickets (pessimistic lock)
            BS->>DB: Create booking record
            BS->>DB: Set expiration (15 minutes)
            BS->>DB: COMMIT TRANSACTION
            
            BS->>MB: Publish BookingCreated event
            BS->>AG: Return booking confirmation
            AG->>U: Booking ID + payment deadline
            
        else Insufficient Tickets
            BS->>DB: ROLLBACK TRANSACTION
            BS->>AG: Return insufficient tickets error
            AG->>U: Error: Not enough tickets
        end
    end
    
    %% 4. Payment Processing
    rect rgb(255, 245, 238)
        Note over U, PS: Phase 4: Payment Processing
        U->>AG: POST /api/payments<br/>{bookingId, paymentMethod, cardDetails}
        AG->>PS: Process payment
        
        PS->>PS: Validate payment data
        PS->>PS: Tokenize card info (PCI compliance)
        
        par Payment Processing
            PS->>External: Call Stripe/PayPal API
            External->>PS: Payment confirmation
        and Booking Validation
            PS->>BS: Validate booking status
            BS->>PS: Confirm booking active
        end
        
        alt Payment Successful
            PS->>MB: Publish PaymentCompleted event
            PS->>AG: Payment success + transaction ID
            AG->>U: Payment confirmed
            
        else Payment Failed
            PS->>MB: Publish PaymentFailed event
            PS->>AG: Payment error details
            AG->>U: Error: Payment failed - retry
        end
    end
    
    %% 5. Booking Confirmation
    rect rgb(248, 245, 255)
        Note over MB, NS: Phase 5: Confirmation and Notifications
        
        MB->>BS: Consume PaymentCompleted event
        BS->>DB: Update booking status to CONFIRMED
        BS->>DB: Update ticket status to SOLD
        BS->>MB: Publish BookingConfirmed event
        
        MB->>NS: Consume BookingConfirmed event
        
        par Notification Processing
            NS->>NS: Generate ticket PDF
            NS->>NS: Create confirmation email
            NS->>External: Send email via SendGrid
        and Optional SMS
            NS->>External: Send SMS confirmation (if enabled)
        end
        
        NS->>U: Email with tickets received
    end
```

## Booking States

```mermaid
stateDiagram-v2
    [*] --> PENDING: Create booking
    
    PENDING --> RESERVED: Tickets reserved
    PENDING --> FAILED: Insufficient tickets
    
    RESERVED --> PAYMENT_PENDING: Awaiting payment
    RESERVED --> EXPIRED: 15 min timeout
    
    PAYMENT_PENDING --> CONFIRMED: Payment successful
    PAYMENT_PENDING --> PAYMENT_FAILED: Payment error
    PAYMENT_PENDING --> EXPIRED: Payment timeout
    
    PAYMENT_FAILED --> PAYMENT_PENDING: Retry payment
    PAYMENT_FAILED --> CANCELLED: Max retries exceeded
    
    CONFIRMED --> TICKETS_DELIVERED: Notifications sent
    CONFIRMED --> REFUND_REQUESTED: User cancellation
    
    REFUND_REQUESTED --> REFUNDED: Refund processed
    REFUND_REQUESTED --> REFUND_DENIED: Policy violation
    
    EXPIRED --> [*]: Cleanup expired booking
    CANCELLED --> [*]: Cleanup cancelled booking
    FAILED --> [*]: Cleanup failed booking
    TICKETS_DELIVERED --> [*]: Process complete
    REFUNDED --> [*]: Process complete
    REFUND_DENIED --> CONFIRMED: Booking remains active
    
    note right of RESERVED
        Pessimistic locking
        15-minute reservation
    end note
    
    note right of CONFIRMED
        Payment processed
        Tickets allocated
    end note
    
    note right of EXPIRED
        Auto-cleanup job
        Release reserved tickets
    end note
```

## Booking Flow with Resilience

```mermaid
flowchart TD
    Start([User requests reservation]) --> ValidateInput{Validate input data}
    
    ValidateInput -->|❌ Invalid| ReturnError[Error: Invalid data]
    ValidateInput -->|✅ Valid| CheckAuth{User authenticated?}
    
    CheckAuth -->|❌ No| ReturnUnauth[Error: Not authorized]
    CheckAuth -->|✅ Yes| StartBooking[Start booking process]
    
    StartBooking --> GetInventory[Check available inventory]
    
    GetInventory --> CheckAvailability{Tickets available?}
    CheckAvailability -->|❌ No| ReturnUnavailable[Error: Tickets sold out]
    CheckAvailability -->|✅ Yes| LockInventory[Lock inventory]
    
    LockInventory --> LockSuccess{Lock successful?}
    LockSuccess -->|❌ Race condition| RetryLock{Retry?}
    RetryLock -->|❌ Max retries| ReturnConflict[Error: Reservation conflict]
    RetryLock -->|✅ Yes| LockInventory
    
    LockSuccess -->|✅ Yes| CreateReservation[Create temporary reservation]
    
    CreateReservation --> SetExpiration[Set 15min expiration]
    SetExpiration --> PublishEvent[Publish BookingCreated event]
    PublishEvent --> ReturnSuccess[Return reservation ID]
    
    %% Background processes
    PublishEvent -.-> StartPaymentTimer[Start payment timer]
    StartPaymentTimer -.-> PaymentTimeout{Timer expired?}
    PaymentTimeout -.->|✅ Yes| ExpireBooking[Expire booking]
    PaymentTimeout -.->|❌ No| WaitPayment[Wait for payment]
    
    ExpireBooking -.-> ReleaseInventory[Release inventory]
    ReleaseInventory -.-> CleanupReservation[Cleanup reservation]
    
    %% Error handling
    ReturnError --> End([End])
    ReturnUnauth --> End
    ReturnUnavailable --> End
    ReturnConflict --> End
    ReturnSuccess --> End
    CleanupReservation -.-> End
    
    %% Styling
    style StartBooking fill:#e8f5e8
    style LockInventory fill:#fff3e0
    style CreateReservation fill:#e3f2fd
    style PublishEvent fill:#f3e5f5
    style ExpireBooking fill:#ffebee
```

## Concurrency Handling

### Scenario: High Demand - Last Ticket

```mermaid
sequenceDiagram
    participant U1 as User A
    participant U2 as User B
    participant BS as Booking Service
    participant DB as Database
    participant Lock as Pessimistic Lock
    
    Note over U1, Lock: Scenario: Only 1 ticket remaining
    
    par Simultaneous Requests
        U1->>BS: POST /bookings (1 ticket)
        U2->>BS: POST /bookings (1 ticket)
    end
    
    BS->>DB: BEGIN TRANSACTION (User A)
    BS->>DB: BEGIN TRANSACTION (User B)
    
    BS->>Lock: SELECT ... FOR UPDATE (User A)
    Lock-->>BS: Lock acquired ✅
    
    BS->>Lock: SELECT ... FOR UPDATE (User B)
    Note over Lock: User B waits for lock...
    
    BS->>DB: Check availability: 1 ticket ✅
    BS->>DB: Reserve ticket for User A
    BS->>DB: Update inventory: 0 tickets
    BS->>DB: COMMIT TRANSACTION (User A)
    
    Lock-->>BS: Lock released
    Lock-->>BS: Lock acquired for User B ✅
    
    BS->>DB: Check availability: 0 tickets ❌
    BS->>DB: ROLLBACK TRANSACTION (User B)
    
    BS-->>U1: Success: Booking confirmed
    BS-->>U2: Error: No tickets available
    
    Note over U1, Lock: User A gets the ticket, User B receives clear error
```

## Recovery Strategies

### Auto-Cleanup of Expired Reservations

```mermaid
flowchart TD
    ScheduledJob[Scheduled job every 5min] --> QueryExpired[Search expired reservations]
    
    QueryExpired --> HasExpired{Expired reservations?}
    HasExpired -->|❌ No| Sleep[Wait for next cycle]
    HasExpired -->|✅ Yes| ProcessExpired[Process expired reservations]
    
    ProcessExpired --> StartTx[Start transaction]
    StartTx --> ReleaseTickets[Release reserved tickets]
    ReleaseTickets --> UpdateInventory[Update available inventory]
    UpdateInventory --> MarkExpired[Mark reservation as expired]
    MarkExpired --> CommitTx[Commit transaction]
    CommitTx --> PublishEvent[Publish ReservationExpired event]
    PublishEvent --> LogCleanup[Log cleanup activity]
    LogCleanup --> ProcessNext{More reservations?}
    
    ProcessNext -->|✅ Yes| ProcessExpired
    ProcessNext -->|❌ No| Sleep
    
    Sleep --> QueryExpired
    
    %% Error handling
    StartTx --> TxError{Transaction error?}
    TxError -->|✅ Yes| RollbackTx[Rollback and retry]
    RollbackTx --> LogError[Log error]
    LogError --> Sleep
    TxError -->|❌ No| ReleaseTickets
    
    style ScheduledJob fill:#e1f5fe
    style ProcessExpired fill:#fff3e0
    style PublishEvent fill:#f3e5f5
    style LogError fill:#ffebee
```

## Metrics and Monitoring

### Booking Process KPIs

```mermaid
graph LR
    subgraph "Business Metrics"
        BookingRate[📊 Bookings/minute]
        ConversionRate[📈 Conversion Rate %]
        RevenueRate[💰 Revenue/hour]
        CancellationRate[❌ Cancellation Rate %]
    end
    
    subgraph "Performance Metrics"
        AvgLatency[⏱️ Average Latency]
        P95Latency[📊 P95 Latency]
        ErrorRate[❌ Error Rate %]
        ThroughputRPS[🚀 Throughput RPS]
    end
    
    subgraph "System Health"
        LockContention[🔒 Lock Contention]
        DeadlockRate[💀 Deadlock Rate]
        ExpiredReservations[⏰ Expired Rate %]
        InventoryAccuracy[✅ Inventory Accuracy]
    end
    
    subgraph "User Experience"
        TimeToBook[⏱️ Time to Book]
        PaymentLatency[💳 Payment Latency]
        NotificationDelay[📧 Notification Delay]
        MobilePerformance[📱 Mobile Performance]
    end
```

### Critical Alerts

| Metric | Threshold | Action |
|---------|-----------|---------|
| Error Rate | > 5% | Immediate investigation |
| P95 Latency | > 2s | Automatic scaling |
| Lock Contention | > 50% | Concurrency review |
| Inventory Mismatch | > 0 | Urgent reconciliation |
| Failed Payments | > 10% | Check external integration |

## Flow Testing

### Critical Test Cases

1. **Happy Path**: Successful booking with payment
2. **Insufficient Inventory**: Handling sold out tickets
3. **Concurrent Bookings**: Race conditions en alta demanda
4. **Payment Failures**: Retries and recovery
5. **Timeout Scenarios**: Reservation expiration
6. **Network Issues**: Resilience against network failures
7. **Database Failover**: Continuity during DB issues

### Performance Testing

```bash
# High demand load simulation
# 1000 usuarios concurrentes intentando reservar el mismo evento
k6 run booking-load-test.js \
  --vus 1000 \
  --duration 30s \
  --stage "0s:0,10s:1000,20s:1000,30s:0"
```

This flow ensures inventory integrity while providing a smooth and failure-resilient user experience.