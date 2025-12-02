using JCP.TicketWave.CatalogService.Domain.Models;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetByNameAsync(string name);
    Task<Category> AddAsync(Category entity);
    Task<Category> UpdateAsync(Category entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}