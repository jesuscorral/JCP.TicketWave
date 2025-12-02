namespace JCP.TicketWave.CatalogService.Application.Features.Categories.GetCategories;

public record GetCategoriesResponse(IEnumerable<CategoryDto> Categories);

public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    int EventCount);