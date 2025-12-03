using FluentValidation;

namespace JCP.TicketWave.BookingService.Domain.Validators;

public class TicketValidator : AbstractValidator<CreateTicketRequest>
{
    public TicketValidator()
    {
        RuleFor(x => x.EventId)
            .NotEqual(Guid.Empty)
            .WithMessage("Event ID is required");

        RuleFor(x => x.TicketType)
            .NotEmpty()
            .WithMessage("Ticket type is required")
            .MaximumLength(50)
            .WithMessage("Ticket type cannot exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative");

        RuleFor(x => x.SeatNumber)
            .MaximumLength(20)
            .WithMessage("Seat number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.SeatNumber));
    }
}

// Request model for validation
public record CreateTicketRequest(
    Guid EventId,
    string TicketType,
    decimal Price,
    string? SeatNumber = null);