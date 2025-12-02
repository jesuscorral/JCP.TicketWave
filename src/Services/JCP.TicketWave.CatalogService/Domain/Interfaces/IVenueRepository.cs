using JCP.TicketWave.CatalogService.Domain.Models;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface IVenueRepository
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(Guid id);
    Task<IEnumerable<Venue>> GetByCityAsync(string city);
    Task<IEnumerable<Venue>> SearchVenuesAsync(string searchTerm);
    Task<Venue> AddAsync(Venue entity);
    Task<Venue> UpdateAsync(Venue entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}