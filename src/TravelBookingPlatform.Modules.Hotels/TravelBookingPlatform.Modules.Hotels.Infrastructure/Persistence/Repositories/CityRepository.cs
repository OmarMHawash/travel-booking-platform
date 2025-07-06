using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class CityRepository : BaseRepository<City>, ICityRepository
{
    public CityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<City?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(c => c.Name == name);
    }

    public async Task<bool> PostCodeExistsAsync(string postCode, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(c => c.PostCode == postCode);
    }

    public async Task<IReadOnlyList<City>> GetCitySuggestionsAsync(string searchText, int maxResults = 10)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(searchTerm) || c.Country.ToLower().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<City>> GetPopularDestinationsAsync(int count = 5)
    {
        // For now, return cities with the most hotels. In a real implementation,
        // this could be based on booking frequency, user searches, etc.
        return await _dbSet
            .Include(c => c.Hotels)
            .OrderByDescending(c => c.Hotels.Count)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<City>> SearchCitiesAsync(string searchText)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(searchTerm) || c.Country.ToLower().Contains(searchTerm))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}