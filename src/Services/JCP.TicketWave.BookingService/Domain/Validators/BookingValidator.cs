using FluentValidation;

namespace JCP.TicketWave.BookingService.Domain.Validators;

public class BookingValidator : AbstractValidator<CreateBookingRequest>
{
    public BookingValidator()
    {
        RuleFor(x => x.EventId)
            .NotEqual(Guid.Empty)
            .WithMessage("Event ID is required");

        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(50)
            .WithMessage("Quantity cannot exceed 50 tickets per booking");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount cannot be negative");
    }
}

// Request model for validation
public record CreateBookingRequest(
    Guid EventId,
    Guid UserId,
    string CustomerEmail,
    int Quantity,
    decimal TotalAmount,
    DateTime? ExpiresAt = null);