using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class DealRepository : BaseRepository<Deal>, IDealRepository
{
    public DealRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Deal>> GetFeaturedDealsAsync(int count = 10)
    {
        return await _dbSet
            .Where(d => d.IsFeatured && d.IsActive && d.ValidTo > DateTime.UtcNow)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Deal>> GetActiveDealsAsync()
    {
        return await _dbSet
            .Where(d => d.IsActive && d.ValidTo > DateTime.UtcNow)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .ToListAsync();
    }

    public async Task<List<Deal>> GetDealsByHotelAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId && d.IsActive)
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .OrderBy(d => d.ValidTo)
            .ToListAsync();
    }

    public async Task<Deal?> GetDealWithDetailsAsync(Guid dealId)
    {
        return await _dbSet
            .Include(d => d.Hotel).ThenInclude(h => h.City)
            .Include(d => d.RoomType)
            .FirstOrDefaultAsync(d => d.Id == dealId);
    }

    public async Task<bool> HasActiveDealForRoomTypeAsync(Guid roomTypeId)
    {
        return await _dbSet
            .AnyAsync(d => d.RoomTypeId == roomTypeId && d.IsActive && d.ValidTo > DateTime.UtcNow);
    }

    public async Task<Deal?> GetOverlappingDealAsync(Guid hotelId, Guid roomTypeId, DateTime validFrom, DateTime validTo)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId
                     && d.RoomTypeId == roomTypeId
                     && d.IsActive
                     && ((d.ValidFrom <= validFrom && d.ValidTo >= validFrom) ||
                         (d.ValidFrom <= validTo && d.ValidTo >= validTo) ||
                         (d.ValidFrom >= validFrom && d.ValidTo <= validTo)))
            .FirstOrDefaultAsync();
    }

    public async Task<Deal?> GetByHotelAndTitleAsync(Guid hotelId, string title)
    {
        return await _dbSet
            .Where(d => d.HotelId == hotelId
                     && d.Title == title
                     && d.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetActiveFeaturedDealsCountAsync(Guid hotelId)
    {
        return await _dbSet
            .CountAsync(d => d.HotelId == hotelId
                          && d.IsFeatured
                          && d.IsActive
                          && d.ValidTo > DateTime.UtcNow);
    }
}