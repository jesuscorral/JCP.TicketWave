namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class TicketTypeInfo
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalAvailable { get; set; }
    public int AvailableCount { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();

    public TicketTypeInfo() { }

    public TicketTypeInfo(string name, decimal price, int totalAvailable, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (totalAvailable < 0)
            throw new ArgumentException("Total available cannot be negative", nameof(totalAvailable));

        Name = name;
        Price = price;
        TotalAvailable = totalAvailable;
        AvailableCount = totalAvailable;
        Description = description;
    }

    public bool IsAvailable => AvailableCount > 0;
    public int SoldCount => TotalAvailable - AvailableCount;
    public decimal SalesPercentage => TotalAvailable > 0 ? (decimal)SoldCount / TotalAvailable * 100 : 0;
}