namespace JCP.TicketWave.CatalogService.Application.Features.Categories.GetCategories;

public class GetCategoriesHandler
{
    // TODO: Implement repository pattern for NoSQL database
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        // Placeholder implementation
        await Task.Delay(10, cancellationToken);
        
        return new GetCategoriesResponse(
            Categories: Array.Empty<CategoryDto>());
    }
}