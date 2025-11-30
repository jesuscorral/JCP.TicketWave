# Architecture Diagrams - TicketWave

This folder contains diagrams that complement the Architecture Decision Records (ADRs) documentation.

## Diagram Index

### 1. General Architecture
- **[system-architecture.md](./system-architecture.md)** - General system architecture diagram
- **[microservices-overview.md](./microservices-overview.md)** - Overview of microservices and their responsibilities

### 2. Inter-Service Communication
- **[service-communication.md](./service-communication.md)** - Synchronous and asynchronous communication patterns
- **[api-gateway-flow.md](./api-gateway-flow.md)** - Request flow through API Gateway

### 3. Business Flows
- **[booking-flow.md](./booking-flow.md)** - Complete ticket booking process
- **[payment-flow.md](./payment-flow.md)** - Payment processing flow

### 4. Architecture Patterns
- **[clean-architecture.md](./clean-architecture.md)** - Clean Architecture + Vertical Slices structure
- **[data-flow.md](./data-flow.md)** - Data flow between layers and services

### 5. Resilience and Security
- **[resilience-patterns.md](./resilience-patterns.md)** - Implemented resilience patterns
- **[security-layers.md](./security-layers.md)** - System security layers

## Diagram Conventions

### Format
- **Mermaid**: For diagrams integrated in markdown
- **PlantUML**: For more complex diagrams (comments included)
- **C4 Model**: For system architecture diagrams

### Symbology
- 🟦 **Blue**: Internal services (microservices)
- 🟨 **Yellow**: API Gateway and entry points
- 🟩 **Green**: Databases and storage
- 🟧 **Orange**: External and third-party services
- 🟥 **Red**: Critical security points

### Detail Levels
- **L1**: Complete system view (Context)
- **L2**: Container view (Services)
- **L3**: Component view (Internal structure)
- **L4**: Code view (Implementation details)

## Recommended Tools

### For Visualization
- **VS Code**: With Mermaid and PlantUML extensions
- **Draw.io**: For interactive diagrams
- **Lucidchart**: For team collaboration

### For Maintenance
- **Automated Documentation**: Generate diagrams from code
- **Version Control**: Keep synchronized with code changes
- **Review Process**: Validate diagrams in PRs

## Relationship with ADRs

Each diagram is linked to one or more ADRs:

| Diagram | Related ADRs | Purpose |
|----------|--------------|---------|
| system-architecture | ADR-001 | Justify microservices decision |
| clean-architecture | ADR-002 | Show code organization |
| service-communication | ADR-004 | Illustrate communication patterns |
| api-gateway-flow | ADR-005 | Explain gateway responsibilities |
| resilience-patterns | ADR-007 | Visualize resilience strategies |
| security-layers | ADR-008 | Map security defenses |

## Diagram Updates

### When to Update
- Changes in service architecture
- New communication patterns
- Modifications in business flows
- Changes in security strategies

### Update Process
1. **Identify Change**: What architectural aspect changed?
2. **Review ADRs**: Is a new architectural decision required?
3. **Update Diagram**: Reflect visual changes
4. **Validate Consistency**: Verify coherence with code
5. **Review**: Validation by architecture team