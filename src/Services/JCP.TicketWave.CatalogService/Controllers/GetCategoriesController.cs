using JCP.TicketWave.CatalogService.Features.Categories.GetCategories;

namespace JCP.TicketWave.CatalogService.Controllers;

public static class GetCategoriesController
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
            GetCategoriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoriesQuery();
            var result = await handler.Handle(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Categories")
        .WithSummary("Get all event categories");
    }
}