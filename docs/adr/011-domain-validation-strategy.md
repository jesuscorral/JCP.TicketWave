# ADR-011: Domain Validation Strategy with FluentValidation

## Status
Accepted

## Date
2025-12-03

## Context
As our Domain-Driven Design architecture matured, we identified the need for a robust, consistent validation strategy across all microservices. The existing approach had several limitations:

### Problems with Manual Validations
- **Scattered validation logic**: 29+ manual validation instances across domain models using `throw new DomainException()`
- **Inconsistent error messages**: No standardized format for validation errors
- **Limited business rule expression**: Simple null/range checks without complex business rules
- **Difficult testing**: Validation logic embedded in constructors/factory methods
- **Poor maintainability**: Changes to validation rules required modifying domain entities

### Business Requirements
- **Complex business rules**: Email formats, currency validation, conditional logic
- **Localization support**: Multi-language error messages (future requirement)
- **Validation reuse**: Consistent rules across different contexts
- **Clear error reporting**: Structured validation feedback for APIs

## Decision

We adopt **FluentValidation** as our primary domain validation strategy with the following architectural principles:

### 1. Validator Classes in Domain Layer
```csharp
// Domain/Validators/BookingValidator.cs
public class BookingValidator : AbstractValidator<CreateBookingRequest>
{
    public BookingValidator()
    {
        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required")
            .EmailAddress().WithMessage("Customer email must be a valid email address");
            
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 50).WithMessage("Quantity must be between 1 and 50 tickets");
    }
}
```

### 2. Request DTOs for Validation
```csharp
// Domain/Validators/BookingValidator.cs
public record CreateBookingRequest(
    Guid EventId,
    Guid UserId, 
    string CustomerEmail,
    int Quantity,
    decimal TotalAmount,
    DateTime? ExpiresAt);
```

### 3. Factory Method Integration
```csharp
// Domain/Models/Booking.cs
public static Booking Create(
    Guid eventId, Guid userId, string customerEmail, 
    int quantity, decimal totalAmount, DateTime? expiresAt = null)
{
    // Validate using FluentValidation before entity creation
    var validationRequest = new CreateBookingRequest(
        eventId, userId, customerEmail, quantity, totalAmount, expiresAt);
        
    var validator = new BookingValidator();
    var validationResult = validator.Validate(validationRequest);
    
    if (!validationResult.IsValid)
    {
        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        throw new DomainException($"Booking validation failed: {errors}");
    }
    
    return new Booking(eventId, userId, customerEmail, quantity, totalAmount, expiresAt);
}
```

### 4. Dependency Injection Configuration
```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();
```

## Implementation Details

### Validator Architecture
Each service implements comprehensive validators:

#### BookingService
- **BookingValidator**: Email validation, quantity limits (1-50), amount validation
- **TicketValidator**: Seat validation, pricing rules, status verification

#### CatalogService  
- **EventValidator**: Date validation, URL validation, currency support (USD, EUR, MXN), capacity limits
- **CategoryValidator**: Name requirements, business rules
- **VenueValidator**: Address validation, capacity constraints, URL formatting

#### PaymentService
- **PaymentValidator**: Currency support (USD, EUR, MXN, JPY), amount limits (0.01-1,000,000)
- **PaymentMethodValidator**: Conditional card validation with expiry date logic
- **RefundValidator**: Refund amount validation, business rule compliance

### Business Rule Examples
```csharp
// Complex conditional validation
RuleFor(x => x.CardNumber)
    .NotEmpty().WithMessage("Card number is required")
    .When(x => x.Type == PaymentMethodType.CreditCard)
    .WithMessage("Card number is required for credit card payments");

// Custom business rules
RuleFor(x => x.Currency)
    .Must(BeValidCurrency).WithMessage("Currency must be USD, EUR, MXN, or JPY");
    
private static bool BeValidCurrency(string currency)
{
    var validCurrencies = new[] { "USD", "EUR", "MXN", "JPY" };
    return validCurrencies.Contains(currency?.ToUpper());
}
```

### Error Message Strategy
- **Descriptive messages**: Clear, actionable error descriptions
- **Business context**: Messages reflect domain understanding
- **Aggregated errors**: Multiple validation failures reported together
- **Consistent format**: Standardized error message patterns

## Benefits

### Architecture Benefits
- **ADR-002 Compliance**: Validators remain in domain layer maintaining clean architecture
- **ADR-009 Compatibility**: Works seamlessly with repository pattern
- **Single Responsibility**: Validators handle only validation concerns
- **Open/Closed Principle**: Easy to extend rules without modifying existing code

### Development Benefits
- **Centralized rules**: All validation logic in dedicated validator classes
- **Rich expression**: Complex business rules with fluent syntax
- **Easy testing**: Validators can be unit tested independently
- **Maintainability**: Changes to validation don't affect domain entities
- **IntelliSense support**: Strong typing with compile-time validation

### Business Benefits
- **Consistent validation**: Same rules applied across all contexts
- **Clear feedback**: Structured error messages for better UX
- **Business rule compliance**: Complex domain constraints properly expressed
- **Audit trail**: Validation failures properly logged and trackable

## Implementation Status

### ✅ Package Management
- FluentValidation 11.9.0 added to `Directory.Packages.props`
- FluentValidation.DependencyInjectionExtensions configured
- All service projects updated with package references

### ✅ Validator Implementation
- **8 comprehensive validators** created across all services
- **Business rules implemented**: Email, currency, conditional, range validations
- **Request DTOs defined**: Proper data structures for each validation scenario

### ✅ Domain Integration
- **Factory methods updated**: Booking.cs, Event.cs, Payment.cs
- **Error handling preserved**: Maintains existing `DomainException` pattern
- **Clean architecture maintained**: No infrastructure dependencies in domain

### ✅ Service Configuration
- **Dependency injection**: All services configured with automatic validator registration
- **Build verification**: All projects compile successfully
- **Architecture tests**: Existing tests still pass with new validation layer

## Consequences

### Positive
- **Robust validation**: Professional-grade validation with rich business rules
- **Better maintainability**: Centralized, testable validation logic
- **Improved error handling**: Structured, descriptive error messages
- **Architecture compliance**: Supports DDD principles and clean architecture
- **Development velocity**: Faster to implement and modify validation rules

### Trade-offs
- **Additional complexity**: More classes and abstractions per domain entity
- **Learning curve**: Team needs to understand FluentValidation syntax
- **Package dependency**: External dependency on FluentValidation library

### Migration Path
- **Backwards compatible**: Maintains existing `DomainException` pattern
- **Incremental adoption**: Can add validators to new entities gradually
- **No breaking changes**: Existing APIs continue to work unchanged

## Related ADRs
- ADR-002: Clean Architecture with Vertical Slices (architectural foundation)
- ADR-009: Repository Pattern Implementation (domain layer organization)
- ADR-010: Central Package Management (FluentValidation package management)

## Success Metrics
- ✅ **Validation coverage**: All domain factory methods use FluentValidation
- ✅ **Error consistency**: Standardized validation error messages
- ✅ **Build success**: All services compile without errors
- ✅ **Architecture compliance**: Domain layer remains pure with no infrastructure dependencies
- 🔲 **Test coverage**: Unit tests for all validators (future work)
- 🔲 **Performance**: Validation performance benchmarks (future measurement)