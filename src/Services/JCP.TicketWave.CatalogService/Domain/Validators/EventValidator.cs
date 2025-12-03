using FluentValidation;

namespace JCP.TicketWave.CatalogService.Domain.Validators;

public class EventValidator : AbstractValidator<CreateEventRequest>
{
    public EventValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required")
            .MaximumLength(100)
            .WithMessage("Tenant ID cannot exceed 100 characters");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Event title is required")
            .MaximumLength(200)
            .WithMessage("Event title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDateTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Start date must be in the future")
            .LessThan(x => x.EndDateTime)
            .WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDateTime)
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID is required");

        RuleFor(x => x.VenueId)
            .NotEqual(Guid.Empty)
            .WithMessage("Venue ID is required");

        RuleFor(x => x.AvailableTickets)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available tickets cannot be negative")
            .LessThanOrEqualTo(100000)
            .WithMessage("Available tickets cannot exceed 100,000");

        RuleFor(x => x.TicketPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ticket price cannot be negative")
            .LessThanOrEqualTo(10000)
            .WithMessage("Ticket price cannot exceed $10,000");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.TicketPrice)
            .WithMessage("Max price must be greater than or equal to ticket price")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR)");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl)
            .WithMessage("Image URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ExternalUrl)
            .Must(BeAValidUrl)
            .WithMessage("External URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ExternalUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

// Request model for validation
public record CreateEventRequest(
    string TenantId,
    string Title,
    string? Description,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid CategoryId,
    Guid VenueId,
    int AvailableTickets,
    decimal TicketPrice,
    decimal? MaxPrice,
    string Currency,
    string? ImageUrl,
    string? ExternalUrl);