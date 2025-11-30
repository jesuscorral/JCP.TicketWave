# JCP.TicketWave - Sistema de Gestión de Eventos y Venta de Entradas

## Descripción

JCP.TicketWave es una solución de microservicios desarrollada en .NET 10 para la gestión de eventos y venta de entradas de alta demanda, similar a Ticketmaster o Eventbrite pero simplificado. 

## Arquitectura

La solución utiliza **Clean Architecture** con **Vertical Slices** para organizar el código de manera eficiente y mantenible.

### Microservicios

1. **Catalog Service** (Puerto 7001)
   - **Propósito**: Gestión del catálogo de eventos
   - **Características**: Lectura intensiva, optimizado para caché y NoSQL
   - **Endpoints**:
     - `GET /api/events` - Lista de eventos con paginación y filtros
     - `GET /api/events/{id}` - Detalles de un evento específico
     - `GET /api/categories` - Categorías de eventos

2. **Booking Service** (Puerto 7002)
   - **Propósito**: Gestión de reservas y tickets
   - **Características**: Escritura crítica, SQL Server, transacciones ACID, manejo de bloqueos
   - **Endpoints**:
     - `POST /api/bookings` - Crear nueva reserva
     - `GET /api/bookings/{id}` - Obtener detalles de reserva
     - `POST /api/tickets/reserve` - Reservar tickets temporalmente

3. **Payment Service** (Puerto 7003)
   - **Propósito**: Procesamiento de pagos y reembolsos
   - **Características**: Integración con terceros (Stripe/PayPal), idempotencia
   - **Endpoints**:
     - `POST /api/payments` - Procesar pago
     - `GET /api/payments/{id}` - Estado del pago
     - `POST /api/refunds` - Procesar reembolso

4. **Notification Service** (Puerto 7004)
   - **Propósito**: Envío de notificaciones y generación de PDFs
   - **Características**: Worker service, procesamiento en background, email, PDFs
   - **Funciones**:
     - Envío de emails de confirmación
     - Generación de tickets en PDF
     - Procesamiento de colas de mensajes

5. **API Gateway** (Puerto 7000)
   - **Propósito**: Punto de entrada unificado para todos los servicios
   - **Características**: Enrutamiento, agregación de servicios, health checks
   - **Funciones**:
     - Proxy a microservicios
     - Health check consolidado
     - CORS y configuración centralizada

## Estructura del Proyecto

```
JCP.TicketWave/
├── src/
│   ├── Gateway/
│   │   └── JCP.TicketWave.Gateway/          # API Gateway
│   ├── Services/
│   │   ├── JCP.TicketWave.CatalogService/   # Servicio de Catálogo
│   │   ├── JCP.TicketWave.BookingService/   # Servicio de Reservas
│   │   ├── JCP.TicketWave.PaymentService/   # Servicio de Pagos
│   │   └── JCP.TicketWave.NotificationService/ # Servicio de Notificaciones
│   └── Shared/
│       ├── JCP.TicketWave.Shared.Contracts/ # Contratos compartidos
│       └── JCP.TicketWave.Shared.Infrastructure/ # Infraestructura compartida
└── tests/ (Estructura preparada para tests)
```

### Estructura por Servicio (Clean Architecture + Vertical Slices)

Cada servicio sigue la misma estructura:

```
ServiceName/
├── Features/           # Vertical Slices organizados por funcionalidad
│   ├── FeatureName/
│   │   ├── Command.cs  # Comandos (escritura)
│   │   ├── Query.cs    # Consultas (lectura)
│   │   └── Handler.cs  # Lógica de negocio
├── Domain/            # Entidades de dominio
├── Infrastructure/    # Implementaciones de infraestructura
└── Program.cs         # Configuración del servicio
```

## Patrones Implementados

### 1. Vertical Slice Architecture
- Cada feature está completamente autocontenida
- Reduce acoplamiento entre funcionalidades
- Facilita el desarrollo en paralelo

### 2. CQRS (Command Query Responsibility Segregation)
- Separación clara entre comandos y consultas
- Optimización independiente para lectura y escritura

