using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class RoomTypeRepository : BaseRepository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<RoomType?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(rt => rt.Id != excludeId.Value);
        }

        return await query.AnyAsync(rt => rt.Name == name);
    }

    public async Task<IReadOnlyList<RoomType>> GetByCapacityAsync(int adults, int children)
    {
        return await _dbSet
            .Where(rt => rt.MaxAdults >= adults && rt.MaxChildren >= children)
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RoomType>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(rt => rt.PricePerNight >= minPrice && rt.PricePerNight <= maxPrice)
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RoomType>> GetWithCapacityAndPriceRangeAsync(
        int adults,
        int children,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _dbSet
            .Where(rt => rt.MaxAdults >= adults && rt.MaxChildren >= children);

        if (minPrice.HasValue)
        {
            query = query.Where(rt => rt.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(rt => rt.PricePerNight <= maxPrice.Value);
        }

        return await query
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }
}