using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Review?> GetByBookingIdAsync(Guid bookingId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task<IReadOnlyList<Review>> GetByHotelIdAsync(Guid hotelId, int pageNumber, int pageSize)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByHotelIdAsync(Guid hotelId)
    {
        return await _dbSet
            .CountAsync(r => r.HotelId == hotelId);
    }

    public async Task<IReadOnlyList<int>> GetAllRatingsForHotelAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId)
            .Select(r => r.StarRating)
            .ToListAsync();
    }
}