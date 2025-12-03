# Architecture Decision Records (ADRs)

This directory contains all important architectural decisions for JCP.TicketWave.

## ADR Index

- [ADR-001: Microservices Architecture Adoption](001-microservices-architecture.md)
- [ADR-002: Clean Architecture with Vertical Slices](002-clean-architecture-vertical-slices.md)
- [ADR-003: Persistence Technologies per Service](003-persistence-technologies.md)
- [ADR-004: Service Communication Strategy](004-service-communication.md)
- [ADR-005: API Gateway as Entry Point](005-api-gateway.md)
- [ADR-006: State Management and Transactions Strategy](006-state-management.md)
- [ADR-007: Resilience and Fault Tolerance Patterns](007-resilience-patterns.md)
- [ADR-008: Security and Compliance](008-security-compliance.md)
- [ADR-009: Repository Pattern Implementation](009-repository-pattern-implementation.md)
- [ADR-010: Central Package Management Implementation](010-central-package-management.md)
- [ADR-011: Domain Validation Strategy](011-domain-validation-strategy.md)
- [ADR-012: Message Broker and Event Streaming](012-message-broker-event-streaming.md) ✨ **NEW**

## Recent Changes (December 2025)

### ADR-012: Message Broker and Event Streaming
**Status**: Accepted  
**Implements**: RabbitMQ-based event-driven architecture for cross-service communication
- ✅ Domain events → Integration events pattern
- ✅ RabbitMQ infrastructure with Docker setup
- ✅ Cross-service event handlers implementation
- ✅ Foundation for Saga pattern implementation

**Compliance Check**:
- ✅ ADR-001: Supports microservices independence
- ✅ ADR-002: Maintains clean architecture boundaries  
- ✅ ADR-004: Implements asynchronous communication strategy
- ✅ ADR-007: Adds resilience through message durability and retry patterns

## ADR Format

All ADRs follow the standard format:

1. **Title**: A short phrase describing the decision
2. **Status**: Proposed, Accepted, Rejected, Superseded, etc.
3. **Context**: The situation requiring a decision
4. **Decision**: The decision made
5. **Consequences**: Expected results of the decision
6. **Alternatives Considered**: Other options that were evaluated

## Architecture Compliance

### Validation Checklist
Before creating a PR, ensure all changes comply with existing ADRs:

- [ ] **ADR-001**: Service boundaries maintained, no coupling violations
- [ ] **ADR-002**: Domain layer purity, infrastructure dependencies managed
- [ ] **ADR-003**: Correct persistence technology used per service
- [ ] **ADR-004**: Communication patterns followed (sync/async as defined)
- [ ] **ADR-005**: API Gateway routing patterns respected
- [ ] **ADR-007**: Resilience patterns applied (retries, timeouts, circuit breakers)
- [ ] **ADR-008**: Security patterns maintained (authentication, authorization)

### Current Implementation Status
- ✅ **Event-Driven Architecture**: Complete implementation across all services
- ✅ **Cross-Service Communication**: RabbitMQ integration events working
- ✅ **Example Endpoints**: Testing infrastructure for integration flows
- 🚧 **Saga Patterns**: Foundation ready, implementation pending
- �� **Production Monitoring**: Basic setup complete, advanced monitoring pending

## Maintenance

- Each significant new architectural change must be documented as a new ADR
- Existing ADRs are not modified, but marked as "Superseded" if necessary
- All PR reviews must include ADR compliance check
- Architecture team reviews required for new ADRs
