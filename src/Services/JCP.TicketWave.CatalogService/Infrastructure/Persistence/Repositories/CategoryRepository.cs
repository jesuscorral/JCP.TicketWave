using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.CatalogService.Domain.Models;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower().Trim());
    }

    public async Task<Category> AddAsync(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Category> UpdateAsync(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Categories.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity != null)
        {
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }
}