### 3. Domain Events
- Comunicación asíncrona entre servicios
- Infraestructura preparada para eventos de dominio

### 4. Repository Pattern
- Abstracción de acceso a datos
- Facilita testing y cambios de implementación

## Tecnologías y Características

### Tecnologías Base
- **.NET 10** - Framework principal
- **ASP.NET Core** - APIs y Web services
- **Minimal APIs** - Para servicios simples y Gateway
- **Worker Services** - Para procesamiento background

### Características de Cada Servicio

#### Catalog Service
- **NoSQL optimizado** - Preparado para MongoDB/CosmosDB
- **Caché distribuido** - Redis para alta performance
- **Read-heavy** - Optimizado para consultas frecuentes

#### Booking Service
- **SQL Server** - Para transacciones ACID
- **Pessimistic Locking** - Para manejo de concurrencia
- **Unit of Work** - Para transacciones complejas

#### Payment Service
- **Idempotencia** - Prevención de pagos duplicados
- **Retry Logic** - Manejo de fallos temporales
- **Third-party Integration** - Stripe, PayPal, etc.

#### Notification Service
- **Message Queues** - Azure Service Bus, RabbitMQ
- **Email Services** - SendGrid, SMTP
- **PDF Generation** - iTextSharp, PdfSharp

## Configuración y Ejecución

### Prerrequisitos
- .NET 10 SDK
- Visual Studio 2022 / VS Code
- SQL Server (para Booking Service)
- Redis (para Catalog Service cache)

### Compilar la Solución
```bash
dotnet build
```

### Ejecutar Servicios Individualmente

#### Gateway (Puerto 7000)
```bash
cd src/Gateway/JCP.TicketWave.Gateway
dotnet run
```

#### Catalog Service (Puerto 7001)
```bash
cd src/Services/JCP.TicketWave.CatalogService
dotnet run
```

#### Booking Service (Puerto 7002)
```bash
cd src/Services/JCP.TicketWave.BookingService
dotnet run
```

#### Payment Service (Puerto 7003)
```bash
cd src/Services/JCP.TicketWave.PaymentService
dotnet run
```

#### Notification Service (Background)
```bash
cd src/Services/JCP.TicketWave.NotificationService
dotnet run
```

### Health Checks

- **Gateway**: `https://localhost:7000/health`
- **Servicios consolidados**: `https://localhost:7000/health/services`
- **Servicios individuales**: `https://localhost:700X/health`

### Documentación API

Cada servicio expone documentación Swagger:
- **Gateway**: `https://localhost:7000/swagger`
- **Catalog**: `https://localhost:7001/swagger`
- **Booking**: `https://localhost:7002/swagger`
- **Payment**: `https://localhost:7003/swagger`

## Próximos Pasos (TODOs)

### Implementaciones Pendientes

1. **Persistence Layer**
   - Entity Framework Core para Booking Service
   - MongoDB driver para Catalog Service
   - Redis cache implementation

2. **Message Queues**
   - Azure Service Bus integration
   - Event-driven communication entre servicios

3. **Authentication & Authorization**
   - JWT tokens
   - Identity Service
   - API Gateway authentication

4. **Monitoring & Logging**
   - Application Insights
   - Structured logging
   - Distributed tracing

5. **Testing**
   - Unit tests para cada feature
   - Integration tests
   - Load testing para alta demanda

6. **DevOps**
   - Docker containers
   - Kubernetes deployment
   - CI/CD pipelines

## Escalabilidad

La arquitectura está diseñada para soportar alta demanda:

- **Horizontal Scaling**: Cada servicio puede escalarse independientemente
- **Database Optimization**: NoSQL para lecturas, SQL para transacciones críticas
- **Caching Strategy**: Redis para datos frecuentemente accedidos
- **Event-Driven**: Comunicación asíncrona para reducir acoplamiento
- **Load Balancing**: Gateway como punto único de entrada

## Contribución

Este proyecto utiliza las mejores prácticas de desarrollo:
- Clean Code principles
- SOLID principles
- Domain Driven Design (DDD)
- Test Driven Development (TDD) - preparado para implementar
- Continuous Integration/Deployment - preparado para implementar