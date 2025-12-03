using JCP.TicketWave.CatalogService.Domain.Interfaces;

namespace JCP.TicketWave.CatalogService.Application.Features.Categories.GetCategories;

public class GetCategoriesHandler
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync();
        
        var categoryDtos = categories.Select(c => new CategoryDto(
            Id: c.Id,
            Name: c.Name,
            Description: c.Description ?? string.Empty, // Handle null description
            EventCount: 0 // TODO: Add event count calculation
        )).ToArray();

        return new GetCategoriesResponse(
            Categories: categoryDtos);
    }
}