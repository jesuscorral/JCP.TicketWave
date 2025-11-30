namespace JCP.TicketWave.CatalogService.Features.Categories;

public static class GetCategories
{
    public record Query();

    public record Response(IEnumerable<CategoryDto> Categories);

    public record CategoryDto(
        Guid Id,
        string Name,
        string Description,
        int EventCount);

    public class Handler
    {
        // TODO: Implement repository pattern for NoSQL database
        public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
        {
            // Placeholder implementation
            await Task.Delay(10, cancellationToken);
            
            return new Response(
                Categories: Array.Empty<CategoryDto>());
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new Query();
            var result = await handler.Handle(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Categories")
        .WithSummary("Get all event categories");
    }
}