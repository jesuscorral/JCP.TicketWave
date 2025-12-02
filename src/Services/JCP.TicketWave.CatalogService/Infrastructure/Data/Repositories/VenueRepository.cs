using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.CatalogService.Domain.Entities;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Data;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly CatalogDbContext _context;

    public VenueRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _context.Venues
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(Guid id)
    {
        return await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == id && v.IsActive);
    }

    public async Task<IEnumerable<Venue>> GetByCityAsync(string city)
    {
        return await _context.Venues
            .Where(v => v.Address.City == city && v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venue>> SearchVenuesAsync(string searchTerm)
    {
        return await _context.Venues
            .Where(v => v.IsActive && 
                       (v.Name.Contains(searchTerm) || 
                        v.Address.Street.Contains(searchTerm) ||
                        v.Address.City.Contains(searchTerm)))
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue> AddAsync(Venue entity)
    {
        _context.Venues.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Venue> UpdateAsync(Venue entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Deactivate(); // Use domain method for soft delete
            await UpdateAsync(entity);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Venues
            .AnyAsync(v => v.Id == id && v.IsActive);
    }
}