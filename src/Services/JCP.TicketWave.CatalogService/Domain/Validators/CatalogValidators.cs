using FluentValidation;

namespace JCP.TicketWave.CatalogService.Domain.Validators;

public class CategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class VenueValidator : AbstractValidator<CreateVenueRequest>
{
    public VenueValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Venue name is required")
            .MaximumLength(200)
            .WithMessage("Venue name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required")
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than zero")
            .LessThanOrEqualTo(200000)
            .WithMessage("Capacity cannot exceed 200,000");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
}

// Request models for validation
public record CreateCategoryRequest(
    string Name,
    string? Description);

public record CreateVenueRequest(
    string Name,
    string Address,
    string City,
    string Country,
    int Capacity,
    string? PostalCode = null);