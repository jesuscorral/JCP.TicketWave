# RabbitMQ Integration Guide

## 🐰 TicketWave RabbitMQ Setup

### Local Development

#### Prerequisites
- Docker Desktop installed and running
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code

#### Quick Start

1. **Start RabbitMQ**:
   ```bash
   # Windows
   scripts\start-rabbitmq.bat
   
   # Linux/MacOS  
   chmod +x scripts/start-rabbitmq.sh
   ./scripts/start-rabbitmq.sh
   ```

2. **Verify Installation**:
   - Management UI: http://localhost:15672
   - Username: `admin`
   - Password: `admin123`

3. **Run Services**:
   ```bash
   # Start all services
   dotnet build
   
   # Run individual services
   dotnet run --project src/Services/JCP.TicketWave.BookingService
   dotnet run --project src/Services/JCP.TicketWave.CatalogService
   dotnet run --project src/Services/JCP.TicketWave.PaymentService
   ```

### Configuration

#### Local Development (appsettings.json)
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/ticketwave",
    "Username": "admin", 
    "Password": "admin123",
    "Exchange": "events.topic",
    "DeadLetterExchange": "events.dlx",
    "QueuePrefix": "events.",
    "MaxRetries": 3,
    "ConnectionTimeout": 30,
    "MessageTtl": 86400000,
    "UseSsl": false
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "RabbitMQ": {
    "Host": "#{RabbitMQ.Host}#",
    "Port": 5672,
    "VirtualHost": "/ticketwave", 
    "Username": "#{RabbitMQ.Username}#",
    "Password": "#{RabbitMQ.Password}#",
    "Exchange": "events.topic",
    "DeadLetterExchange": "events.dlx",
    "QueuePrefix": "events.",
    "MaxRetries": 3,
    "ConnectionTimeout": 30,
    "MessageTtl": 86400000,
    "UseSsl": true
  }
}
```

### Architecture

#### Event Flow
```
BookingService -> RabbitMQ -> CatalogService
                           -> PaymentService  
                           -> NotificationService
```

#### Exchanges and Queues
- **Exchange**: `events.topic` (Topic exchange for routing)
- **Dead Letter**: `events.dlx` (Failed message handling)
- **Queues**:
  - `events.booking.created`
  - `events.booking.confirmed`
  - `events.booking.cancelled`
  - `events.payment.completed`
  - `events.payment.failed`
  - `events.event.created`
  - `events.event.cancelled`

### Integration Events

#### Publishing Events
```csharp
// In domain event handler
public async Task HandleAsync(BookingCreatedIntegrationEvent domainEvent, CancellationToken cancellationToken)
{
    await _integrationEventBus.PublishAsync(domainEvent, cancellationToken);
}
```

#### Subscribing to Events  
```csharp
// In Program.cs startup
public async Task StartAsync(CancellationToken cancellationToken)
{
    await _integrationEventBus.SubscribeAsync<BookingCreatedIntegrationEvent>(
        async (evt) => await _handler.HandleAsync(evt), 
        cancellationToken);
}
```

### Production Deployment

#### Azure Container Instances
```yaml
# docker-compose.production.yml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASS}
    ports:
      - "5672:5672"
      - "15672:15672"
```

#### Azure Service Bus Alternative
For enterprise scenarios, consider Azure Service Bus:
```csharp
// Alternative implementation
services.AddAzureServiceBus(configuration.GetConnectionString("ServiceBus"));
```

### Monitoring & Troubleshooting

#### Health Checks
```bash
# Check RabbitMQ status
docker exec ticketwave-rabbitmq rabbitmq-diagnostics check_running

# View logs
docker logs ticketwave-rabbitmq

# Management UI metrics
curl -u admin:admin123 http://localhost:15672/api/queues
```

#### Common Issues
1. **Connection Refused**: Ensure Docker is running and RabbitMQ container is healthy
2. **Queue Not Found**: Check definitions.json configuration  
3. **Permission Denied**: Verify virtual host and user permissions
4. **Message TTL**: Check message expiration settings

### Security

#### Production Security
- Use strong passwords
- Enable TLS/SSL (`UseSsl: true`)
- Configure firewall rules
- Use Azure Key Vault for secrets
- Enable audit logging

#### Network Configuration
```bash
# Docker network for services
docker network create ticketwave-network
```

### Performance Tuning

#### Connection Pooling
```csharp
// Optimize connection settings
factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(30);
factory.AutomaticRecoveryEnabled = true;
factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
```

#### Queue Configuration
- Set appropriate TTL for messages (24 hours default)
- Configure dead letter handling
- Use durable queues for persistence
- Monitor queue depth and consumer lag

### Development Tips

1. **Use Management UI** for debugging queue states
2. **Check logs** regularly for connection issues  
3. **Test locally** before deploying
4. **Monitor memory usage** in production
5. **Implement retry logic** for transient failures