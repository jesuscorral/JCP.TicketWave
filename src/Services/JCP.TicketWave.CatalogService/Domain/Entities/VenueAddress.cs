namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class VenueAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public VenueAddress() { }

    public VenueAddress(
        string street,
        string city,
        string state,
        string country,
        string postalCode,
        double? latitude = null,
        double? longitude = null)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string FullAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";
    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
}